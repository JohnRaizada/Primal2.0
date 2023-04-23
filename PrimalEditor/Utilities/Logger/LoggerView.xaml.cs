using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for LoggerView.xaml
    /// </summary>
    public partial class LoggerView : UserControl
    {
        public LoggerView()
        {
            InitializeComponent();
        }

        private void OnClear_Button_Click(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
            var floatingWindow = new Window
            {
                Title = "Floating Window",
                Width = 300,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var textBlock = new TextBlock { Text = "Hello, World!" };
            textBlock.Style = (Style)Application.Current.FindResource("LightTextBlockStyle");

            floatingWindow.Content = textBlock;
            floatingWindow.Show();
        }

        private void OnMessageFilter_Button_Click(object sender, RoutedEventArgs e)
        {
            var filter = 0x0;
            if (toggleInfo.IsChecked == true) filter |= (int)MessageType.Info;
            if (toggleWarnings.IsChecked == true) filter |= (int)MessageType.Warning;
            if (toggleErrors.IsChecked == true) filter |= (int)MessageType.Error;
            Logger.SetMessageFilter(filter);
        }
    }
}
