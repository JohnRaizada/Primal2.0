using System.Windows;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for SDKConfirmationWindow.xaml
    /// </summary>
    public partial class SDKConfirmationWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SDKConfirmationWindow"/> class.
        /// </summary>
        public SDKConfirmationWindow()
        {
            InitializeComponent();
            Loaded += (sender, e) =>
            {
                Downloads.Instance.GenerateDeleteQueue();
                Downloads.Instance.GenerateDownloadQueue();
            };
            DataContext = Downloads.Instance;
        }
        private void OnSDKConfirmation_ProccedButton_Click(object sender, RoutedEventArgs e)
        {
            SDKDownloadsView downloadsView = new();
            downloadsView.ShowDialog();
            Close();
            SDKManager.Instance.NotificationText = "Its probable that the items has changed and the data needs to be updated.";
            SDKManager.Instance.IsNotificationTextVisible = true;
            SDKManager.Instance.IsNotificationRefreshButtonVisible = true;
        }
        private void OnSDKConfirmation_CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
