using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace PrimalEditor.Utilities
{
    public class DownloadItem : ViewModelBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(nameof(Status));
                }
            }
        }
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged(nameof(Progress));
                }
            }
        }
        private Brush _progressBarForeground;
        public Brush ProgressBarForeground
        {
            get => _progressBarForeground;
            set
            {
                if (_progressBarForeground != value)
                {
                    _progressBarForeground = value;
                    OnPropertyChanged(nameof(ProgressBarForeground));
                }
            }
        }
        private string _url;
        public string URL
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    OnPropertyChanged(nameof(URL));
                }
            }
        }
    }
    public class DeleteItem
    {
        public string Name { get; set; }
        public string URL { get; set; }
    }
    public class CommandLineMessage
    {
        public string Text { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public Brush Background { get; set; }
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
        public Dictionary<ExpanderListMenuItem, bool> ModifiedItems = null;
        private string _averageProgress = "0";
        public string AverageProgress
        {
            get => _averageProgress;
            set
            {
                if (_averageProgress != value)
                {
                    _averageProgress = value;
                    OnPropertyChanged(nameof(AverageProgress));
                }
            }
        }
        private List<DownloadItem> _downloadItems = new List<DownloadItem>();
        public List<DownloadItem> DownloadItems
        {
            get => _downloadItems;
            set
            {
                if (_downloadItems != value)
                {
                    _downloadItems = value;
                    OnPropertyChanged(nameof(DownloadItems));
                }
            }
        }
        private List<DeleteItem> _deleteItems = new List<DeleteItem>();
        public List<DeleteItem> DeleteItems
        {
            get => _deleteItems;
            set
            {
                if (value != _deleteItems)
                {
                    _deleteItems = value;
                    OnPropertyChanged(nameof(DeleteItems));
                }
            }
        }
        private SolidColorBrush GenerateProgressBarForeground(double progress, double maximum = 100, double minimum = 0)
        {
            return new SolidColorBrush(Color.FromRgb((byte)(255 - ((progress / (maximum - minimum)) * 255)), (byte)((progress / (maximum - minimum)) * 255), 255));
        }
        internal void GenerateDownloadQueue()
        {
            var finalOutput = new List<DownloadItem>();
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
                foreach (var item in ModifiedItems) if (item.Value) await CommandHelper.RunCommandAsync(commandOutputRelay, $"sdkmanager --install \"{item.Key.URL}\""); else await CommandHelper.RunCommandAsync(commandOutputRelay, $"sdkmanager --uninstall \"{item.Key.URL}\"");
            }));
        }
        internal void GenerateDeleteQueue()
        {
            var finalOutput = new List<DeleteItem>();
            foreach (var item in ModifiedItems)
            {
                if (item.Value) continue;
                finalOutput.Add(new DeleteItem { Name = item.Key.Name, URL = item.Key.URL });
            }
            DeleteItems = finalOutput;
        }
    }
}
