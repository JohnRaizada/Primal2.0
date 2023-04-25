using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Utilities.Controls
{
	[TemplatePart(Name = "PART_textBlock", Type = typeof(TextBlock))]
	[TemplatePart(Name = "PART_textBox", Type = typeof(TextBox))]
	class NumberBox : Control
	{
		private double _originalValue;
		private double _mouseXStart;
		private double _multiplier;
		private bool _captured = false;
		private bool _valueChanged = false;
		public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(NumberBox));
		public static readonly DependencyProperty PrecisionProperty = DependencyProperty.Register(nameof(Precision), typeof(int), typeof(NumberBox), new PropertyMetadata(0));
		public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(nameof(Multiplier), typeof(double), typeof(NumberBox), new PropertyMetadata(1.0));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(NumberBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnValueChanged)));
        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(NumberBox), new PropertyMetadata(double.NegativeInfinity));
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(NumberBox), new PropertyMetadata(double.PositiveInfinity));
        public event RoutedEventHandler ValueChanged
        {
            add => AddHandler(ValueChangedEvent, value);
            remove => RemoveHandler(ValueChangedEvent, value);
        }
        public int Precision
		{
			get => (int)GetValue(PrecisionProperty);
			set => SetValue(PrecisionProperty, value);
		}
		public double Multiplier
		{
			get => (double)GetValue(MultiplierProperty);
			set => SetValue(MultiplierProperty, value);
		}
		public string Value
		{
			get => (string)GetValue(ValueProperty);
			set => SetValue(ValueProperty, value);
		}
		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
		}

		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
		}
		private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (e.NewValue is string value)
			{
				if (double.TryParse(value, out var number))
				{
                    // The new value is a valid number
                    // Get the minimum and maximum values
                    var minimum = (double)d.GetValue(MinimumProperty);
                    var maximum = (double)d.GetValue(MaximumProperty);

                    // Clamp the number to the specified range
                    number = Math.Max(number, minimum);
                    number = Math.Min(number, maximum);

                    // Format the value with the specified number of decimal digits
                    var precision = (int)d.GetValue(PrecisionProperty);
					var format = $"F{precision}";
					d.SetValue(e.Property, number.ToString(format));
				}
				else
				{
					// The new value is not a valid number
					// Reset the value to its previous value
					d.SetValue(e.Property, e.OldValue);
				}
				return;
			}
			(d as NumberBox).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			if (GetTemplateChild("PART_textBlock") is TextBlock textBlock)
			{
				textBlock.MouseLeftButtonDown += OnTextBlock_Mouse_LBD;
				textBlock.MouseLeftButtonUp += OnTextBlock_Mouse_LBU;
				textBlock.MouseMove += OnTextBlock_Mouse_Move;
			}
		}

		private void OnTextBlock_Mouse_LBD(object sender, MouseButtonEventArgs e)
		{
			double.TryParse(Value, out _originalValue);
			Mouse.Capture(sender as UIElement);
			_captured = true;
			_valueChanged = false;
			e.Handled = true;
			_mouseXStart = e.GetPosition(this).X;
			Focus();
		}
		private void OnTextBlock_Mouse_LBU(object sender, MouseButtonEventArgs e)
		{
			if (_captured)
			{
				Mouse.Capture(null);
				_captured = false;
				e.Handled = true;
				if (!_valueChanged && GetTemplateChild("PART_textBox") is TextBox textBox)
				{
					textBox.Visibility = Visibility.Visible;
					textBox.Focus();
					textBox.SelectAll();
				}
			}
		}
		private void OnTextBlock_Mouse_Move(object sender, MouseEventArgs e)
		{
			if (_captured)
			{
				var mouseX = e.GetPosition(this).X;
				var d = mouseX - _mouseXStart;
				if (Math.Abs(d) > SystemParameters.MinimumHorizontalDragDistance)
				{
					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _multiplier = 0.001;
					else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) _multiplier = 0.1;
					else _multiplier = 0.01;
					var newValue = _originalValue + (d * _multiplier * Multiplier);
					Value = newValue.ToString();
					_valueChanged = true;
				}
			}
		}
		static NumberBox()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
		}
	}
}
