using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
	/// <summary>
	/// Interaction logic for OpenProjectView.xaml
	/// </summary>
	public partial class OpenProjectView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenProjectView"/> class.
        /// </summary>
        public OpenProjectView()
		{
			InitializeComponent();
			Loaded += (s, e) =>
			{
				var item =
					projectsListBox.ItemContainerGenerator.ContainerFromItem(projectsListBox.SelectedIndex) as
						ListBoxItem;
				item?.Focus();
			};
		}

		private void OnOpen_Button_Click(object sender, RoutedEventArgs e)
		{
			OpenSelectedProject();
		}
		private void OnListBoxItem_Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			OpenSelectedProject();
		}
		private void OpenSelectedProject()
		{
			var project = OpenProject.Open((ProjectData)projectsListBox.SelectedItem);
			bool dialogResult = false;
			var win = Window.GetWindow(this);
			if (project != null)
			{
				dialogResult = true;
				win.DataContext = project;
			}
			win.DialogResult = dialogResult;
			win.Close();
		}
	}
}
