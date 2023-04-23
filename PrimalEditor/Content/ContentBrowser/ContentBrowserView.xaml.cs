using PrimalEditor.Editors;
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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is long size) ? SizeSuffix(size, 0) : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class PlainView : ViewBase
    {
        public static readonly DependencyProperty ItemContainerStyleProperty = ItemsControl.ItemContainerStyleProperty.AddOwner(typeof(PlainView));
        public Style ItemContainerStyle
        {
            get { return (Style)GetValue(ItemContainerStyleProperty); }
            set { SetValue(ItemContainerStyleProperty, value); }
        }
        public static readonly DependencyProperty ItemTemplateProperty=ItemsControl.ItemTemplateProperty.AddOwner(typeof(PlainView));
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        public static readonly DependencyProperty ItemWidthProperty = WrapPanel.ItemWidthProperty.AddOwner(typeof(PlainView));
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }
        public static readonly DependencyProperty ItemHeightProperty = WrapPanel.ItemHeightProperty.AddOwner(typeof(PlainView));
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        protected override object DefaultStyleKey => new ComponentResourceKey(GetType(), "PlainViewResourceId");
    }
    /// <summary>
    /// Interaction logic for ContentBrowserView.xaml
    /// </summary>
    public partial class ContentBrowserView : UserControl, IDisposable
    {
        private int _numberOfClicks = 0;
        private string _sortedProperty = nameof(ContentInfo.FileName);
        private ListSortDirection _sortDirection; 
        public SelectionMode SelectionMode
        {
            get => (SelectionMode)GetValue(SelectionModeProperty);
            set => SetValue(SelectionModeProperty, value);
        }
        public static readonly DependencyProperty SelectionModeProperty =
            DependencyProperty.Register(nameof(SelectionMode), typeof(SelectionMode), typeof(ContentBrowserView), new PropertyMetadata(SelectionMode.Extended));
        public FileAccess FileAccess
        {
            get => (FileAccess)GetValue(FileAccessProperty);
            set => SetValue(FileAccessProperty, value);
        }
        public static readonly DependencyProperty FileAccessProperty =
            DependencyProperty.Register(nameof(FileAccess), typeof(FileAccess), typeof(ContentBrowserView), new PropertyMetadata(FileAccess.ReadWrite)); 
        internal ObservableCollection<ContentInfo> SelectedItems
        {
            get { return (ObservableCollection<ContentInfo>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }
        public static readonly DependencyProperty SelectedItemsProperty = 
            DependencyProperty.Register("SelectedItems", typeof(ObservableCollection<ContentInfo>), typeof(ContentBrowserView), new PropertyMetadata(new ObservableCollection<ContentInfo>()));
        private List<object> _selectedItems = new List<object>();
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
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.DataContextChanged += OnProjectChanged;
            }
            OnProjectChanged(null, new DependencyPropertyChangedEventArgs(DataContextProperty, null, Project.Current));
            folderListView.AddHandler(Thumb.DragDeltaEvent, new DragDeltaEventHandler(Thumb_DragDelta), true);
            folderListView.Items.SortDescriptions.Add(new SortDescription(_sortedProperty, _sortDirection));
        }

        private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            if (e.OriginalSource is Thumb thumb && thumb.TemplatedParent is GridViewColumnHeader header)
            {
                if (header.Column.ActualWidth < 50)
                {
                    header.Column.Width = 50;
                }
                else if (header.Column.ActualWidth > 250)
                {
                    header.Column.Width = 250;
                }
            }
        }

        private void OnProjectChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
            if (e.NewValue is Project project)
            {
                Debug.Assert(e.NewValue==Project.Current);
                var contentBrowser = new ContentBrowser(project);
                contentBrowser.PropertyChanged += OnSelectedFolderChanged;
                DataContext= contentBrowser;
            }
        }
        private void OnSelectedFolderChanged(object sender, PropertyChangedEventArgs e)
        {
            var vm = sender as ContentBrowser;
            if (e.PropertyName == nameof(vm.SelectedFolder) && !string.IsNullOrEmpty(vm.SelectedFolder))
            {
                GeneratePathStackButtons();
            }
        }

        private void GeneratePathStackButtons()
        {
            var vm = DataContext as ContentBrowser;
            var path = Directory.GetParent(Path.TrimEndingDirectorySeparator(vm.SelectedFolder)).FullName;
            var contentPath = Path.TrimEndingDirectorySeparator(vm.ContentFolder);
            pathStack.Children.RemoveRange(1, pathStack.Children.Count - 1);
            if (vm.SelectedFolder == vm.ContentFolder) return;
            string[] paths = new string[3];
            string[] labels = new string[3];
            int i;
            for (i = 0; i < 3; ++i)
            {
                paths[i] = path;
                labels[i] = path[(path.LastIndexOf(Path.DirectorySeparatorChar) + 1)..];
                if (path == contentPath) break;
                path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            }
            if (i == 3) i = 2;
            for (; i >= 0; --i)
            {
                var btn = new Button()
                {
                    DataContext = paths[i],
                    Content = new TextBlock() { Text = labels[i], TextTrimming = TextTrimming.CharacterEllipsis }
                };
                pathStack.Children.Add(btn);
                if (i > 0) pathStack.Children.Add(new System.Windows.Shapes.Path());
            }
        }
        private void OnPathStack_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm.UpdatePathStack(vm.SelectedFolder);
            vm.SelectedFolder = (sender as Button).DataContext as string;
        }
        private void OnGridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var column = sender as GridViewColumnHeader;
            var sortBy = column.Tag.ToString();
            folderListView.Items.SortDescriptions.Clear();
            var newDir = ListSortDirection.Ascending;
            if (_sortedProperty == sortBy && _sortDirection == newDir)
            {
                newDir = ListSortDirection.Descending;
            }
            _sortDirection = newDir;
            _sortedProperty = sortBy;
            folderListView.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        private void OnContent_Item_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var info = (sender as FrameworkElement).DataContext as ContentInfo;
            if (info.IsToggled) return;
            ExecuteSelection(info);
        }
        private void OnContent_Item_KeyDown(object sender, KeyEventArgs e)
        {
            var info = (sender as FrameworkElement).DataContext as ContentInfo;
            if (e.Key == Key.Enter)
            {
                ExecuteSelection(info);
            }
        }
        private void ExecuteSelection(ContentInfo info)
        {
            if (info == null) return;
            if (info.IsDirectory)
            {
                var vm = DataContext as ContentBrowser;
                string currentSelectedFolder = vm.SelectedFolder;
                vm.SelectedFolder = info.FullPath;
                vm.UpdatePathStack(currentSelectedFolder);
            }
            else if (FileAccess.HasFlag(FileAccess.Read))
            {
                var assetInfo = Asset.GetAssetInfo(info.FullPath);
                if (assetInfo != null)
                {
                    OpenAssetEditor(assetInfo);
                }
            }
        }

        private IAssetEditor OpenAssetEditor(AssetInfo info)
        {
            IAssetEditor editor = null;
            try
            {
                switch (info.Type)
                {
                    case AssetType.Unknown: break;
                    case AssetType.Animation: break;
                    case AssetType.Audio: break;
                    case AssetType.Material: break;
                    case AssetType.Mesh:
                        editor = OpenEditorPanel<GeomteryEditorView>(info, info.Guid, "GeometryEditor");
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

        private IAssetEditor OpenEditorPanel<T>(AssetInfo info, Guid guid, string title) where T : FrameworkElement, new()
        {
            // First look for a window that's already open and is displaying the same asset.
            foreach (Window window in Application.Current.Windows)
            {
                if (window.Content is FrameworkElement content && content.DataContext is IAssetEditor editor && editor.Asset.Guid == info.Guid)
                {
                    window.Activate();
                    return editor;
                }
            }
            // If not already open in an asset editor, we create a new window and load the asset.
            var newEditor = new T();
            Debug.Assert(newEditor.DataContext is IAssetEditor);
            (newEditor.DataContext as IAssetEditor).SetAsset(info);
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
            if (vm.SelectedFolder != null && e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files?.Length > 0 && Directory.Exists(vm.SelectedFolder))
                {
                    _ = ContentHelper.ImportFilesAsync(files, vm.SelectedFolder);
                    e.Handled = true;
                }
            }
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
                vm.SelectedItems.RemoveAt(index);
            }
            foreach (var thing in e.AddedItems)
            {
                _selectedItems.Add(thing);
                var item = folderListView.SelectedItems[_selectedItems.IndexOf(thing)] as ContentInfo;
                SelectedItems.Add(item?.IsDirectory == true ? null : item);
                if (DataContext == null) continue;
                var vm = DataContext as ContentBrowser;
                vm.SelectedItems.Add(item.FullPath);
                vm.SelectedItemsInfo.Add(item);
            }
            _numberOfClicks = 0;
        }
        public void Dispose()
        {
            Loaded -= OnContentBrowserLoaded;
            if (Application.Current?.MainWindow != null)
            {
                Application.Current.MainWindow.DataContextChanged -= OnProjectChanged;
            }
            (DataContext as ContentBrowser)?.Dispose();
            DataContext = null;
        }
        private void OnLeft_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm.BackOperation();
        }
        private void OnRight_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm.ForwardOperation();
        }
        private void OnUp_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            vm.UpOperation();
        }
        private void OnHome_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ContentBrowser;
            if (vm.BackPathStack.Peek() == vm.SelectedFolder) return;
            vm.UpdatePathStack(vm.SelectedFolder);
            vm.SelectedFolder = vm.ContentFolder;
        }
        private void TextBoxBlockCombo_OnValueChanged(object sender, ValueChangedEventArgs e)
        {
            var info = (sender as FrameworkElement).DataContext as ContentInfo;
            if (info == null) return;
            if (e.Value == info.FileName) return;
            SystemOperations.Rename(e.Value, info.FullPath);
        }

        private void TextBoxBlockCombo_LeftMouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_numberOfClicks++ < 1) return;
            var vm = DataContext as ContentBrowser;
            CommandHelper.CallCommand(vm.RenameCommand);
            _numberOfClicks = 0;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var vm = DataContext as ContentBrowser;
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
                    default:
                        break;
                }
            }
        }
        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            menuItem.IsSubmenuOpen = true;
            Debug.WriteLine(menuItem.IsSubmenuOpen);
        }
        private void OnNewMenuItem_MouseEnter(object sender, MouseEventArgs e)
        {
            //newContextMenu.IsOpen = true;
        }

        private void OnAddItem_Button_Click(object sender, RoutedEventArgs e)
        {
            myContextMenu.IsOpen = true;
        }

        private void OnAddItemButton_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem item)
            {
                var vm = DataContext as ContentBrowser;
                switch (item.Header.ToString())
                {
                    case "Folder":
                        CommandHelper.CallCommand(vm.NewFolderCommand);
                        break;
                    case "Primitive Mesh":
                        var dlg = new PrimitiveMeshDialog();
                        dlg.ShowDialog();
                        break;
                    case "Asset":
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
