using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PrimalEditor.Content
{
    sealed class ContentInfo : ViewModelBase
    {
        public static int IconWidth => 90;
        public byte[]? Icon { get; }
        public byte[]? IconSmall { get; }
        public string FullPath { get; }
        private string? _fileName;
        public string FileName
        {
            get => _fileName = Path.GetFileNameWithoutExtension(FullPath);
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged(nameof(FileName));
                }
            }
        }
        public bool IsDirectory { get; }
        public DateTime DateModified { get; }
        public long? Size { get; }
        private bool _isToggled = false;
        public bool IsToggled
        {
            get => _isToggled;
            set
            {
                if (_isToggled != value)
                {
                    _isToggled = value;
                    OnPropertyChanged(nameof(IsToggled));
                }
            }
        }
        public ContentInfo(string fullPath, byte[]? icon = null, byte[]? smallIcon = null, DateTime? lastModified = null, bool isToggled = false)
        {
            Debug.Assert(File.Exists(fullPath) || Directory.Exists(fullPath));
            var info = new FileInfo(fullPath);
            IsDirectory = ContentHelper.IsDirectory(fullPath);
            DateModified = lastModified ?? info.LastWriteTime;
            Size = IsDirectory ? (long?)null : info.Length;
            Icon = icon;
            IconSmall = smallIcon ?? icon;
            FullPath = fullPath;
            IsToggled = isToggled;
        }
    }
    class ContentBrowser : ViewModelBase, IDisposable
    {
        private readonly DelayEventTimer _refreshTimer = new DelayEventTimer(TimeSpan.FromMilliseconds(250));
        public string ContentFolder { get; }
        private readonly ObservableCollection<ContentInfo> _folderContent = new ObservableCollection<ContentInfo>();
        public ReadOnlyObservableCollection<ContentInfo> FolderContent { get; }
        private string? _selectedFolder;
        public string? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                if (_selectedFolder != value)
                {
                    _selectedFolder = value;
                    if (!string.IsNullOrEmpty(_selectedFolder))
                    {
                        _ = GetFolderContent();
                    }
                    OnPropertyChanged(nameof(SelectedFolder));
                }
            }
        }
        private List<string> _selectedItems = new List<string>();
        public List<string> SelectedItems
        {
            get => _selectedItems;
            set
            {
                if (_selectedItems != value)
                {
                    _selectedItems = value;
                    OnPropertyChanged(nameof(SelectedItems));
                }
            }
        }
        private List<ContentInfo> _selectedItemsInfo = new List<ContentInfo>();
        public List<ContentInfo> SelectedItemsInfo
        {
            get => _selectedItemsInfo;
            set
            {
                if (_selectedItemsInfo != value)
                {
                    _selectedItemsInfo = value;
                    OnPropertyChanged(nameof(SelectedItemsInfo));
                }
            }
        }
        private Stack<string> _backPathStack = new Stack<string>();
        public Stack<string> BackPathStack
        {
            get => _backPathStack;
            set
            {
                if (_backPathStack != value)
                {
                    _backPathStack = value;
                    OnPropertyChanged(nameof(BackPathStack));
                }
            }
        }
        private Stack<string> _frontPathStack = new Stack<string>();
        public Stack<string> FrontPathStack
        {
            get => _frontPathStack;
            set
            {
                if (_frontPathStack != value)
                {
                    _frontPathStack = value;
                    OnPropertyChanged(nameof(FrontPathStack));
                }
            }
        }
        private bool _isBackButtonEnabled = false;
        public bool IsBackButtonEnabled
        {
            get => _isBackButtonEnabled;
            set
            {
                if (_isBackButtonEnabled != value)
                {
                    _isBackButtonEnabled = value;
                    OnPropertyChanged(nameof(IsBackButtonEnabled));
                }
            }
        }
        private bool _isFrontButtonEnabled = false;
        public bool IsFrontButtonEnabled
        {
            get => _isFrontButtonEnabled;
            set
            {
                if (_isFrontButtonEnabled != value)
                {
                    _isFrontButtonEnabled = value;
                    OnPropertyChanged(nameof(IsFrontButtonEnabled));
                }
            }
        }
        private bool _isUpButtonEnabled = false;
        public bool IsUpButtonEnabled
        {
            get => _isUpButtonEnabled;
            set
            {
                if (_isUpButtonEnabled != value)
                {
                    _isUpButtonEnabled = value;
                    OnPropertyChanged(nameof(IsUpButtonEnabled));
                }
            }
        }
        public ICommand? CopyCommand { get; private set; }
        public ICommand? CutCommand { get; private set; }
        public ICommand? PasteCommand { get; private set; }
        public ICommand? TemporaryDeleteCommand { get; private set; }
        public ICommand? PermanentDeleteCommand { get; private set; }
        public ICommand? BackCommand { get; private set; }
        public ICommand? ForwardCommand { get; private set; }
        public ICommand? UpCommand { get; private set; }
        public ICommand? RenameCommand { get; private set; }
        public ICommand? NewFolderCommand { get; internal set; }

        internal void SetCommands()
        {
            CopyCommand = new RelayCommand<object>(x => SystemOperations.LocalCopy(SelectedItems));
            CutCommand = new RelayCommand<object>(x => SystemOperations.LocalCut(SelectedItems));
            TemporaryDeleteCommand = new RelayCommand<object>(x => SystemOperations.Delete(SelectedItems));
            PermanentDeleteCommand = new RelayCommand<object>(x => SystemOperations.Delete(SelectedItems, true));
            BackCommand = new RelayCommand<object>(x => BackOperation(), x => IsBackButtonEnabled);
            ForwardCommand = new RelayCommand<object>(x => ForwardOperation(), x => IsFrontButtonEnabled);
            UpCommand = new RelayCommand<object>(x => UpOperation(), x => IsUpButtonEnabled);
            RenameCommand = new RelayCommand<object>(x =>
            {
                var lastItem = SelectedItemsInfo.LastOrDefault();
                if (lastItem != null)
                {
                    lastItem.IsToggled = true;
                }
            });
            if (SelectedFolder != null)
            {
                NewFolderCommand = new RelayCommand<object>(x => SystemOperations.CreateNewFolder(SelectedFolder));
                PasteCommand = new RelayCommand<object>(x => SystemOperations.Paste(SelectedFolder));
            }
            OnPropertyChanged(nameof(CopyCommand));
            OnPropertyChanged(nameof(CutCommand));
            OnPropertyChanged(nameof(PasteCommand));
            OnPropertyChanged(nameof(TemporaryDeleteCommand));
            OnPropertyChanged(nameof(PermanentDeleteCommand));
            OnPropertyChanged(nameof(BackCommand));
            OnPropertyChanged(nameof(ForwardCommand));
            OnPropertyChanged(nameof(UpCommand));
            OnPropertyChanged(nameof(RenameCommand));
        }
        internal void UpOperation()
        {
            if (SelectedFolder == null) return;
            UpdatePathStack(SelectedFolder);
            DirectoryInfo? parentDirectory = Directory.GetParent(SelectedFolder);
            if (parentDirectory == null) return;
            SelectedFolder = parentDirectory.FullName;
            UpdateAccessibilityStatus();
        }

        internal void BackOperation()
        {
            if (BackPathStack.Count < 1) return;
            if (SelectedFolder == null) return;
            FrontPathStack.Push(SelectedFolder);
            SelectedFolder = BackPathStack.Pop();
            UpdateAccessibilityStatus();
        }

        internal void ForwardOperation()
        {
            if (FrontPathStack.Count < 1) return;
            if (SelectedFolder == null) return;
            BackPathStack.Push(SelectedFolder);
            SelectedFolder = FrontPathStack.Pop();
            UpdateAccessibilityStatus();
        }
        internal void HomeOperation()
        {
            if (BackPathStack.Count <= 0) return;
            if (BackPathStack.Peek() == SelectedFolder) return;
            if (SelectedFolder == null) return;
            UpdatePathStack(SelectedFolder);
            SelectedFolder = ContentFolder;
            UpdateAccessibilityStatus();
        }
        internal void UpdatePathStack(string path)
        {
            Debug.Assert(path != null);
            Debug.Assert(path.Length > 0);
            Debug.Assert(!string.IsNullOrEmpty(path));
            BackPathStack.Push(path);
            FrontPathStack.Clear();
            UpdateAccessibilityStatus();
        }
        internal void UpdateAccessibilityStatus()
        {
            if (FrontPathStack.Count < 1)
            {
                IsFrontButtonEnabled = false;
            }
            else
            {
                IsFrontButtonEnabled = true;
            }
            if (SelectedFolder != ContentFolder)
            {
                IsUpButtonEnabled = true;
            }
            else
            {
                IsUpButtonEnabled = false;
            }
            if (BackPathStack.Count < 1)
            {
                IsBackButtonEnabled = false;
                return;
            }
            IsBackButtonEnabled = true;
        }
        private void OnContentModified(object? sender, ContentModifiedEventArgs e)
        {
            if (Path.GetDirectoryName(e.FullPath) != SelectedFolder) return;
            _refreshTimer.Trigger();
        }
        private void Refresh(object? sender, DelayEventTimerArgs e)
        {
            _ = GetFolderContent();
        }
        private async Task GetFolderContent()
        {
            var folderContent = new List<ContentInfo>();
            await Task.Run(() =>
            {
                if (SelectedFolder == null) return;
                folderContent = GetFolderContent(SelectedFolder);
            });
            _folderContent.Clear();
            folderContent.ForEach(x => _folderContent.Add(x));
        }

        private List<ContentInfo> GetFolderContent(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            var folderContent = new List<ContentInfo>();
            try
            {
                // Get sub-folder
                foreach (var dir in Directory.GetDirectories(path))
                {
                    folderContent.Add(new ContentInfo(dir));
                }
                // Get files
                foreach (var file in Directory.GetFiles(path, $"*{Asset.AssetFileExtension}"))
                {
                    var fileInfo = new FileInfo(file);
                    folderContent.Add(ContentInfoCache.Add(file));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return folderContent;
        }
        public void Dispose()
        {
            ContentWatcher.ContentModified -= OnContentModified;
            ContentInfoCache.Save();
        }

        public ContentBrowser(Project project)
        {
            Debug.Assert(project != null);
            var contentFolder = project.ContentPath;
            Debug.Assert(!string.IsNullOrEmpty(contentFolder.Trim()));
            contentFolder = Path.TrimEndingDirectorySeparator(contentFolder);
            ContentFolder = contentFolder;
            SelectedFolder = contentFolder;
            FolderContent = new ReadOnlyObservableCollection<ContentInfo>(_folderContent);
            ContentWatcher.ContentModified += OnContentModified;
            _refreshTimer.Triggered += Refresh;
            SetCommands();
        }
    }
}
