using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// A class that represents a <see cref="DownloadItem"/>
    /// </summary>
    public class DownloadItem : ViewModelBase
    {
        private string? _name;
        /// <summary>
        /// Gets or sets the name of the download item.
        /// </summary>
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string? _status;
        /// <summary>
        /// Gets or sets the status of the download item.
        /// </summary>
        public string? Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }
        private double _progress;
        /// <summary>
        /// Gets or sets the progress of the download item.
        /// </summary>
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress == value) return;
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        private Brush? _progressBarForeground;
        /// <summary>
        /// Gets or sets the foreground brush of the progress bar.
        /// </summary>
        public Brush? ProgressBarForeground
        {
            get => _progressBarForeground;
            set
            {
                if (_progressBarForeground == value) return;
                _progressBarForeground = value;
                OnPropertyChanged(nameof(ProgressBarForeground));
            }
        }
        private string? _url;
        /// <summary>
        /// Gets or sets the URL of the download item.
        /// </summary>
        public string? URL
        {
            get => _url;
            set
            {
                if (_url == value) return;
                _url = value;
                OnPropertyChanged(nameof(URL));
            }
        }
    }
    /// <summary>
    /// A class that represents a <see cref="DeleteItem"/>
    /// </summary>
    public class DeleteItem
    {
        /// <summary>
        /// Gets or sets the name of the delete item.
        /// </summary>
        public string? Name { get; set; }
        /// <summary>
        /// Gets or sets the URL of the delete item.
        /// </summary>
        public string? URL { get; set; }
    }
    /// <summary>
    /// A class that represents a <see cref="CommandLineMessage"/>
    /// </summary>
    public class CommandLineMessage
    {
        /// <summary>
        /// Gets or sets the text of the command line message.
        /// </summary>
        public string? Text { get; set; }
        /// <summary>
        /// Gets or sets the text of the command line message.
        /// </summary>
        public HorizontalAlignment HorizontalAlignment { get; set; }
        /// <summary>
        /// Gets or sets the background brush of the command line message.
        /// </summary>
        public Brush? Background { get; set; }
    }
    class Downloads : ViewModelBase
    {
        private static Downloads? _instance;
        private static readonly object _lock = new();
        internal static Downloads Instance
        {
            get
            {
                if (_instance == null) lock (_lock) _instance ??= new Downloads();
                return _instance;
            }
        }
        public Dictionary<ExpanderListMenuItem, bool>? ModifiedItems;
        private string _averageProgress = "0";
        public string AverageProgress
        {
            get => _averageProgress;
            set
            {
                if (_averageProgress == value) return;
                _averageProgress = value;
                OnPropertyChanged(nameof(AverageProgress));
            }
        }
        private List<DownloadItem> _downloadItems = new();
        public List<DownloadItem> DownloadItems
        {
            get => _downloadItems;
            set
            {
                if (_downloadItems == value) return;
                _downloadItems = value;
                OnPropertyChanged(nameof(DownloadItems));
            }
        }
        private List<DeleteItem> _deleteItems = new();
        public List<DeleteItem> DeleteItems
        {
            get => _deleteItems;
            set
            {
                if (value == _deleteItems) return;
                _deleteItems = value;
                OnPropertyChanged(nameof(DeleteItems));
            }
        }
        internal void GenerateDownloadQueue()
        {
            var finalOutput = new List<DownloadItem>();
            if (ModifiedItems == null) return;
            foreach (var item in ModifiedItems)
            {
                if (!item.Value) continue;
                finalOutput.Add(new DownloadItem
                {
                    Name = item.Key.Name,
                    URL = item.Key.URL
                });
            }
            DownloadItems = finalOutput;
        }
        internal void DeleteThenDownload(CommandOutputRelay commandOutputRelay)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(async () =>
            {
                await CommandHelper.RunCommandAsync(commandOutputRelay, $"cd {SDKManager.Instance.AndroidSDKLocation}");
                await CommandHelper.RunCommandAsync(commandOutputRelay, "cd cmdline-tools");
                await CommandHelper.RunCommandAsync(commandOutputRelay, "cd latest");
                await CommandHelper.RunCommandAsync(commandOutputRelay, "cd bin");
                if (ModifiedItems == null) return;
                foreach (var item in ModifiedItems) if (item.Value) await CommandHelper.RunCommandAsync(commandOutputRelay, $"sdkmanager --install \"{item.Key.URL}\""); else await CommandHelper.RunCommandAsync(commandOutputRelay, $"sdkmanager --uninstall \"{item.Key.URL}\"");
            }));
        }
        internal void GenerateDeleteQueue()
        {
            var finalOutput = new List<DeleteItem>();
            if (ModifiedItems == null) return;
            foreach (var item in ModifiedItems)
            {
                if (item.Value) continue;
                finalOutput.Add(new DeleteItem { Name = item.Key.Name, URL = item.Key.URL });
            }
            DeleteItems = finalOutput;
        }
    }
}
