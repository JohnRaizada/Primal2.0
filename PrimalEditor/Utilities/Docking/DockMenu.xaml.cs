using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// A menu that allows users to dock or float a window.
    /// </summary>
    public partial class DockMenu : UserControl
    {
        /// <summary>
        /// The property associated with the context of the window that is being docked or floated.
        /// </summary>
        public static readonly DependencyProperty ContextProperty = DependencyProperty.Register("Context", typeof(string), typeof(DockMenu), new PropertyMetadata(default(string)));
        /// <summary>
        /// The context of the window that is being docked or floated.
        /// </summary>
		public string Context
        {
            get => (string)GetValue(ContextProperty);
            set => SetValue(ContextProperty, value);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DockMenu"/> class.
        /// </summary>
        public DockMenu()
        {
            InitializeComponent();
            button.Click += Button_Click;
        }
        private void Button_Click(object sender, RoutedEventArgs e) => contextMenu.IsOpen = true;
        /// <summary>
        /// Handles the <see cref="MenuItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The event arguments.</param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;
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
                default: break;
            }
        }
        /// <summary>
        /// The selected item in the menu.
        /// </summary>
        public object? SelectedItem { get; set; }
    }
}
