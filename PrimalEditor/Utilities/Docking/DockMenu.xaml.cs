using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for DockMenu.xaml
    /// </summary>
    public partial class DockMenu : UserControl
	{
		public static readonly DependencyProperty ContextProperty = DependencyProperty.Register(
	"Context", typeof(string), typeof(DockMenu), new PropertyMetadata(default(string)));

        public string Context
		{
			get => (string)GetValue(ContextProperty);
			set => SetValue(ContextProperty, value);
		}
        public DockMenu()
		{
			InitializeComponent();
			button.Click += Button_Click;
		}

        private void Button_Click(object sender, RoutedEventArgs e)
		{
			contextMenu.IsOpen = true;
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			if (sender is MenuItem menuItem)
			{
				switch (menuItem.Header.ToString())
				{
					case "Close":
						Docking.CloseWindow(Context);
						break;
					case "Dock":
						Docking.DockWindow(Context);
						break;
					case "Float":
						Docking.CreateFloatingWindow(Context);
						break;
					default:
						break;
				}
			}
		}

		public object SelectedItem { get; set; }
	}
}
