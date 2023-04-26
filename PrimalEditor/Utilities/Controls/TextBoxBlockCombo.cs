using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Utilities.Controls
{
    public class TextBoxBlockCombo : Control
    {
        private TextBox _textBox;
        private TextBlock _textBlock;
        private Border _textBlockBorder;
        private int clickCount = 0;
        public static readonly DependencyProperty IsToggledProperty =
            DependencyProperty.Register("IsToggled", typeof(bool), typeof(TextBoxBlockCombo), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsToggledChanged)));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(TextBoxBlockCombo), new PropertyMetadata(string.Empty));

        public static readonly RoutedEvent ValueChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(ValueChangedEventHandler), typeof(TextBoxBlockCombo));

        public bool IsToggled
        {
            get { return (bool)GetValue(IsToggledProperty); }
            set { SetValue(IsToggledProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public event ValueChangedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }

        static TextBoxBlockCombo()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxBlockCombo), new FrameworkPropertyMetadata(typeof(TextBoxBlockCombo)));
        }

        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as TextBoxBlockCombo;
            if ((bool)e.NewValue == false)
            {
                control.RaiseValueChangedEvent();
            }
            UpdateControl(control, (bool)e.NewValue);
        }

        private static void UpdateControl(TextBoxBlockCombo control, bool newValue)
        {
            var textBox = control.GetTemplateChild("PART_TextBox") as TextBox;
            var textBlock = control.GetTemplateChild("PART_TextBlock") as TextBlock;
            if (textBlock == null || textBox == null) return;
            if (newValue)
            {
                textBox.Visibility = Visibility.Visible;
                textBlock.Visibility = Visibility.Collapsed;
                textBox.SelectAll();
                textBox.Focus();
                return;
            }
            textBox.Visibility = Visibility.Collapsed;
            textBlock.Visibility = Visibility.Visible;
        }

        private void RaiseValueChangedEvent()
        {
            var args = new ValueChangedEventArgs(_textBox.Text);
            args.RoutedEvent = ValueChangedEvent;
            RaiseEvent(args);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _textBlock = GetTemplateChild("PART_TextBlock") as TextBlock;
            _textBlockBorder = (Border)GetTemplateChild("TextBlockBorder");
            if (_textBox != null)
            {
                _textBox.KeyDown += TextBox_KeyDown;
                _textBox.LostFocus += TextBox_LostFocus;
                _textBlock.MouseDown += TextBlock_MouseDown;
                if (_textBlockBorder == null) return;
                _textBlockBorder.MouseUp += TextBlock_MouseDown;
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (++clickCount < 2) return;
            IsToggled = true;
            clickCount = 0;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IsToggled = false;
                e.Handled = true;
            }
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            IsToggled = false;
            e.Handled = true;
        }
    }
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);

    public class ValueChangedEventArgs : RoutedEventArgs
    {
        public string Value { get; }

        public ValueChangedEventArgs(string value)
        {
            Value = value;
        }
    }
}