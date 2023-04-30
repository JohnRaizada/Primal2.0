using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock.Controls;

namespace PrimalEditor.Utilities.Controls
{
    class ExpanderListView : Control
    {
        private static int _highestColumnIndex = 0;
        private static IEnumerable? _localContentSource;
        private static bool _isChildUnchecked = false;
        private static bool _isChildCollapsed = false;
        private static bool _isUpdatingIsSaved = false;
        private static bool _isContentChangeTriggeredInternally = false;
        public static readonly DependencyProperty? HeaderSourceProperty = DependencyProperty.Register(nameof(HeaderSource), typeof(IEnumerable), typeof(ExpanderListView), new PropertyMetadata(null, OnHeaderSourceChanged));
        public static readonly DependencyProperty? ContentSourceProperty = DependencyProperty.Register(nameof(ContentSource), typeof(IEnumerable), typeof(ExpanderListView), new PropertyMetadata(null, OnContentSourceChanged));
        public static readonly DependencyProperty? IsToggledProperty = DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(ExpanderListView), new PropertyMetadata(false, OnIsToggledChanged));
        public static readonly DependencyProperty? IsSavedProperty = DependencyProperty.Register(nameof(IsSaved), typeof(bool), typeof(ExpanderListView), new PropertyMetadata(true, OnIsSavedChanged));
        public IEnumerable HeaderSource
        {
            get => (IEnumerable)GetValue(HeaderSourceProperty);
            set => SetValue(HeaderSourceProperty, value);
        }
        public IEnumerable ContentSource
        {
            get => (IEnumerable)GetValue(ContentSourceProperty);
            set => SetValue(ContentSourceProperty, value);
        }
        public bool IsToggled
        {
            get => (bool)GetValue(IsToggledProperty);
            set => SetValue(IsToggledProperty, value);
        }
        public bool IsSaved
        {
            get => (bool)GetValue(IsSavedProperty);
            set => SetValue(IsSavedProperty, value);
        }
        private static void OnHeaderSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isContentChangeTriggeredInternally) return;
            if (!(e.NewValue is IEnumerable newValue)) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            if (expanderListView == null) return;
            Grid? columnHeaders = (Grid)expanderListView.GetTemplateChild("PART_ColumnHeaders");
            if (columnHeaders == null) return;
            ListView? contentView = (ListView)expanderListView.GetTemplateChild("PART_ContentList");
            if (contentView == null) return;
            columnHeaders.Children.Clear();
            CreateColumnHeaders(expanderListView, columnHeaders, contentView);
            expanderListView.IsSaved = false;
        }
        private static void OnContentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isContentChangeTriggeredInternally) return;
            if (!(e.NewValue is IEnumerable newValue)) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            Grid? columnHeaders = (Grid)expanderListView.GetTemplateChild("PART_ColumnHeaders");
            if (columnHeaders == null) return;
            ListView? contentView = (ListView)expanderListView.GetTemplateChild("PART_ContentList");
            if (contentView == null) return;
            _localContentSource = expanderListView.ContentSource.OfType<ExpanderListMenuItem>().Select(item => item.DeepCopy()).ToList();
            contentView.Items.Clear();
            contentView.ItemsSource = _localContentSource;
            CreateContent(expanderListView, contentView, columnHeaders);
            expanderListView.IsSaved = false;
        }
        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isContentChangeTriggeredInternally) return;
            if (!(e.NewValue is bool newValue)) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            expanderListView.IsToggled = newValue;
            Grid? columnHeaders = (Grid)expanderListView.GetTemplateChild("PART_ColumnHeaders");
            if (columnHeaders == null) return;
            ListView? contentView = (ListView)expanderListView.GetTemplateChild("PART_ContentList");
            if (contentView == null) return;
            if (_isChildCollapsed) return;
            foreach (var item in contentView.Items)
            {
                var container = contentView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container == null) return;
                var expander = expanderListView.FindVisualChild<Expander>(container);
                if (expander == null) return;
                expander.IsExpanded = newValue;
            }
        }
        private static void OnIsSavedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isUpdatingIsSaved) return;
            _isUpdatingIsSaved = true;

            if (!(e.NewValue is bool newValue)) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            if (expanderListView == null) return;
            if (!newValue)
            {
                expanderListView.IsSaved = Enumerable.SequenceEqual((IEnumerable<ExpanderListMenuItem>)_localContentSource, (IEnumerable<ExpanderListMenuItem>)expanderListView.ContentSource, new ExpanderListMenuItemComparer());
                _isUpdatingIsSaved = false;
                return;
            }
            _isContentChangeTriggeredInternally = true;
            expanderListView.ContentSource = _localContentSource.OfType<ExpanderListMenuItem>().Select(item => item.DeepCopy()).ToList();
            _isContentChangeTriggeredInternally = false;
            expanderListView.IsSaved = newValue;

            _isUpdatingIsSaved = false;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            Grid? columnHeaders = (Grid)GetTemplateChild("PART_ColumnHeaders");
            if (columnHeaders == null) return;
            ListView? contentView = (ListView)GetTemplateChild("PART_ContentList");
            if (contentView == null) return;
            columnHeaders.Children.Clear();
            CreateColumnHeaders(this, columnHeaders, contentView);
            _localContentSource = ContentSource.OfType<ExpanderListMenuItem>().Select(item => item.DeepCopy()).ToList();
            contentView.ItemsSource = _localContentSource;
            Loaded += (sender, e) =>
            {
                CreateContent(this, contentView, columnHeaders);
            };
            IsSaved = false;
        }
        private static void CreateContent(ExpanderListView expanderListView, ListView contentView, Grid columnHeaders)
        {
            foreach ((ExpanderListMenuItem item, int index) in _localContentSource.OfType<ExpanderListMenuItem>().Select((item, index) => (item, index)))
            {
                CreateRowContent(expanderListView, contentView, item, columnHeaders, _highestColumnIndex, index);
            }
        }
        private static void CreateColumnHeaders(ExpanderListView expanderListView, Grid columnHeaders, ListView contentView)
        {
            int columnIndex = 0;
            foreach (var header in expanderListView.HeaderSource)
            {
                TextBlock? textBlock = new TextBlock
                {
                    Text = header.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    Style = (Style)Application.Current.FindResource("EnableDisableHeadingTextBlockStyle"),
                };
                columnHeaders.ColumnDefinitions.Add(new ColumnDefinition { MinWidth = 10 });
                Grid.SetColumn(textBlock, columnIndex);
                columnHeaders.Children.Add(textBlock);
                GridSplitter? splitter = new GridSplitter
                {
                    Width = 5,
                    Background = (Brush)Application.Current.FindResource("Editor.Window.GrayBrush3"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                int currentColumnIndex = columnIndex;
                splitter.DragDelta += (sender, e) =>
                {
                    expanderListView.GridSplitter_DragDelta(sender, e, currentColumnIndex, columnHeaders, contentView);
                };
                if (columnIndex == expanderListView.HeaderSource.Cast<object>().Count() - 1) continue;
                Grid.SetColumn(splitter, columnIndex++);
                columnHeaders.Children.Add(splitter);
            }
            _highestColumnIndex = columnIndex;
        }
        private static void CreateRowContent(ExpanderListView expanderListView, ListView contentView, ExpanderListMenuItem item, Grid columnHeaders, int columnIndex, int index)
        {
            var container = contentView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
            if (container == null) return;
            var expander = expanderListView.FindVisualChild<Expander>(container);
            if (expander == null) return;
            expander.Width = columnHeaders.ActualWidth;
            expander.IsExpanded = expanderListView.IsToggled;
            expander.Collapsed += (sender, e) =>
            {
                _isChildCollapsed = true;
                expanderListView.IsToggled = false;
                _isChildCollapsed = false;
            };
            expander.Expanded += (sender, e) =>
            {
                bool allChecked = true;
                // modify the value of all checked based on the checked values of all expanders
                foreach (Expander otherExpander in expanderListView.FindVisualChildren<Expander>())
                {
                    if (otherExpander != expander && otherExpander.IsExpanded == false)
                    {
                        allChecked = false;
                        break;
                    }
                }
                if (!allChecked) return;
                expanderListView.IsToggled = allChecked;
            };
            StackPanel? header = expander.Header as StackPanel;
            if (header == null) return;
            // Add a flag to keep track of whether the masterCheckBox is being unchecked as a result of a child checkbox being unchecked
            var grid = expander.Content as Grid;
            if (grid == null) return;
            CheckBox? masterCheckBox = AddMasterCheckBox(header, grid);
            if (masterCheckBox == null) return;
            // Get the properties of the ExpanderListMenuItem class
            var properties = typeof(ExpanderListMenuItem).GetProperties();
            if (properties.Count() <= columnIndex) return;
            var listMenuItemsProperty = properties.FirstOrDefault(p => p.Name == "ListMenuItems");
            if (listMenuItemsProperty == null) return;
            var listMenuItems = listMenuItemsProperty.GetValue(item) as List<ExpanderListMenuItem>;
            if (listMenuItems == null)
            {
                AddRow(expanderListView, columnIndex, columnHeaders, grid, properties, item, masterCheckBox, index);
                return;
            }
            for (int j = 0; j < listMenuItems.Count; j++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                AddRow(expanderListView, columnIndex, columnHeaders, grid, properties, item, masterCheckBox, index, j);
            }
        }
        private static CheckBox? AddMasterCheckBox(StackPanel header, Grid grid)
        {
            CheckBox? masterCheckBox = header.Children[0] as CheckBox;
            // Subscribe to the Checked and Unchecked events of the masterCheckBox
            if (masterCheckBox == null) return null;
            masterCheckBox.Checked += (sender, e) =>
            {
                foreach (var child in grid.Children)
                {
                    if (!(child is StackPanel stackPanel)) continue;
                    if (stackPanel.Children[0] is CheckBox checkBox)
                    {
                        checkBox.IsChecked = true;
                    }
                    else if (stackPanel.Children[1] is CheckBox checkBox1)
                    {
                        checkBox1.IsChecked = true;
                    }
                }
            };
            masterCheckBox.Unchecked += (sender, e) =>
            {
                // Check if the masterCheckBox is being unchecked as a result of a child checkbox being unchecked
                if (_isChildUnchecked) return;
                foreach (var child in grid.Children)
                {
                    if (!(child is StackPanel stackPanel)) continue;
                    if (stackPanel.Children[0] is CheckBox checkBox)
                    {
                        checkBox.IsChecked = false;
                    }
                    else if (stackPanel.Children[1] is CheckBox checkBox1)
                    {
                        checkBox1.IsChecked = false;
                    }
                }
            };

            return masterCheckBox;
        }
        private static void AddRow(ExpanderListView expanderListView, int columnIndex, Grid columnHeaders, Grid grid, PropertyInfo[] properties, ExpanderListMenuItem item, CheckBox masterCheckBox, int index, int rowIndex = 0)
        {
            for (int i = 0; i < columnIndex + 1; i++)
            {
                double defaultColumnWidth = columnHeaders.ColumnDefinitions[i].ActualWidth;
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(defaultColumnWidth)
                });
                var menuItem = item.ListMenuItems == null ? item : item.ListMenuItems.Count > 0 && item.ListMenuItems.Count > rowIndex ? item.ListMenuItems[rowIndex] : item;
                var text = properties[i].GetValue(menuItem);
                if (text == null) return;
                TextBlock? textBlock = new TextBlock
                {
                    Text = text.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                };
                if (i == 0)
                {
                    AddFirstCell(expanderListView, menuItem, grid, masterCheckBox, textBlock, index, i, rowIndex);
                    continue;
                }
                Grid.SetColumn(textBlock, i);
                Grid.SetRow(textBlock, rowIndex);
                grid.Children.Add(textBlock);
            }
        }
        private static void AddFirstCell(ExpanderListView expanderListView, ExpanderListMenuItem menuItem, Grid grid, CheckBox masterCheckBox, TextBlock textBlock, int index, int columnIndex, int rowIndex = 0)
        {
            var contentViewMenuItem = expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index).ListMenuItems == null ? expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index) : expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index).ListMenuItems.Count > 0 && expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index).ListMenuItems.Count > rowIndex ? expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index).ListMenuItems[rowIndex] : expanderListView.ContentSource.OfType<ExpanderListMenuItem>().ElementAt(index);
            StackPanel? stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            CheckBox? checkBox = new CheckBox
            {
                Margin = new Thickness(30, 1, 1, 1),
                IsChecked = menuItem.IsChecked
            };
            // Subscribe to the Checked and Unchecked events of the checkBox
            checkBox.Checked += (sender, e) =>
            {
                bool allChecked = true;
                foreach (var child in grid.Children)
                {
                    if (!(child is StackPanel stackPanel)) continue;
                    if (stackPanel.Children[0] is Path path) continue;
                    if (!(stackPanel.Children[1] is CheckBox checkBox && checkBox.IsChecked == false)) continue;
                    allChecked = false;
                    break;

                }
                masterCheckBox.IsChecked = allChecked;
                menuItem.IsChecked = true;
                expanderListView.IsSaved = Enumerable.SequenceEqual((IEnumerable<ExpanderListMenuItem>)_localContentSource, (IEnumerable<ExpanderListMenuItem>)expanderListView.ContentSource, new ExpanderListMenuItemComparer());
                if (contentViewMenuItem.IsChecked == menuItem.IsChecked)
                {
                    RemoveIcon(stackPanel, checkBox);
                    return;
                }
                AddIcon("M0 16q0 2.912 1.824 5.088t4.576 2.752q0.032 0 0.032-0.032v-0.064t0.032-0.032q0.544-1.344 1.344-2.176t2.208-1.184v-2.336q0-2.496 1.728-4.256t4.256-1.76 4.256 1.76 1.76 4.256v2.336q1.376 0.384 2.176 1.216t1.344 2.144l0.096 0.288h0.384q2.464 0 4.224-1.76t1.76-4.224v-2.016q0-2.464-1.76-4.224t-4.224-1.76q-0.096 0-0.32 0.032 0.32-1.152 0.32-2.048 0-3.296-2.368-5.632t-5.632-2.368q-2.88 0-5.056 1.824t-2.784 4.544q-1.152-0.352-2.176-0.352-3.296 0-5.664 2.336t-2.336 5.664v1.984zM10.016 25.824q-0.096 0.928 0.576 1.6l4 4q0.576 0.576 1.408 0.576t1.408-0.576l4-4q0.672-0.672 0.608-1.6-0.064-0.32-0.16-0.576-0.224-0.576-0.736-0.896t-1.12-0.352h-1.984v-5.984q0-0.832-0.608-1.408t-1.408-0.608-1.408 0.608-0.576 1.408v5.984h-2.016q-0.608 0-1.12 0.352t-0.736 0.896q-0.096 0.288-0.128 0.576z", stackPanel, checkBox);
            };
            checkBox.Unchecked += (sender, e) =>
            {
                // Set the flag to indicate that the masterCheckBox is being unchecked as a result of a child checkbox being unchecked
                _isChildUnchecked = true;
                masterCheckBox.IsChecked = false;
                _isChildUnchecked = false;
                menuItem.IsChecked = false;
                expanderListView.IsSaved = Enumerable.SequenceEqual((IEnumerable<ExpanderListMenuItem>)_localContentSource, (IEnumerable<ExpanderListMenuItem>)expanderListView.ContentSource, new ExpanderListMenuItemComparer());
                if (contentViewMenuItem.IsChecked == menuItem.IsChecked)
                {
                    RemoveIcon(stackPanel, checkBox);
                    return;
                }
                AddIcon("M6 5H18M9 5V5C10.5769 3.16026 13.4231 3.16026 15 5V5M9 20H15C16.1046 20 17 19.1046 17 18V9C17 8.44772 16.5523 8 16 8H8C7.44772 8 7 8.44772 7 9V18C7 19.1046 7.89543 20 9 20Z", stackPanel, checkBox);
            };
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBlock);
            Grid.SetColumn(stackPanel, columnIndex);
            Grid.SetRow(stackPanel, rowIndex);
            grid.Children.Add(stackPanel);
        }
        private static void RemoveIcon(StackPanel stackPanel, CheckBox checkBox)
        {
            stackPanel.Children.RemoveAt(0);
            checkBox.Margin = new Thickness(30, 1, 1, 1);
        }
        private static void AddIcon(string path, StackPanel stackPanel, CheckBox checkBox)
        {
            Path? icon = new Path
            {
                Stretch = Stretch.Uniform,
                Fill = (Brush)Application.Current.FindResource("Editor.Disabled.FontBrush"),
                Margin = new Thickness(1),
                Height = 15,
                Width = 15,
                Data = Geometry.Parse(path)
            };
            stackPanel.Children.Insert(0, icon);
            checkBox.Margin = new Thickness(13, 1, 1, 1);
        }
        private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e, int columnIndex = 0, Grid? columnHeaders = null, ListView? listView = null)
        {
            if (columnHeaders == null || listView == null) return;
            var container = listView.ItemContainerGenerator.ContainerFromItem(listView.Items[0]) as ListViewItem;
            if (container == null) return;
            var expander = FindVisualChild<Expander>(container);
            if (expander == null) return;
            var grid = expander.Content as Grid;
            if (grid == null) return;
            if (columnIndex >= grid.ColumnDefinitions.Count) return;
            var newWidth = columnHeaders.ColumnDefinitions[columnIndex].ActualWidth + e.HorizontalChange;
            if (newWidth <= columnHeaders.ColumnDefinitions[columnIndex].MinWidth) return;
            columnHeaders.ColumnDefinitions[columnIndex].Width = new GridLength(newWidth);
            foreach (var item in listView.Items)
            {
                container = listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container == null) return;
                expander = FindVisualChild<Expander>(container);
                if (expander == null) return;
                grid = expander.Content as Grid;
                if (grid == null) return;
                grid.ColumnDefinitions[columnIndex].Width = new GridLength(newWidth);
                double adjacentColumnWidth = 0;
                if (columnIndex <= grid.ColumnDefinitions.Count - 2)
                {
                    adjacentColumnWidth = columnHeaders.ColumnDefinitions[columnIndex + 1].ActualWidth;
                    grid.ColumnDefinitions[columnIndex + 1].Width = new GridLength(adjacentColumnWidth);
                }
                if (columnIndex > 0)
                {
                    adjacentColumnWidth = columnHeaders.ColumnDefinitions[columnIndex - 1].ActualWidth;
                    grid.ColumnDefinitions[columnIndex - 1].Width = new GridLength(adjacentColumnWidth);
                }
            }
        }
        private T? FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                var child = VisualTreeHelper.GetChild(obj, i);
                if (child is T visualChild)
                {
                    return visualChild;
                }
                else
                {
                    var descendant = FindVisualChild<T>(child);
                    if (descendant != null)
                    {
                        return descendant;
                    }
                }
            }
            return null;
        }
    }
    public class ExpanderListMenuItemComparer : IEqualityComparer<ExpanderListMenuItem>
    {
        public bool Equals(ExpanderListMenuItem x, ExpanderListMenuItem y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Name != y.Name) return false;
            if (x.APILevel != y.APILevel) return false;
            if (x.Status != y.Status) return false;
            if (x.IsChecked != y.IsChecked) return false;
            if (x.ListMenuItems == null && y.ListMenuItems == null) return true;
            if (x.ListMenuItems == null || y.ListMenuItems == null) return false;
            return x.ListMenuItems.SequenceEqual(y.ListMenuItems, new ExpanderListMenuItemComparer());
        }
        public int GetHashCode(ExpanderListMenuItem obj)
        {
            int hash = 17;
            hash = hash * 23 + (obj.Name?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.APILevel.GetHashCode();
            hash = hash * 23 + obj.Status.GetHashCode();
            hash = hash * 23 + obj.IsChecked.GetHashCode();
            if (obj.ListMenuItems != null)
            {
                foreach (var item in obj.ListMenuItems)
                {
                    hash = hash * 23 + item.GetHashCode();
                }
            }
            return hash;
        }
    }

}