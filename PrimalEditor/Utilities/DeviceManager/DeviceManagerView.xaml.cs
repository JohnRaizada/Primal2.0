using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities.DeviceManager
{
    /// <summary>
    /// Interaction logic for DeviceManagerView.xaml
    /// </summary>
    public partial class DeviceManagerView : Window
    {
        public DeviceManagerView()
        {
            InitializeComponent();
            DataContext = DeviceManager.Instance;
        }
        private void OnDeviceManager_DeviceTypeSwitch_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }
        private void OnDeviceManager_NewDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            switch (button.Content)
            {
                case "Create Device":
                    CreateDeviceView createDeviceView = new CreateDeviceView();
                    createDeviceView.ShowDialog();
                    break;
                case "Pair Using Wifi":
                    break;
                default: break;
            }
        }
    }
}
