using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace PrimalEditor.Utilities.Controls
{
    internal class PasswordBoxView : Control
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PasswordBoxView));
        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(PasswordBoxView), new PropertyMetadata(false));
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
        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((PasswordBoxView)d).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("visibilityIcons") is not Grid grid) return;
            if (GetTemplateChild("textBox") is not TextBox textBox) return;
            if (GetTemplateChild("passwordBox") is not PasswordBox passwordBox) return;
            grid.MouseLeftButtonDown += OnEyeMouseDown;
            textBox.KeyUp += OnKeyboardValueChanged;
            passwordBox.KeyUp += OnKeyboardValueChanged;
        }
        private void OnKeyboardValueChanged(object sender, KeyEventArgs e)
        {
            if (GetTemplateChild("notificationTextBlock") is not TextBlock textBlock) return;
            string value = "";
            if (sender is PasswordBox passwordBox) value = passwordBox.Password;
            else if (sender is TextBox textBox) value = textBox.Text;
            textBlock.Visibility = Visibility.Visible;
            if (string.IsNullOrEmpty(value)) textBlock.Text = "Password cannot be empty.";
            else if (value.Length < 8) textBlock.Text = "Password must be at least 8 characters long.";
            else if (!Regex.IsMatch(value, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,}$")) textBlock.Text = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character.";
            else textBlock.Visibility = Visibility.Collapsed;
        }
        private void OnEyeMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (GetTemplateChild("textBox") is not TextBox textBox) return;
            if (GetTemplateChild("passwordBox") is not PasswordBox passwordBox) return;
            if (GetTemplateChild("hideIcon") is not Path hideIcon) return;
            if (GetTemplateChild("showIcon") is not Path showIcon) return;
            textBox.Text = passwordBox.Password;
            passwordBox.Visibility = passwordBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            textBox.Visibility = textBox.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            hideIcon.Visibility = hideIcon.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            showIcon.Visibility = showIcon.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
