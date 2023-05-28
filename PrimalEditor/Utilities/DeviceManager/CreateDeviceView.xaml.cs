using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace PrimalEditor.Utilities.DeviceManager
{
    /// <summary>
    /// Interaction logic for CreateDeviceView.xaml
    /// </summary>
    public partial class CreateDeviceView : Window
    {
        private readonly CubicEase _easing = new() { EasingMode = EasingMode.EaseInOut };
        public CreateDeviceView()
        {
            InitializeComponent();
            DataContext = DeviceManager.Instance;
        }
        private void OnCreateDevice_CreateHardwareProfileButton_Click(object sender, RoutedEventArgs e)
        {
            NewHardwareProfileView newHardwareProfileView = new NewHardwareProfileView();
            newHardwareProfileView.ShowDialog();
        }
        private void OnCreateDevice_ImportHardwareProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void OnCreateDevice_NewHardwareProfileButton_Click(object sender, RoutedEventArgs e)
        {
        }
        private void OnCreateDevice_PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (configurationView.Margin.Equals(new Thickness(0))) return;
            configurationView.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(-1600, 0, 0, 0), new Thickness(0), new Duration(TimeSpan.FromSeconds(0.5)))
            {
                EasingFunction = _easing
            });
        }
        private void OnCreateDevice_CancelButton_Click(object sender, RoutedEventArgs e) => Close();
        private void OnCreateDevice_ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (configurationView.Margin.Equals(new Thickness(-1600, 0, 0, 0))) return;
            configurationView.BeginAnimation(MarginProperty, new ThicknessAnimation(new Thickness(0), new Thickness(-1600, 0, 0, 0), new Duration(TimeSpan.FromSeconds(0.5)))
            {
                EasingFunction = _easing
            });
        }
    }
}
