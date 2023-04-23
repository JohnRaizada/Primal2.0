using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
