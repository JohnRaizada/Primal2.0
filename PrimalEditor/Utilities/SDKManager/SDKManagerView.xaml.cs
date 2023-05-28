using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for SDKManagerView.xaml
    /// </summary>
    public partial class SDKManagerView : Window
    {
        private static readonly HashSet<ExpanderListMenuItem> _deletedItems = new();
        private static readonly HashSet<ExpanderListMenuItem> _addedItems = new();
        private static double? _originalDataSource;
        private static bool? _showObsoletePackages;
        private static bool? _originalDisplayState;
        /// <summary>
        /// Initializes a new instance of the <see cref="SDKManagerView"/> class.
        /// </summary>
        public SDKManagerView()
        {
            InitializeComponent();
            ForceCursor = true;
            Loaded += async (sender, e) =>
            {
                SDKManager.Instance.UpdateInstance();
                SDKManager.Instance.GenerateAndroidUpdateSitesContent();
                await SDKManager.Instance.GenerateAndroidPlatformPackagesAsync();
                await SDKManager.Instance.GenerateAndroidToolsPackagesAsync();
                if (SDKManager.Instance.IsAutoSyncEnabled == true)
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
                    {
                        syncingBar.Visibility = Visibility.Visible;
                        await SDKManager.Instance.SyncDataAsync();
                        syncingBar.Visibility = Visibility.Collapsed;
                    }));
                }
                DataContext = SDKManager.Instance;
                SDKManager.Instance.IsContentAvailable = true;
                _originalDisplayState = SDKManager.Instance.ShowObsoletePackages;
            };
        }
        private void OnSDKManager_Android_EditButton_Click(object sender, RoutedEventArgs e) => SDKPath.IsToggled = true;
        private void OnSDKManager_DismissButton_Click(object sender, RoutedEventArgs e) => Close();
        private void OnSDKManager_OkButton_Click(object sender, RoutedEventArgs e)
        {
            OnSDKManager_ApplyButton_Click(sender, e);
            Close();
        }
        private void OnSDKManager_ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            // A true means to be downloaded, and deleted otherwise
            var itemsChanged = new Dictionary<ExpanderListMenuItem, bool>();
            foreach (ExpanderListMenuItem item in _deletedItems) itemsChanged.Add(item, false);
            foreach (ExpanderListMenuItem item in _addedItems) itemsChanged.Add(item, true);
            if (itemsChanged.Count <= 0) return;
            Downloads.Instance.ModifiedItems = itemsChanged;
            SDKConfirmationWindow confirmationWindow = new();
            confirmationWindow.ShowDialog();
        }
        private void RadioButton_Toggled(object sender, RoutedEventArgs e, ExpanderListView listView, RadioButton radioButtonToCheck)
        {
            if (sender is not RadioButton radioButton) return;
            if (listView == null) return;
            if (radioButton == radioButtonToCheck) listView.ContentViewMode = radioButtonToCheck.IsChecked == true ? ContentViewMode.List : ContentViewMode.Expander;
            else listView.ContentViewMode = radioButtonToCheck.IsChecked == true ? ContentViewMode.Expander : ContentViewMode.List;
        }
        private void Platform_RadioButton_Toggled(object sender, RoutedEventArgs e) => RadioButton_Toggled(sender, e, platformListView, listViewPlatformRadioButton);
        private void Tools_RadioButton_Toggled(object sender, RoutedEventArgs e) => RadioButton_Toggled(sender, e, toolsListView, listViewToolsRadioButton);
        private void UpdateSites_RadioButton_Toggled(object sender, RoutedEventArgs e) => RadioButton_Toggled(sender, e, updateSitesListView, updateSitesListViewRadioButton);
        private async void OnSDKManager_Android_Sync_Click(object sender, RoutedEventArgs e)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                syncingBar.Visibility = Visibility.Visible;
                await SDKManager.Instance.SyncDataAsync();
                syncingBar.Visibility = Visibility.Collapsed;
                SDKManager.Instance.IsNotificationReloadButtonVisible = true;
                SDKManager.Instance.NotificationText = "Content may have changed. Please refresh the page to see the updated content!";
                SDKManager.Instance.IsNotificationTextVisible = true;
            }));
        }
        private void OnSDKManager_AutoSyncCheckbox_Toggled(object sender, RoutedEventArgs e)
        {
            SDKManager.Instance.IsAutoSyncEnabled = autoSyncCheckBox.IsChecked;
            SDKManager.Instance.Save();
        }
        private void OnSDKManager_Android_SDKLocationBox_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            var validity = ContentHelper.VerifyDirectory(e.Value);
            if (validity == string.Empty)
            {
                SDKManager.Instance.IsNotificationTextVisible = false;
                SDKManager.Instance.AndroidSDKLocation = e.Value;
                SDKManager.Instance.Save();
                return;
            }
            SDKManager.Instance.NotificationText = validity;
            SDKManager.Instance.IsNotificationTextVisible = true;
        }
        private void OnSDKManager_WarningButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("Beware! Enabling Auto Sync or Manually Syncing while in online mode and doing this too frequently, may lead to the servers blocking the ip address. And thus leading the app paralyzed and unable to check for updates 'EVER'; until google servers unblock us!! Remember: The app is using web scraping to retrieve data due to the absence of an API, so excessive scrapping will utilize more server and device resources!!!", "Please be mindful!!!!", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
        private void OnSDKManager_PlatformListView_CheckBoxChanged(object sender, CheckBoxChangedEventArgs e)
        {
            if (_addedItems.Contains(e.MenuItem)) _addedItems.Remove(e.MenuItem);
            else if (_deletedItems.Contains(e.MenuItem)) _deletedItems.Remove(e.MenuItem);
            else if (e.IsChecked) _addedItems.Add(e.MenuItem);
            else _deletedItems.Add(e.MenuItem);
        }
        private void OnSDKManager_ToolsListView_CheckBoxChanged(object sender, CheckBoxChangedEventArgs e)
        {
            if (_addedItems.Contains(e.MenuItem)) _addedItems.Remove(e.MenuItem);
            else if (_deletedItems.Contains(e.MenuItem)) _deletedItems.Remove(e.MenuItem);
            else if (e.IsChecked) _addedItems.Add(e.MenuItem);
            else _deletedItems.Add(e.MenuItem);
        }
        private void OnSDKManager_DataSourceSwitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SDKManager.Instance.DataSource = e.NewValue == 1 ? DataSourceType.Local : DataSourceType.Web;
            SDKManager.Instance.Save();
        }
        private void OnSDKManager_ReloadButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
            SDKManagerView managerView = new();
            managerView.ShowDialog();
            SDKManager.Instance.IsNotificationReloadButtonVisible = false;
            SDKManager.Instance.IsNotificationTextVisible = false;
            _originalDataSource = dataSourceSwitch.Value;
        }
        private void OnSDKManager_Android_ObsoletePackagesCheckbox_Toggled(object sender, RoutedEventArgs e)
        {
            if (sender is not CheckBox checkBox) return;
            _showObsoletePackages = checkBox.IsChecked;
            if (showPlatformObsoletePackages != null) showPlatformObsoletePackages.IsChecked = _showObsoletePackages;
            if (showToolsObsoletePackages != null) showToolsObsoletePackages.IsChecked = _showObsoletePackages;
            if (_showObsoletePackages == _originalDisplayState)
            {
                SDKManager.Instance.IsNotificationTextVisible = false;
                SDKManager.Instance.IsNotificationRefreshButtonVisible = false;
                return;
            }
            SDKManager.Instance.NotificationText = "The content needs to be regenerated for this change to show effect.";
            SDKManager.Instance.IsNotificationTextVisible = true;
            SDKManager.Instance.IsNotificationRefreshButtonVisible = true;
        }
        private void OnSDKManager_RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                SDKManager.Instance.ShowObsoletePackages = _showObsoletePackages;
                syncingBar.Visibility = Visibility.Visible;
                await SDKManager.Instance.SyncDataAsync();
                syncingBar.Visibility = Visibility.Collapsed;
                Close();
                _originalDisplayState = SDKManager.Instance.ShowObsoletePackages;
                SDKManager.Instance.IsNotificationTextVisible = false;
                SDKManager.Instance.IsNotificationRefreshButtonVisible = false;
                SDKManagerView managerView = new();
                managerView.ShowDialog();
            }));
        }
        private void OnSDKManager_InfoButton_Click(object sender, RoutedEventArgs e) => MessageBox.Show("The manager does not feature checking for the status of installation, or the relevance of packages in online mode due to the lack of incentive required to invest some significant effort into the feature. Thus, it may or may not be added to the feature in the later point in time depending on the necessity at that point of time.", "There's a feature missing", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
    }
    /// <summary>
    /// Generates a foreground color for dynamically changing progress bar based on given values.
    /// </summary>
    /// <remarks>
    /// This returns a RGB SolidColorBrush.
    /// Takes the value of progress as a double argument.
    /// Takes the maximum value as a double parameter.
    /// Takes the minimum value as a double parameter.
    /// Ultimately returns bright red when progress is minimum and bright green when progress is maximum.
    /// </remarks>
    public class ProgressToForegroundConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double progress) return GenerateProgressBarForeground(progress);
            return Binding.DoNothing;
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        private static SolidColorBrush GenerateProgressBarForeground(double progress, double maximum = 100, double minimum = 0)
        {
            return new SolidColorBrush(Color.FromRgb((byte)(255 - ((progress / (maximum - minimum)) * 255)), (byte)((progress / (maximum - minimum)) * 255), 255));
        }
    }
}