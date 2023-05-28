using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace PrimalEditor.Utilities.Controls
{
    class SteppedSlider : Control
    {
        public static readonly RoutedEvent ValueChangedEvent = EventManager.RegisterRoutedEvent(nameof(ValueChanged), RoutingStrategy.Bubble, typeof(ValueChangedEventHandler), typeof(SteppedSlider));
        public static readonly DependencyProperty ItemSourceProperty = DependencyProperty.Register(nameof(ItemSource), typeof(IEnumerable), typeof(SteppedSlider), new PropertyMetadata(default(IEnumerable), OnItemSourceChanged));
        public static readonly DependencyProperty IsLabelVisibleProperty = DependencyProperty.Register(nameof(IsLabelVisible), typeof(bool), typeof(SteppedSlider), new PropertyMetadata(true, OnIsLabelVisibleChanged));
        public static readonly DependencyProperty TickPlacementProperty = DependencyProperty.Register(nameof(TickPlacement), typeof(TickPlacement), typeof(SteppedSlider), new PropertyMetadata(TickPlacement.BottomRight, OnTickPlacementChanged));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(SteppedSlider), new PropertyMetadata(Orientation.Horizontal, OnOrientationChanged));
        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(SteppedSlider), new PropertyMetadata(null, OnSelectedItemChanged));
        public static readonly DependencyProperty SelectedIndexProperty = DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(SteppedSlider), new PropertyMetadata(0, OnSelectedIndexChanged));
        public event ValueChangedEventHandler ValueChanged
        {
            add { AddHandler(ValueChangedEvent, value); }
            remove { RemoveHandler(ValueChangedEvent, value); }
        }
        public IEnumerable ItemSource
        {
            get => (IEnumerable)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }
        public bool IsLabelVisible
        {
            get => (bool)GetValue(IsLabelVisibleProperty);
            set => SetValue(IsLabelVisibleProperty, value);
        }
        public TickPlacement TickPlacement
        {
            get => (TickPlacement)GetValue(TickPlacementProperty);
            set => SetValue(TickPlacementProperty, value);
        }
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
        static SteppedSlider() => DefaultStyleKeyProperty.OverrideMetadata(typeof(SteppedSlider), new FrameworkPropertyMetadata(typeof(SteppedSlider)));
        private static void OnItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is IEnumerable newValue)
            {
                var slider = (SteppedSlider)d;
                if (slider.GetTemplateChild("PART_STACKPANEL") is not UniformGrid stackPanel) return;
                // Clear the existing children of the stack panel
                stackPanel.Children.Clear();
                // Add new children to the stack panel based on the values in the ItemSource property
                foreach (var value in newValue)
                {
                    var textBlock = new TextBlock
                    {
                        Text = value.ToString(),
                        HorizontalAlignment = slider.Orientation == Orientation.Horizontal ? HorizontalAlignment.Center : HorizontalAlignment.Left,
                        TextTrimming = TextTrimming.CharacterEllipsis,
                        TextWrapping = TextWrapping.Wrap,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(1),
                        Padding = new Thickness(1)
                    };
                    stackPanel.Children.Add(textBlock);
                }
                if (slider.GetTemplateChild("PART_SLIDER") is not Slider thumbSlider) return;
                // Update the number of ticks on the slider based on the length of the ItemSource property
                thumbSlider.TickFrequency = 1;
                thumbSlider.Ticks = new DoubleCollection(Enumerable.Range(0, newValue.Cast<object>().Count() + 1).Select(i => (double)i));
                thumbSlider.Maximum = thumbSlider.Ticks.Count - 2;
            }
            ((SteppedSlider)d).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
        private static void OnIsLabelVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not bool newValue) return;
            var slider = (SteppedSlider)d;
            if (slider.GetTemplateChild("PART_STACKPANEL") is not UniformGrid stackPanel) return;
            stackPanel.Visibility = newValue ? Visibility.Visible : slider.Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
        }
        private static void OnTickPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not TickPlacement newValue) return;
            var slider = (SteppedSlider)d;
            if (slider.GetTemplateChild("PART_SLIDER") is not Slider thumbSlider) return;
            if (newValue == thumbSlider.TickPlacement) return;
            thumbSlider.TickPlacement = newValue;
            if (slider.GetTemplateChild("Collector") is not StackPanel controller) return;
            if (slider.GetTemplateChild("PART_STACKPANEL") is not UniformGrid stackPanel) return;
            controller.Children.Remove(stackPanel);
            if (e.OldValue is not TickPlacement oldValue) return;
            switch (newValue)
            {
                case TickPlacement.None:
                    stackPanel.Visibility = slider.Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
                    break;
                case TickPlacement.TopLeft:
                    switch (oldValue)
                    {
                        case TickPlacement.None:
                            stackPanel.Visibility = slider.IsLabelVisible ? Visibility.Visible : slider.Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
                            controller.Children.Insert(0, stackPanel);
                            break;
                        case TickPlacement.BottomRight:
                            controller.Children.Remove(stackPanel);
                            controller.Children.Insert(0, stackPanel);
                            break;
                        case TickPlacement.Both:
                            controller.Children.RemoveAt(2);
                            break;
                        default: break;
                    }
                    break;
                case TickPlacement.BottomRight:
                    switch (oldValue)
                    {
                        case TickPlacement.None:
                            stackPanel.Visibility = slider.IsLabelVisible ? Visibility.Visible : slider.Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
                            controller.Children.Add(stackPanel);
                            break;
                        case TickPlacement.TopLeft:
                            controller.Children.Remove(stackPanel);
                            controller.Children.Add(stackPanel);
                            break;
                        case TickPlacement.Both:
                            controller.Children.RemoveAt(0);
                            break;
                        default: break;
                    }
                    break;
                case TickPlacement.Both:
                    switch (oldValue)
                    {
                        case TickPlacement.None:
                            stackPanel.Visibility = slider.IsLabelVisible ? Visibility.Visible : slider.Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
                            controller.Children.Add(stackPanel);
                            controller.Children.Insert(0, stackPanel);
                            break;
                        case TickPlacement.TopLeft:
                            controller.Children.Add(stackPanel);
                            break;
                        case TickPlacement.BottomRight:
                            controller.Children.Insert(0, stackPanel);
                            break;
                        default: break;
                    }
                    break;
                default: break;
            }
        }
        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is not Orientation newValue) return;
            var slider = (SteppedSlider)d;
            if (slider == null) return;
            if (slider.GetTemplateChild("PART_SLIDER") is not Slider thumbSlider || slider.GetTemplateChild("PART_STACKPANEL") is not UniformGrid stackPanel || slider.GetTemplateChild("Collector") is not StackPanel controller) return;
            controller.Orientation = newValue == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            stackPanel.Rows = slider.Orientation == Orientation.Horizontal ? 1 : stackPanel.Children.Count;
            stackPanel.Columns = slider.Orientation == Orientation.Vertical ? 1 : stackPanel.Children.Count;
            thumbSlider.Orientation = newValue;
            if (slider.Orientation == Orientation.Horizontal) return;
            var children = stackPanel.Children.Cast<UIElement>().ToList();
            children.Reverse();
            stackPanel.Children.Clear();
            foreach (var child in children) stackPanel.Children.Add(child);
        }
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is object newValue)
            {
                var slider = (SteppedSlider)d;
                var index = slider.ItemSource.Cast<object>().ToList().IndexOf(newValue);
                if (index >= 0) slider.SelectedIndex = index;
            }
            ((SteppedSlider)d).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is int newValue)
            {
                var slider = (SteppedSlider)d;
                if (slider.GetTemplateChild("PART_SLIDER") is not Slider thumbSlider) return;
                thumbSlider.Value = newValue;
            }
            ((SteppedSlider)d).RaiseEvent(new RoutedEventArgs(ValueChangedEvent));
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (GetTemplateChild("PART_STACKPANEL") is not UniformGrid stackPanel) return;
            // Clear the existing children of the stack panel
            stackPanel.Children.Clear();
            // Add new children to the stack panel based on the values in the ItemSource property
            foreach (var value in ItemSource)
            {
                var textBlock = new TextBlock
                {
                    Text = value.ToString(),
                    HorizontalAlignment = Orientation == Orientation.Horizontal ? HorizontalAlignment.Center : HorizontalAlignment.Left,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Margin = new Thickness(1),
                    Padding = new Thickness(1)
                };
                stackPanel.Children.Add(textBlock);
            }
            if (GetTemplateChild("PART_SLIDER") is not Slider thumbSlider) return;
            // Update the number of ticks on the slider based on the length of the ItemSource property
            thumbSlider.TickFrequency = 1;
            thumbSlider.Ticks = new DoubleCollection(Enumerable.Range(0, ItemSource.Cast<object>().Count() + 1).Select(i => (double)i));
            thumbSlider.Maximum = thumbSlider.Ticks.Count - 2;
            thumbSlider.Loaded += (s, e) =>
            {
                if (Orientation != Orientation.Horizontal) return;
                var textBlockWidth = thumbSlider.ActualWidth / (thumbSlider.Maximum + 1);
                var textAreaWidth = textBlockWidth * stackPanel.Children.Count;
                // Update the width of each TextBlock once the Slider and UniformGrid have been rendered
                stackPanel.Width = textAreaWidth;
                thumbSlider.Width = textAreaWidth - textBlockWidth;
            };
            if (GetTemplateChild("Collector") is not StackPanel controller) return;
            controller.SizeChanged += (s, e) =>
            {
                if (Orientation != Orientation.Horizontal) return;
                var delta = e.NewSize.Width - e.PreviousSize.Width;
                thumbSlider.Width += delta;
                var textBlockWidth = thumbSlider.Width / (thumbSlider.Maximum);
                var textAreaWidth = textBlockWidth * stackPanel.Children.Count;
                // Update the width of each TextBlock once the Slider and UniformGrid have been rendered
                /* Check if stack panel is wider than the parent of the parent and if so, return*/
                if (VisualTreeHelper.GetParent(controller) is not FrameworkElement parent) return;
                if (stackPanel.Width >= parent.ActualWidth) stackPanel.Width = parent.ActualWidth;
                else stackPanel.Width = textAreaWidth;
                thumbSlider.Width = stackPanel.Width / stackPanel.Children.Count * thumbSlider.Maximum;
            };
            stackPanel.Visibility = IsLabelVisible ? Visibility.Visible : Orientation == Orientation.Horizontal ? Visibility.Collapsed : Visibility.Hidden;
            stackPanel.Rows = Orientation == Orientation.Horizontal ? 1 : stackPanel.Children.Count;
            stackPanel.Columns = Orientation == Orientation.Vertical ? 1 : stackPanel.Children.Count;
            thumbSlider.Orientation = Orientation;
            controller.Orientation = Orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal;
            if (Orientation == Orientation.Horizontal) return;
            var children = stackPanel.Children.Cast<UIElement>().ToList();
            children.Reverse();
            stackPanel.Children.Clear();
            foreach (var child in children) stackPanel.Children.Add(child);
        }
    }
}
