using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace PrimalEditor.Editors
{
	/// <summary>
	/// Interaction logic for ComponentView.xaml
	/// </summary>
	[ContentProperty("ComponentContent")]
	public partial class ComponentView : UserControl
	{
		/// <summary>
		/// Using a DependencyProperty as the backing store for Header. This enables animation, styling, binding, etc...
		/// </summary>
		public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(ComponentView));
		/// <summary>
		/// Using a DependencyProperty as the backing store for ComponentContent. This enables animation, styling, binding, etc...
		/// </summary>
		public static readonly DependencyProperty ComponentContentProperty = DependencyProperty.Register(nameof(ComponentContent), typeof(FrameworkElement), typeof(ComponentView));
        /// <summary>
        /// The header of the component.
        /// </summary>
        public string Header
		{
			get { return (string)GetValue(HeaderProperty); }
			set { SetValue(HeaderProperty, value); }
        }
        /// <summary>
        /// The content of the component.
        /// </summary>
        public FrameworkElement ComponentContent
		{
			get { return (FrameworkElement)GetValue(ComponentContentProperty); }
			set { SetValue(ComponentContentProperty, value); }
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ComponentView"/> class.
        /// </summary>
        public ComponentView() => InitializeComponent();
	}
}
