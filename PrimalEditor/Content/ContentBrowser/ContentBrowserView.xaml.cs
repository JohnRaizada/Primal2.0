﻿using PrimalEditor.Editors;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
namespace PrimalEditor.Content
{
    class DataSizeToStringConverter : IValueConverter
    {
        static readonly string[] _sizeSuffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        static string SizeSuffix(long value, int decimalPlaces = 1)
        {
            if (value <= 0 || decimalPlaces < 0) return string.Empty;
            // mag is 0 for bytes, 1 for KB, 2 for MB, etc.
            int mag = (int)Math.Log(value, 1024);
            // 1L << (mag * 10) == 2 ^ (10 * mag)
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));
            // make adjustment when the value is large enough that it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }
            return string.Format("{0:n" + decimalPlaces + "} {1}", adjustedSize, _sizeSuffixes[mag]);
        }
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => (value is long size) ? SizeSuffix(size, 0) : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl, IDisposable
    {
        private int _numberOfClicks = 0;
        private string _sortedProperty = nameof(ContentInfo.FileName);
        private ListSortDirection _sortDirection;
        /// <summary>
        /// Gets or sets the selection mode for the content browser.
        /// </summary>
        public SelectionMode SelectionMode
        {
            get => (SelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }
        /// <summary>
        /// Property associated with selection mode of the items.
        /// </summary>
        /// <remarks>
        /// Selection mode can be anyone of Single, Multiple and Extended
        /// </remarks>
        public static readonly DependencyProperty SelectionModeProperty = DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(ContentBrowserView), new PropertyMetadata(SelectionMode.Extended));
        /// <summary>
        /// Gets or sets the file access mode for the content browser.
        /// </summary>
        public FileAccess FileAccess
        {
            get => (FileAccess)GetValue(FileAccessProperty);
            set => SetValue(FileAccessProperty, value);
        }
        /// <summary>
        /// Property associated with File Access of the items.
        /// </summary>
        /// <remarks>
        /// File Access can be anyone of Read, Write and ReadWrite
        /// </remarks>
        public static readonly DependencyProperty FileAccessProperty = DependencyProperty.Register(nameof(FileAccess), typeof(FileAccess), typeof(ContentBrowserView), new PropertyMetadata(FileAccess.ReadWrite));
        internal ObservableCollection<ContentInfo> SelectedItems
        {
            get { return (ObservableCollection<ContentInfo>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        /// <summary>
        /// Property associated with Selected Item.
        /// </summary>
        /// <remarks>
        /// It is useful in accessing and modifying the properties associated with selected items.
        /// </remarks>
        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<ContentInfo>), typeof(ContentBrowserView), new PropertyMetadata(new ObservableCollection<ContentInfo>()));
        private readonly List<object> _selectedItems = new();
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentBrowserView"/> class.
        /// </summary>
        public ContentBrowserView()
        {
            DataContext = null;
            InitializeComponent();
            Loaded += OnContentBrowserLoaded;
            AllowDrop = true;
        }
        private void OnContentBrowserLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnContentBrowserLoaded;
            if (Application.Current?.MainWindow != null) Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
            OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
            folderListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
            folderListView.Items.SortDescriptions.Add(new SortDescription(_sortedProperty, _sortDirection));
        }
        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.OriginalSource is not Thumb thumb || thumb.TemplatedParent is not GridViewColumnHeader header) return;
            if (header.Column.ActualWidth < 50) header.Column.Width = 50;
            else if (header.Column.ActualWidth > 250) header.Column.Width = 250;
        }
        private void OnProjectChanged(object? sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
            if (e.NewValue is not Project project) return;
            Debug.Assert(e.NewValue == Project.Current);
            var contentBrowser = new ContentBrowser(project);
            contentBrowser.PropertyChanged += OnSelectedFolderChanged;
            DataContext = contentBrowser;
        }
        private void OnSelectedFolderChanged(object? sender, PropertyChangedEventArgs e)
        {
            var vm = sender as ContentBrowser;
            if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm?.SelectedFolder)) GeneratePathStackButtons();
        }
        private void GeneratePathStackButtons()
        {
            if (DataContext is not ContentBrowser vm) return;
            if (vm.SelectedFolder == null || vm.ContentFolder == null) return;
            var path = Path.TrimEndingDirectorySeparator(vm.SelectedFolder);
            var contentPath = Path.TrimEndingDirectorySeparator(vm.ContentFolder);
            pathStack.Children.Clear();
            if (vm.SelectedFolder == vm.ContentFolder) return;
            string[] paths = new string[3];
            string[] labels = new string[3];
            int i;
            for (i = 0; i < 3; ++i)
            {
                paths[i] = path;
                labels[i] = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                if (path == contentPath) break;
                path = path[..path.LastIndexOf(Path.DirectorySeparatorChar)];
            }
            if (i == 3) i = 2;
            for (; i >= 0; --i)
            {
                var btn = new Button()
                {
                    DataContext = paths[i],
                    Content = new TextBlock() { Text = labels[i], TextTrimming = TextTrimming.CharacterEllipsis },
                    Background = Brushes.Transparent,
                    Foreground = (Brush)Application.Current.FindResource("Editor.FontBrush")
                };
                pathStack.Children.Add(btn);
                if (i > 0) pathStack.Children.Add(new System.Windows.Shapes.Path());
            }
        }
        private void OnPathStack_Button_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is not ContentBrowser vm) return;
            vm.UpdatePathStack(vm.SelectedFolder);
            vm.SelectedFolder = (string)((Button)sender).DataContext;
        }
        private void OnGridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            string? sortBy = column?.Tag.ToString();
            if (sortBy == null) return;
            folderListView.Items.SortDescriptions.Clear();
            var newDir = ListSortDirection.Ascending;
            if (_sortedProperty == sortBy && _sortDirection == newDir) newDir = ListSortDirection.Descending;
            _sortDirection = newDir;
            _sortedProperty = sortBy;
            folderListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }
        private void OnContent_Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not ContentInfo info) return;
            if (info.IsToggled) return;
            ExecuteSelection(info);
        }
        private void OnContent_Item_KeyDown(object sender, KeyEventArgs e)
        {
            if (((FrameworkElement)sender).DataContext is not ContentInfo info) return;
            if (e.Key == Key.Enter) ExecuteSelection(info);
        }
        private void ExecuteSelection(ContentInfo info)
        {
            if (info == null) return;
            if (info.IsDirectory)
            {
                if (DataContext is not ContentBrowser vm) return;
                string? currentSelectedFolder = vm.SelectedFolder;
                vm.SelectedFolder = info.FullPath;
                vm.UpdatePathStack(currentSelectedFolder);
            }
            else if (FileAccess.HasFlag(FileAccess.Read))
            {
                var assetInfo = Asset.GetAssetInfo(info.FullPath);
                if (assetInfo != null) OpenAssetEditor(assetInfo);
            }
        }
        private static IAssetEditor? OpenAssetEditor(AssetInfo info)
        {
            IAssetEditor? editor = null;
            try
            {
                switch (info.Type)
                {
                    case AssetType.Unknown: break;
                    case AssetType.Animation: break;
                    case AssetType.Audio: break;
                    case AssetType.Material: break;
                    case AssetType.Mesh:
                        editor = OpenEditorPanel<GeomteryEditorView>(info, "GeometryEditor");
                        break;
                    case AssetType.Skeleton: break;
                    case AssetType.Texture: break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return editor;
        }
        private static IAssetEditor? OpenEditorPanel<T>(AssetInfo info, string title) where T : FrameworkElement, new()
        {
            // First look for a window that's already open and is displaying the same asset.
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is not FrameworkElement content || content.DataContext is not IAssetEditor editor || editor.Asset.Guid != info.Guid) continue;
                window.Activate();
                return editor;
            }
            // If not already open in an asset editor, we create a new window and load the asset.
            var newEditor = new T();
            Debug.Assert(newEditor.DataContext is IAssetEditor);
            ((IAssetEditor)newEditor.DataContext).SetAsset(info);
            var win = new Window()
            {
                Content = newEditor,
                Title = title,
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Style = Application.Current.FindResource("PrimalWindowStyle") as Style
            };
            win.Show();
            return newEditor.DataContext as IAssetEditor;
        }
        private void OnFolderContent_ListView_Drop(object sender, DragEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            if ((vm?.SelectedFolder) == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (!(files?.Length > 0) || !Directory.Exists(vm.SelectedFolder)) return;
            _ = ContentHelper.ImportFilesAsync(files, vm.SelectedFolder);
            e.Handled = true;
        }
        private void OnFolderContent_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Remove the unselected items from the collection
            foreach (var thing in e.RemovedItems)
            {
                var index = _selectedItems.IndexOf(thing);
                SelectedItems.RemoveAt(index);
                _selectedItems.Remove(thing);
                if (DataContext == null) continue;
                var vm = DataContext as ContentBrowser;
                vm?.SelectedItems.RemoveAt(index);
            }
            foreach (var thing in e.AddedItems)
            {
                _selectedItems.Add(thing);
                if (folderListView.SelectedItems[_selectedItems.IndexOf(thing)] is not ContentInfo item) continue;
                if (item.IsDirectory) continue;
                SelectedItems.Add(item);
                if (DataContext == null) continue;
                var vm = DataContext as ContentBrowser;
                if (item == null) continue;
                vm?.SelectedItems.Add(item.FullPath);
                vm?.SelectedItemsInfo.Add(item);
            }
            _numberOfClicks = 0;
        }
        /// <inheritdoc/>
        public void Dispose()
        {
            Loaded -= OnContentBrowserLoaded;
            if (Application.Current?.MainWindow != null) Application.Current.MainWindow.DataContextChanged -= OnProjectChanged;
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
        }
        private void OnLeft_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm?.BackOperation();
        }
        private void OnRight_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm?.ForwardOperation();
        }
        private void OnUp_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm?.UpOperation();
        }
        private void OnHome_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm?.HomeOperation();
        }
        private void TextBoxBlockCombo_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (((FrameworkElement)sender)?.DataContext is not ContentInfo info) return;
            if (e.Value == info.FileName) return;
            SystemOperations.Rename(e.Value, info.FullPath);
        }
        private void TextBoxBlockCombo_LeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_numberOfClicks++ < 1) return;
            if (DataContext is not ContentBrowser vm) return;
            CommandHelper.CallCommand(vm.RenameCommand);
            _numberOfClicks = 0;
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;
            if (DataContext is not ContentBrowser vm) return;
            switch (menuItem.Header.ToString())
            {
                case "Copy":
                    if (vm.SelectedItems.Count == 0) return;
                    CommandHelper.CallCommand(vm.CopyCommand);
                    break;
                case "Cut":
                    if (vm.SelectedItems.Count == 0) return;
                    CommandHelper.CallCommand(vm.CutCommand);
                    break;
                case "Paste":
                    CommandHelper.CallCommand(vm.PasteCommand);
                    break;
                case "Delete":
                    if (vm.SelectedItems.Count == 0) return;
                    CommandHelper.CallCommand(vm.TemporaryDeleteCommand);
                    break;
                case "Rename":
                    if (vm.SelectedItems.Count == 0) return;
                    CommandHelper.CallCommand(vm.RenameCommand);
                    break;
                case "Asset":
                    MessageBox.Show("This is the welcome message");
                    break;
                case "Folder":
                    CommandHelper.CallCommand(vm.NewFolderCommand);
                    break;
                case "Primitive Mesh":
                    var dlg = new PrimitiveMeshDialog();
                    dlg.ShowDialog();
                    break;
                default: break;
            }
        }
        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem) return;
            menuItem.IsSubmenuOpen = true;
            Debug.WriteLine(menuItem.IsSubmenuOpen);
        }
        private void OnAddItem_Button_Click(object sender, RoutedEventArgs e) => myContextMenu.IsOpen = true;
        private void OnAddItemButton_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem item) return;
            if (DataContext is not ContentBrowser vm) return;
            switch (item.Header.ToString())
            {
                case "Folder":
                    CommandHelper.CallCommand(vm.NewFolderCommand);
                    break;
                case "Primitive Mesh":
                    var dlg = new PrimitiveMeshDialog();
                    dlg.ShowDialog();
                    break;
                case "Asset": break;
                default: break;
            }
        }
    }
}
