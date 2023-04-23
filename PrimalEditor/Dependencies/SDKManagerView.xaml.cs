using PrimalEditor.GameProject;
using System.Collections.ObjectModel;
using System.Windows;

namespace PrimalEditor.Dependencies
{
    /// <summary>
    /// Interaction logic for SDKManagerView.xaml
    /// </summary>
    public partial class SDKManagerView : Window
    {
        public SDKManagerView()
        {
            InitializeComponent();
            DataContext = Project.Current;
        }

        private void OnSDKManager_Android_EditButton_Click(object sender, RoutedEventArgs e)
        {
            SDKPath.IsToggled = true;
        }

        private void OnSDKManager_Android_SDKPath_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SDKPath.IsToggled = true;
        }
        private void OnSDKManager_DismissButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnSDKManager_ActionButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
    public class AndroidSdkDetails
    {
        public ObservableCollection<Platform> Platforms { get; set; }
        public ObservableCollection<Tool> Tools { get; set; }
        public ObservableCollection<UpdateSite> UpdateSites { get; set; }
    }

    public class Platform
    {
        public string Name { get; set; }
        public string APILevel { get; set; }
        public string Revision { get; set; }
        public string Status { get; set; }
    }

    public class Tool
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string Status { get; set; }
    }

    public class UpdateSite
    {
        public string Name { get; set; }
        public string URL { get; set; }
    }
}
