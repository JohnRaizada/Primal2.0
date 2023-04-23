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

        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Header.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(nameof(Header), typeof(string), typeof(ComponentView));


        public FrameworkElement ComponentContent
        {
            get { return (FrameworkElement)GetValue(ComponentContentProperty); }
            set { SetValue(ComponentContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ComponentContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ComponentContentProperty =
            DependencyProperty.Register(nameof(ComponentContent), typeof(FrameworkElement), typeof(ComponentView));



        public ComponentView()
        {
            InitializeComponent();
        }
    }
}
