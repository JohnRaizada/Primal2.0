using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PrimalEditor.Utilities.Controls
{
    internal class PasswordBoxView : Control
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PasswordBoxView));
        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(PasswordBoxView), new PropertyMetadata(false));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(PasswordBoxView), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValueChanged)));
        public event RoutedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }
        public string Value
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PasswordBoxView).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("visibilityIcons") is Grid grid)
            {
                grid.MouseLeftButtonDown += OnEyeMouseDown;
            }
        }
        private void OnEyeMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(GetTemplateChild("textBox") is TextBox textBox)) return;
            if (!(GetTemplateChild("passwordBox") is PasswordBox passwordBox)) return;
            if (!(GetTemplateChild("hideIcon") is Path hideIcon)) return;
            if (!(GetTemplateChild("showIcon") is Path showIcon)) return;
            textBox.Text = passwordBox.Password;
            passwordBox.Visibility = passwordBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            textBox.Visibility = textBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            hideIcon.Visibility = hideIcon.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            showIcon.Visibility = showIcon.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
