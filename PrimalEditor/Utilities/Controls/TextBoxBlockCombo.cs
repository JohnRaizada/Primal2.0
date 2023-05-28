using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Utilities.Controls
{
    /// <summary>
    /// A control that combines a text box and a block of text.
    /// </summary>
    public class TextBoxBlockCombo : Control
	{
		private TextBox? _textBox;
		private TextBlock? _textBlock;
		private Border? _textBlockBorder;
		private int clickCount = 0;
        /// <summary>
        /// Registers the `IsToggled` dependency property.
        /// </summary>
        public static readonly DependencyProperty IsToggledProperty = DependencyProperty.Register("IsToggled", typeof(bool), typeof(TextBoxBlockCombo), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnIsToggledChanged)));
        /// <summary>
        /// Registers the `Text` dependency property.
        /// </summary>
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(TextBoxBlockCombo), new PropertyMetadata(string.Empty));
        /// <summary>
        /// Registers the `ValueChanged` event.
        /// </summary>
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(ValueChangedEventHandler), typeof(TextBoxBlockCombo));
        /// <summary>
        /// Gets or sets whether the text box is toggled.
        /// </summary>
        public bool IsToggled
		{
			get { return (bool)GetValue(IsToggledProperty); }
			set { SetValue(IsToggledProperty, value); }
        }
        /// <summary>
        /// Gets or sets the text of the text box.
        /// </summary>
        public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
        }
        /// <summary>
        /// Gets or sets the text of the text box.
        /// </summary>
        public event ValueChangedEventHandler ValueChanged
		{
			add { AddHandler(ValueChangedEvent, value); }
			remove { RemoveHandler(ValueChangedEvent, value); }
		}
		static TextBoxBlockCombo() => DefaultStyleKeyProperty.OverrideMetadata(typeof(TextBoxBlockCombo), new FrameworkPropertyMetadata(typeof(TextBoxBlockCombo)));
		private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var control = (TextBoxBlockCombo)d;
			if ((bool)e.NewValue == false) control.RaiseValueChangedEvent();
			UpdateControl(control, (bool)e.NewValue);
		}
		private static void UpdateControl(TextBoxBlockCombo control, bool newValue)
		{
			if (control.GetTemplateChild("PART_TextBlock") is not TextBlock textBlock || control.GetTemplateChild("PART_TextBox") is not TextBox textBox) return;
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
        /// <summary>
        /// Gets or sets the text of the text box.
        /// </summary>
        private void RaiseValueChangedEvent() => RaiseEvent(new ValueChangedEventArgs(_textBox?.Text)
        {
            RoutedEvent = ValueChangedEvent
        });
        /// <inheritdoc/>
        public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			_textBox = (TextBox)GetTemplateChild("PART_TextBox");
			_textBlock = (TextBlock)GetTemplateChild("PART_TextBlock");
			_textBlockBorder = (Border)GetTemplateChild("TextBlockBorder");
			if (_textBox == null) return;
			_textBox.KeyDown += TextBox_KeyDown;
			_textBox.LostFocus += TextBox_LostFocus;
			_textBlock.MouseDown += TextBlock_MouseDown;
			if (_textBlockBorder == null) return;
			_textBlockBorder.MouseUp += TextBlock_MouseDown;
        }
        /// <summary>
        /// Occurs when the text box is clicked.
        /// </summary>
        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (++clickCount < 2) return;
			IsToggled = true;
			clickCount = 0;
		}
		private void TextBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key != Key.Enter) return;
			IsToggled = false;
			e.Handled = true;
        }
        /// <summary>
        /// Occurs when the text box loses focus.
        /// </summary>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			IsToggled = false;
			e.Handled = true;
		}
    }
    /// <summary>
    /// A delegate that is invoked when the `ValueChanged` event is raised.
    /// </summary>
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
    /// <summary>
    /// An event args that is passed to the `ValueChanged` event handler.
    /// </summary>
    public class ValueChangedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// Gets the new value of the text box.
        /// </summary>
        public string Value { get; }
        /// <summary>
        /// Initializes a new instance of the `ValueChangedEventArgs` class.
        /// </summary>
        /// <param name="value">The new value of the text box.</param>
        public ValueChangedEventArgs(string value) => Value = value;
	}
}