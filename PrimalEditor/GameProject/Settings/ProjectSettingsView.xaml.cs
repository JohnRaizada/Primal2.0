using System.Windows;
using System.Windows.Input;

namespace PrimalEditor.GameProject.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class ProjectSettingsView : Window
    {
        public ProjectSettingsView()
        {
            InitializeComponent();
            ForceCursor = true;
        }
        private void OnProjectSettings_Player_DoneButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnProjectSettings_Player_Icon_EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnProjectSettings_Player_Icon_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}