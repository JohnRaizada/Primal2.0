using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
        public event EventHandler<CheckBoxChangedEventArgs>? CheckBoxChanged;
        protected virtual void OnCheckBoxChanged(CheckBoxChangedEventArgs e) => CheckBoxChanged?.Invoke(this, e);
        public static readonly DependencyProperty? HeaderSourceProperty = DependencyProperty.Register(nameof(HeaderSource), typeof(IEnumerable), typeof(ExpanderListView), new PropertyMetadata(null));
        public static readonly DependencyProperty? ContentSourceProperty = DependencyProperty.Register(nameof(ContentSource), typeof(IEnumerable), typeof(ExpanderListView), new PropertyMetadata(null));
        public static readonly DependencyProperty? IsToggledProperty = DependencyProperty.Register(nameof(IsToggled), typeof(bool), typeof(ExpanderListView), new PropertyMetadata(false, OnIsToggledChanged));
        public static readonly DependencyProperty? IsSavedProperty = DependencyProperty.Register(nameof(IsSaved), typeof(bool), typeof(ExpanderListView), new PropertyMetadata(true, OnIsSavedChanged));
        public static readonly DependencyProperty ContentViewModeProperty = DependencyProperty.Register(nameof(ContentViewMode), typeof(ContentViewMode), typeof(ExpanderListView), new PropertyMetadata(ContentViewMode.List));
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
        public ContentViewMode ContentViewMode
        {
            get => (ContentViewMode)GetValue(ContentViewModeProperty);
            set => SetValue(ContentViewModeProperty, value);
        }
        private static void OnIsToggledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_isContentChangeTriggeredInternally) return;
            if (e.NewValue is not bool newValue) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            expanderListView.IsToggled = newValue;
            Grid? columnHeaders = (Grid)expanderListView.GetTemplateChild("PART_ColumnHeaders");
            if (columnHeaders == null) return;
            ListView? contentView = (ListView)expanderListView.GetTemplateChild("PART_ContentList");
            if (contentView == null) return;
            if (_isChildCollapsed) return;
            foreach (var item in contentView.Items)
            {
                if (contentView.ItemContainerGenerator.ContainerFromItem(item) is not ListViewItem container) return;
                var expander = expanderListView.FindVisualChild<Expander>(container);
                if (expander == null) return;
                expander.IsExpanded = newValue;
            }
        }
        private static void OnIsSavedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_localContentSource == null) return;
            if (_isUpdatingIsSaved) return;
            _isUpdatingIsSaved = true;
            if (e.NewValue is not bool newValue) return;
            ExpanderListView? expanderListView = (ExpanderListView)d;
            if (expanderListView == null) return;
            if (!newValue)
            {
                if (expanderListView.ContentSource == null) return;
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
            if (ContentSource == null) return;
            _localContentSource = ContentSource.OfType<ExpanderListMenuItem>().Select(item => item.DeepCopy()).ToList();
            foreach (var item in _localContentSource)
            {
                contentView.Items.Add(item);
            }
            Loaded += (sender, e) =>
            {
                CreateContent(this, contentView, columnHeaders);
            };
            IsSaved = false;
        }
        private static void CreateContent(ExpanderListView expanderListView, ListView contentView, Grid columnHeaders)
        {
            if (_localContentSource == null) return;
            foreach ((ExpanderListMenuItem item, int index) in _localContentSource.OfType<ExpanderListMenuItem>().Select((item, index) => (item, index)))
            {
                CreateRowContent(expanderListView, contentView, item, columnHeaders, _highestColumnIndex, index);
            }
        }
        private static void CreateColumnHeaders(ExpanderListView expanderListView, Grid columnHeaders, ListView contentView)
        {
            int columnIndex = 0;
            if (expanderListView.HeaderSource == null) return;
            foreach (var header in expanderListView.HeaderSource)
            {
                StackPanel stackPanel = new()
                {
                    Orientation = Orientation.Horizontal
                };
                var textBlock = new TextBlock
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
                stackPanel.Children.Add(textBlock);
                AddIcon(IconTypes.Default, stackPanel, index: 1);
                stackPanel.MouseUp += (sender, e) =>
                {
                    SortOut(expanderListView, columnHeaders, contentView, sender, e);
                };
                Grid.SetColumn(stackPanel, columnIndex);
                columnHeaders.Children.Add(stackPanel);
                var splitter = new GridSplitter
                {
                    Width = 5,
                    Background = (Brush)Application.Current.FindResource("Editor.Window.GrayBrush3"),
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                int currentColumnIndex = columnIndex;
                splitter.DragDelta += (sender, e) =>
                {
                    expanderListView.GridSplitter_DragDelta(e, currentColumnIndex, columnHeaders, contentView);
                };
                if (columnIndex == expanderListView.HeaderSource.Cast<object>().Count() - 1) continue;
                Grid.SetColumn(splitter, columnIndex++);
                columnHeaders.Children.Add(splitter);
                columnHeaders.SizeChanged += (sender, e) => OnExpanderListView_HeaderResized(sender, e, expanderListView, contentView);
            }
            _highestColumnIndex = columnIndex;
        }
        private static void OnExpanderListView_HeaderResized(object sender, SizeChangedEventArgs e, ExpanderListView expanderListView, ListView listView)
        {
            if (sender is not Grid columnHeaders) return;
            // Check if expanderListView and listView are not null
            if (expanderListView == null || listView == null) return;
            // Update the width of expanderGrid and dockPanelGrid for each item in listView
            listView.Items
                .OfType<object>()
                .Select(item => listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem)
                .Where(x => x != null)
                .Select(x => (x, expander: x != null ? expanderListView.FindVisualChild<Expander>(x) : null))
                .Where(x => x.expander != null)
                .Select(x => (x.x, x.expander, dockPanel: x.x != null ? expanderListView.FindVisualChild<DockPanel>(x.x) : null))
                .Where(x => x.dockPanel != null)
                .Select(x => (x.x, x.expander, x.dockPanel, expanderGrid: x.expander?.Content as Grid))
                .Where(x => x.expanderGrid != null)
                .Select(x => (x.x, x.expander, x.dockPanel, x.expanderGrid, dockPanelGrid: x.dockPanel != null ? expanderListView.FindVisualChild<Grid>(x.dockPanel) : null))
                .ToList()
                .ForEach(x =>
                {
                    for (int i = 0; i < columnHeaders.ColumnDefinitions.Count; i++)
                    {
                        var newWidth = columnHeaders.ColumnDefinitions[i].ActualWidth;
                        if (x.expanderGrid == null) continue;
                        if (x.expanderGrid.ColumnDefinitions.Count <= i) continue;
                        x.expanderGrid.ColumnDefinitions[i].Width = new GridLength(newWidth);
                        if (x.dockPanelGrid != null)
                        {
                            x.dockPanelGrid.ColumnDefinitions[i].Width = new GridLength(newWidth);
                        }
                    }
                });
        }
        private static void SortOut(ExpanderListView expanderListView, Grid columnHeaders, ListView contentView, object sender, MouseButtonEventArgs e)
        {
            // Try to get the StackPanel and Path from the sender
            if (sender is not StackPanel stackPanel) return;
            if (stackPanel.Children[1] is not Path icon) return;
            // Get the column index of the StackPanel
            int columnIndex = Grid.GetColumn(stackPanel);
            // Parse geometries once
            var defaultGeometry = Geometry.Parse(IconTypes.Default);
            var sortUpGeometry = Geometry.Parse(IconTypes.SortUp);
            var sortDownGeometry = Geometry.Parse(IconTypes.SortDown);
            // Reset the icons of other columns
            foreach (var item in columnHeaders.Children)
            {
                if (item is StackPanel stack && Grid.GetColumn(stack) != columnIndex)
                {
                    if (stack.Children[1] is not Path stackIcon) return;
                    stackIcon.Data = defaultGeometry;
                }
            }
            // Update the icon and sort the column
            if (icon.Data.ToString() == defaultGeometry.ToString())
            {
                icon.Data = sortDownGeometry;
                SortColumn(expanderListView, contentView, columnIndex, false, columnHeaders);
            }
            else if (icon.Data.ToString() == sortUpGeometry.ToString())
            {
                icon.Data = sortDownGeometry;
                SortColumn(expanderListView, contentView, columnIndex, false, columnHeaders);
            }
            else
            {
                icon.Data = sortUpGeometry;
                SortColumn(expanderListView, contentView, columnIndex, true, columnHeaders);
            }
        }
        private static void SortColumn(ExpanderListView expanderListView, ListView contentView, int columnIndex, bool ascending, Grid columnHeaders)
        {
            // Get the items from the ListView
            var itemsList = _localContentSource?
                .OfType<ExpanderListMenuItem>()
                .Select(item => (item, container: contentView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem))
                .Where(x => x.container != null)
                .Select(x => (x.item, x.container, listMenuItemGrid: x.container != null ? expanderListView.FindVisualChild<Grid>(x.container) : null))
                .Where(x => x.listMenuItemGrid != null)
                .Select(x => (x.item, x.container, expander: x.container != null ? expanderListView.FindVisualChild<Expander>(x.container) : null))
                .Where(x => x.expander != null)
                .Select(x => (x.item, x.container, dockPanel: x.container != null ? expanderListView.FindVisualChild<DockPanel>(x.container) : null))
                .Where(x => x.dockPanel != null)
                .Select(x => (x.item, x.container, dockPanelGrid: x.dockPanel != null ? expanderListView.FindVisualChild<Grid>(x.dockPanel) : null))
                .Where(x => x.dockPanelGrid != null)
                .Select(x => x.dockPanelGrid != null ? GetColumnChildren(x.dockPanelGrid, columnIndex) : null)
                .Where(x => x != null)
                .Select(x => x?.ToString())
                .Distinct()
                .ToList();
            if (itemsList == null) return;
            // Sort the items
            var sortedItems = ascending ? itemsList.OrderBy(x => x).ToList() : itemsList.OrderByDescending(x => x).ToList();
            // Clear the Items collection
            contentView.Items.Clear();
            // Get the properties of ExpanderListMenuItem
            var properties = typeof(ExpanderListMenuItem).GetProperties();
            if (properties.Length <= columnIndex) return;
            // Add the items back in sorted order
            foreach (var item in sortedItems)
            {
                // Find all ExpanderListMenuItem objects with the matching property value
                var menuItems = _localContentSource?
                    .OfType<ExpanderListMenuItem>()
                    .Where(x => properties.ToList().Find(x => x.Name == (columnHeaders.Children[columnIndex * 2] as StackPanel).FindLogicalChildren<TextBlock>().FirstOrDefault()?.Text)?.GetValue(x)?.ToString() == item)
                    .OrderBy(x => properties[columnIndex].GetValue(x)?.ToString());
                // Add the item to the Items collection
                if (menuItems == null) continue;
                foreach (var menuItem in menuItems) contentView.Items.Add(menuItem);
            }
        }
        private static string? GetColumnChildren(Grid grid, int columnIndex)
        {
            // Find the first child in the specified column
            var child = grid.Children
                .OfType<UIElement>()
                .FirstOrDefault(x => Grid.GetColumn(x) == columnIndex);

            // Check if the child is a StackPanel or TextBlock
            if (child is StackPanel stackPanel)
            {
                // Try to get the TextBlock from the StackPanel
                var textBlock = stackPanel.Children[1] as TextBlock;
                return textBlock?.Text;
            }
            else if (child is TextBlock textBlock)
            {
                return textBlock.Text;
            }
            return null;
        }
        private static void CreateRowContent(ExpanderListView expanderListView, ListView contentView, ExpanderListMenuItem item, Grid columnHeaders, int columnIndex, int index)
        {
            var container = contentView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
            contentView.ItemContainerGenerator.StatusChanged += (sender, e) =>
            {
                if (contentView.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
                container = contentView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container == null) return;
                if (container.IsLoaded)
                {
                    CreateRowContent(expanderListView, container, item, columnHeaders, columnIndex, index);
                    return;
                }
                container.Loaded += (sender, e) =>
                {
                    // Use the Dispatcher to invoke the CreateRowContent method at a later time
                    container.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        CreateRowContent(expanderListView, container, item, columnHeaders, columnIndex, index);
                    }));
                };
            };
            if (container != null) CreateRowContent(expanderListView, container, item, columnHeaders, columnIndex, index);
        }
        private static void CreateRowContent(ExpanderListView expanderListView, ListViewItem container, ExpanderListMenuItem item, Grid columnHeaders, int columnIndex, int index)
        {
            // Check if container is not null
            if (container == null) return;
            // Try to get expander and dockPanel
            var expander = expanderListView.FindVisualChild<Expander>(container);
            var dockPanel = expanderListView.FindVisualChild<DockPanel>(container);
            // Check if expander and dockPanel are not null
            if (expander == null || dockPanel == null) return;
            // Set expander.Width and dockPanel.Width
            expander.Width = columnHeaders.ActualWidth;
            dockPanel.Width = columnHeaders.ActualWidth;
            // Set expander.IsExpanded
            expander.IsExpanded = expanderListView.IsToggled;
            // Subscribe to expander.Collapsed event
            expander.Collapsed += (sender, e) =>
            {
                _isChildCollapsed = true;
                expanderListView.IsToggled = false;
                _isChildCollapsed = false;
            };
            // Subscribe to expander.Expanded event
            expander.Expanded += (sender, e) =>
            {
                // Check if all expanders are expanded
                bool allExpanded = expanderListView.FindVisualChildren<Expander>()
                    .All(x => x.IsExpanded == true);
                // Update expanderListView.IsToggled if all expanders are expanded
                if (allExpanded)
                    expanderListView.IsToggled = true;
            };
            // Try to get header, expanderGrid, and dockPanelGrid
            var dockPanelGrid = dockPanel != null ? expanderListView.FindVisualChild<Grid>(dockPanel) : null;
            // Check if header, expanderGrid, and dockPanelGrid are not null
            if (expander.Header is not StackPanel header || expander.Content is not Grid expanderGrid || dockPanelGrid == null) return;
            // Try to add masterCheckBox
            var masterCheckBox = AddMasterCheckBox(header, expanderGrid);
            // Get properties of ExpanderListMenuItem class
            var properties = typeof(ExpanderListMenuItem).GetProperties();
            // Check if columnIndex is within bounds
            if (columnIndex >= properties.Length) return;
            // Try to get listMenuItemsProperty and listMenuItems
            var listMenuItemsProperty = properties.FirstOrDefault(p => p.Name == "ListMenuItems");
            // Check if listMenuItems is not null
            if (listMenuItemsProperty?.GetValue(item) is not IList<ExpanderListMenuItem> listMenuItems)
            {
                AddRow(expanderListView, columnIndex, columnHeaders, expanderGrid, properties, item, index, masterCheckBox);
                AddRow(expanderListView, columnIndex, columnHeaders, dockPanelGrid, properties, item, index);
                return;
            }
            for (int j = 0; j < listMenuItems.Count; j++)
            {
                // Add a new row to expanderGrid
                expanderGrid.RowDefinitions.Add(new RowDefinition());
                AddRow(expanderListView, columnIndex, columnHeaders, expanderGrid, properties, item, index, masterCheckBox, j);
                if (j == 0) AddRow(expanderListView, columnIndex, columnHeaders, dockPanelGrid, properties, item, index, isDockPanel: true);
            }
        }
        private static CheckBox? AddMasterCheckBox(StackPanel header, Grid grid)
        {
            // Try to get the masterCheckBox from the header
            if (header.Children[0] is not CheckBox masterCheckBox) return null;
            // Check if all child CheckBoxes are checked
            bool allChecked = grid.Children
                .OfType<StackPanel>()
                .SelectMany(x => x.Children.OfType<CheckBox>())
                .Any() && grid.Children
                .OfType<StackPanel>()
                .SelectMany(x => x.Children.OfType<CheckBox>())
                .All(x => x.IsChecked == true);
            // Update masterCheckBox if all child CheckBoxes are checked
            masterCheckBox.IsChecked = allChecked;
            // Subscribe to the Checked event of the masterCheckBox
            masterCheckBox.Checked += (sender, e) =>
            {
                // Check all child CheckBoxes
                foreach (var checkBox in grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(x => x.Children.OfType<CheckBox>())
                    .ToList()) // Create a copy of the collection
                {
                    if (checkBox.IsEnabled) checkBox.IsChecked = true;
                }
            };
            // Subscribe to the Unchecked event of the masterCheckBox
            masterCheckBox.Unchecked += (sender, e) =>
            {
                // Check if the masterCheckBox is being unchecked as a result of a child checkbox being unchecked
                if (_isChildUnchecked) return;

                // Uncheck all child CheckBoxes
                foreach (var checkBox in grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(x => x.Children.OfType<CheckBox>())
                    .ToList())
                {
                    if (checkBox.IsEnabled) checkBox.IsChecked = false;
                }
            };
            return masterCheckBox;
        }
        private static void AddRow(ExpanderListView expanderListView, int columnIndex, Grid columnHeaders, Grid grid, PropertyInfo[] properties, ExpanderListMenuItem item, int index, CheckBox? masterCheckBox = null, int rowIndex = 0, bool isDockPanel = false)
        {
            for (int i = 0; i < columnIndex + 1; i++)
            {
                if (columnHeaders.ColumnDefinitions.Count <= 0) return;
                double defaultColumnWidth = columnHeaders.ColumnDefinitions[i].ActualWidth;
                grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(defaultColumnWidth)
                });
                var menuItem = item.ListMenuItems == null || isDockPanel ? item : item.ListMenuItems.Count > 0 && item.ListMenuItems.Count > rowIndex ? item.ListMenuItems[rowIndex] : item;
                var text = properties.ToList().Find(x => x.Name == (columnHeaders.Children[i * 2] as StackPanel).FindLogicalChildren<TextBlock>().FirstOrDefault()?.Text)?.GetValue(menuItem);
                if (text == null) return;
                var textBlock = new TextBlock
                {
                    Text = text.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center
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
        private static void AddFirstCell(ExpanderListView expanderListView, ExpanderListMenuItem menuItem, Grid grid, CheckBox? masterCheckBox, TextBlock textBlock, int index, int columnIndex, int rowIndex = 0)
        {
            // Try to get contentViewMenuItem from expanderListView.ContentSource
            var contentViewMenuItem = expanderListView.ContentSource?
                .OfType<ExpanderListMenuItem>()
                .ElementAtOrDefault(index)?
                .ListMenuItems?
                .ElementAtOrDefault(rowIndex) ?? expanderListView.ContentSource?
                .OfType<ExpanderListMenuItem>()
                .ElementAtOrDefault(index);
            // Create a new StackPanel
            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };
            // Create a new CheckBox
            var checkBox = new CheckBox
            {
                Margin = new Thickness(30, 1, 1, 1),
                IsChecked = menuItem.IsChecked,
                IsEnabled = menuItem.IsEnabled,
                VerticalAlignment = VerticalAlignment.Center,
            };
            // Subscribe to the Loaded event of the checkBox
            checkBox.Loaded += (sender, e) =>
            {
                // Check if all child CheckBoxes are checked
                bool allChecked = grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(x => x.Children.OfType<CheckBox>())
                    .Any() && grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(x => x.Children.OfType<CheckBox>())
                    .All(x => x.IsChecked == true);
                // Update masterCheckBox if all child CheckBoxes are checked
                if (masterCheckBox != null)
                    masterCheckBox.IsChecked = allChecked;
            };
            // Subscribe to the Checked event of the checkBox
            checkBox.Checked += (sender, e) =>
            {
                OnCheckBoxToggled(expanderListView, menuItem, true);
                // Check if all child CheckBoxes are checked
                bool allChecked = grid.Children
                    .OfType<StackPanel>()
                    .SelectMany(x => x.Children.OfType<CheckBox>())
                    .All(x => x.IsChecked == true);
                // Update masterCheckBox if all child CheckBoxes are checked
                _isChildUnchecked = true;
                if (masterCheckBox != null)
                    masterCheckBox.IsChecked = allChecked;
                _isChildUnchecked = false;
                // Update menuItem.IsChecked and expanderListView.IsSaved
                menuItem.IsChecked = true;
                expanderListView.IsSaved = !(_localContentSource == null || expanderListView.ContentSource == null
                || !Enumerable.SequenceEqual(
                        _localContentSource.OfType<ExpanderListMenuItem>(),
                        expanderListView.ContentSource.OfType<ExpanderListMenuItem>(),
                        new ExpanderListMenuItemComparer()));
                // Check if contentViewMenuItem.IsChecked matches menuItem.IsChecked
                if (menuItem.IsChecked == expanderListView.ContentSource?.OfType<ExpanderListMenuItem>().Where(x => x.URL == menuItem.URL).FirstOrDefault()?.IsChecked)
                {
                    RemoveIcon(stackPanel, checkBox);
                    return;
                }
                AddIcon(IconTypes.Download, stackPanel, checkBox);
            };
            // Subscribe to the Unchecked event of the checkBox
            checkBox.Unchecked += (sender, e) =>
            {
                OnCheckBoxToggled(expanderListView, menuItem, false);
                // Set the flag to indicate that the masterCheckBox is being unchecked as a result of a child checkbox being unchecked
                _isChildUnchecked = true;
                if (masterCheckBox != null)
                    masterCheckBox.IsChecked = false;
                _isChildUnchecked = false;
                // Update menuItem.IsChecked and expanderListView.IsSaved
                menuItem.IsChecked = false;
                expanderListView.IsSaved = !(_localContentSource == null || expanderListView.ContentSource == null
                || !Enumerable.SequenceEqual(
                        _localContentSource.OfType<ExpanderListMenuItem>(),
                        expanderListView.ContentSource.OfType<ExpanderListMenuItem>(),
                        new ExpanderListMenuItemComparer()));
                // Check if contentViewMenuItem.IsChecked matches menuItem.IsChecked
                if (menuItem.IsChecked == expanderListView.ContentSource?.OfType<ExpanderListMenuItem>().Where(x => x.URL == menuItem.URL).FirstOrDefault()?.IsChecked)
                {
                    RemoveIcon(stackPanel, checkBox);
                    return;
                }
                AddIcon(IconTypes.Delete, stackPanel, checkBox);
            };
            // Add checkBox and textBlock to stackPanel
            stackPanel.Children.Add(checkBox);
            stackPanel.Children.Add(textBlock);
            // Set the column and row of stackPanel in grid
            Grid.SetColumn(stackPanel, columnIndex);
            Grid.SetRow(stackPanel, rowIndex);
            // Add stackPanel to grid
            grid.Children.Add(stackPanel);
        }

        private static void OnCheckBoxToggled(ExpanderListView expanderListView, ExpanderListMenuItem menuItem, bool isChecked)
        {
            // Raise the CheckBoxChanged event with the specified menuItem and isChecked value
            expanderListView.OnCheckBoxChanged(new CheckBoxChangedEventArgs(menuItem, isChecked));
        }

        private static void RemoveIcon(StackPanel stackPanel, CheckBox checkBox)
        {
            stackPanel.Children.RemoveAt(0);
            checkBox.Margin = new Thickness(30, 1, 1, 1);
        }
        private static void AddIcon(string path, StackPanel stackPanel, CheckBox? checkBox = null, int index = 0)
        {
            Path? icon = new()
            {
                Stretch = Stretch.Uniform,
                Fill = (Brush)Application.Current.FindResource("Editor.Disabled.FontBrush"),
                Margin = new Thickness(1),
                Height = 15,
                Width = 15,
                Data = Geometry.Parse(path)
            };
            stackPanel.Children.Insert(index, icon);
            if (checkBox == null) return;
            checkBox.Margin = new Thickness(13, 1, 1, 1);
        }
        private void GridSplitter_DragDelta(DragDeltaEventArgs e, int columnIndex = 0, Grid? columnHeaders = null, ListView? listView = null)
        {
            // Check if columnHeaders and listView are not null
            if (columnHeaders == null || listView == null) return;
            // Check if columnIndex is within bounds
            if (columnIndex >= columnHeaders.ColumnDefinitions.Count) return;
            // Calculate the new width
            var newWidth = columnHeaders.ColumnDefinitions[columnIndex].ActualWidth + e.HorizontalChange;
            // Check if newWidth is greater than minWidth
            if (newWidth <= columnHeaders.ColumnDefinitions[columnIndex].MinWidth) return;
            // Update the width of the columnHeader
            columnHeaders.ColumnDefinitions[columnIndex].Width = new GridLength(newWidth);
            // Update the width of expanderGrid and dockPanelGrid for each item in listView
            listView.Items
                .OfType<object>()
                .Select(item => listView.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem)
                .Where(x => x != null)
                .Select(x => (x, expander: x != null ? FindVisualChild<Expander>(x) : null))
                .Where(x => x.expander != null)
                .Select(x => (x.x, x.expander, dockPanel: x.x != null ? FindVisualChild<DockPanel>(x.x) : null))
                .Where(x => x.dockPanel != null)
                .Select(x => (x.x, x.expander, x.dockPanel, expanderGrid: x.expander?.Content as Grid))
                .Where(x => x.expanderGrid != null)
                .Select(x => (x.x, x.expander, x.dockPanel, x.expanderGrid, dockPanelGrid: x.dockPanel != null ? FindVisualChild<Grid>(x.dockPanel) : null))
                .ToList()
                .ForEach(x =>
                {
                    for (int i = 0; i < columnHeaders.ColumnDefinitions.Count; i++)
                    {
                        var newWidth = columnHeaders.ColumnDefinitions[i].ActualWidth;
                        if (x.expanderGrid == null) continue;
                        if (x.expanderGrid.ColumnDefinitions.Count <= i) continue;
                        x.expanderGrid.ColumnDefinitions[i].Width = new GridLength(newWidth);
                        if (x.dockPanelGrid != null)
                        {
                            x.dockPanelGrid.ColumnDefinitions[i].Width = new GridLength(newWidth);
                        }
                    }
                });
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
                    if (descendant == null) continue;
                    return descendant;
                }
            }
            return null;
        }
    }
    /// <summary>
    /// Useful for comparing between two instances of ExpanderListMenuItem
    /// </summary>
    public class ExpanderListMenuItemComparer : IEqualityComparer<ExpanderListMenuItem>
    {
        /// <inheritdoc/>
        public bool Equals(ExpanderListMenuItem? x, ExpanderListMenuItem? y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;
            if (x.Name != y.Name) return false;
            if (x.APILevel != y.APILevel) return false;
            if (x.Status != y.Status) return false;
            if (x.IsChecked != y.IsChecked) return false;
            if (x.ListMenuItems == null && y.ListMenuItems == null) return true;
            if (x.ListMenuItems == null || y.ListMenuItems == null) return false;
            if (x.IsEnabled != y.IsEnabled) return false;
            if (x.Revision != y.Revision) return false;
            if (x.Version != y.Version) return false;
            if (x.URL != y.URL) return false;
            return x.ListMenuItems.SequenceEqual(y.ListMenuItems, new ExpanderListMenuItemComparer());
        }
        /// <inheritdoc/>
        public int GetHashCode(ExpanderListMenuItem obj)
        {
            int hash = 17;
            hash = hash * 23 + (obj.Name?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.APILevel?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.Status?.GetHashCode() ?? 0);
            hash = hash * 23 + obj.IsChecked.GetHashCode();
            if (obj.ListMenuItems != null)
            {
                foreach (var item in obj.ListMenuItems)
                {
                    hash = hash * 23 + item.GetHashCode();
                }
            }
            hash = hash * 23 + obj.IsEnabled.GetHashCode();
            hash = hash * 23 + obj.Revision.GetHashCode();
            hash = hash * 23 + (obj.Version?.GetHashCode() ?? 0);
            hash = hash * 23 + (obj.URL?.GetHashCode() ?? 0);
            return hash;
        }
    }
    enum ContentViewMode
    {
        Expander,
        List
    }
    internal static class IconTypes
    {
        public static string Default = "M0 9 L10 0 L20 9 M0 11 L10 20 L20 11Z";
        public static string SortUp = "M0 9 L10 0 L20 9";
        public static string SortDown = "M0 11 L10 20 L20 11Z";
        public static string Delete = "M6 5H18M9 5V5C10.5769 3.16026 13.4231 3.16026 15 5V5M9 20H15C16.1046 20 17 19.1046 17 18V9C17 8.44772 16.5523 8 16 8H8C7.44772 8 7 8.44772 7 9V18C7 19.1046 7.89543 20 9 20Z";
        public static string Download = "M0 16q0 2.912 1.824 5.088t4.576 2.752q0.032 0 0.032-0.032v-0.064t0.032-0.032q0.544-1.344 1.344-2.176t2.208-1.184v-2.336q0-2.496 1.728-4.256t4.256-1.76 4.256 1.76 1.76 4.256v2.336q1.376 0.384 2.176 1.216t1.344 2.144l0.096 0.288h0.384q2.464 0 4.224-1.76t1.76-4.224v-2.016q0-2.464-1.76-4.224t-4.224-1.76q-0.096 0-0.32 0.032 0.32-1.152 0.32-2.048 0-3.296-2.368-5.632t-5.632-2.368q-2.88 0-5.056 1.824t-2.784 4.544q-1.152-0.352-2.176-0.352-3.296 0-5.664 2.336t-2.336 5.664v1.984zM10.016 25.824q-0.096 0.928 0.576 1.6l4 4q0.576 0.576 1.408 0.576t1.408-0.576l4-4q0.672-0.672 0.608-1.6-0.064-0.32-0.16-0.576-0.224-0.576-0.736-0.896t-1.12-0.352h-1.984v-5.984q0-0.832-0.608-1.408t-1.408-0.608-1.408 0.608-0.576 1.408v5.984h-2.016q-0.608 0-1.12 0.352t-0.736 0.896q-0.096 0.288-0.128 0.576z";
    }
    /// <summary>
    /// The event arguments which are passed when a checkbox is toggled
    /// </summary>
    public class CheckBoxChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The menuitem whose checkbox is toggled
        /// </summary>
        public ExpanderListMenuItem MenuItem { get; }
        /// <summary>
        /// The status of the checkbox in question
        /// </summary>
        public bool IsChecked { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckBoxChangedEventArgs"/> class.
        /// </summary>
        /// <param name="menuItem">The menuitem whose checkbox is toggled</param>
        /// <param name="isChecked">The checkbox's checked current status</param>
        public CheckBoxChangedEventArgs(ExpanderListMenuItem menuItem, bool isChecked)
        {
            MenuItem = menuItem;
            IsChecked = isChecked;
        }
    }
}
namespace PrimalEditor.Utilities
{
    /// <summary>
    /// The data structure required for transaction with ExpanderListView
    /// </summary>
    public class ExpanderListMenuItem
    {
        /// <summary>
        /// The name of the package
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// The API level attached to the package.
        /// </summary>
        public string? APILevel { get; set; }
        /// <summary>
        /// Displays the relevence of package; i.e. whether the package is installed or not and whether it needs updates
        /// </summary>
        public string? Status { get; set; }
        /// <summary>
        /// Whether the checkbox attached to the list item is checked
        /// </summary>
        public bool? IsChecked { get; set; }
        /// <summary>
        /// The version number of the package
        /// </summary>
        public int? Revision { get; set; }
        /// <summary>
        /// The version code of the tool
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// The site for verifying the relevance of package or tool
        /// </summary>
        public string? URL { get; set; }
        /// <summary>
        /// Declares whether the checkbox should be enabled or not
        /// </summary>
        public bool IsEnabled { get; set; }
        /// <summary>
        /// Used to create multiple menu items under one expander
        /// </summary>
        public List<ExpanderListMenuItem>? ListMenuItems { get; set; }
        /// <summary>
        /// Creates an exact replica of the ExpanderListMenuItem
        /// </summary>
        /// <returns>The exact replica of the supplied data</returns>
        public ExpanderListMenuItem DeepCopy()
        {
            var copy = new ExpanderListMenuItem
            {
                Name = Name,
                APILevel = APILevel,
                Status = Status,
                IsChecked = IsChecked,
                Revision = Revision,
                Version = Version,
                URL = URL,
                IsEnabled = IsEnabled,
                ListMenuItems = ListMenuItems?.Select(item => item.DeepCopy()).ToList()
            };
            return copy;
        }
    }
}