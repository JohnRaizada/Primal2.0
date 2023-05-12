using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Xml.Linq;

namespace PrimalEditor.Utilities
{
    enum Platforms
    {
        Android,
        ChromeOS,
        iOS,
        Mac,
        Windows,
        Linux
    }
    enum DataSourceType
    {
        Local,
        Web
    }
    /// <summary>
    /// Represents an Android package that can be downloaded and installed.
    /// </summary>
    public class AndroidPackage
    {
        /// <summary>
        /// Gets or sets the path of the package.
        /// </summary>
        public string? Path { get; set; }
        /// <summary>
        /// Gets or sets the display name of the package.
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Gets or sets the version of the package.
        /// </summary>
        public string? Version { get; set; }
        /// <summary>
        /// Gets or sets the API level of the package.
        /// </summary>
        public string? APILevel { get; set; }
        /// <summary>
        /// Gets or sets the revision of the package.
        /// </summary>
        public int? Revision { get; set; }
        /// <summary>
        /// Gets or sets the download URL of the package.
        /// </summary>
        public string? DownloadUrl { get; set; }
    }
    [DataContract(Name = "SDK Manager")]
    internal class SDKManager : ViewModelBase
    {
        private static SDKManager? _instance;
        private static readonly object _lock = new();
        internal static SDKManager Instance
        {
            get
            {
                if (_instance == null) lock (_lock) _instance ??= new SDKManager();
                return _instance;
            }
        }
        private void UpdateInstance(SDKManager? newInstance)
        {
            lock (_lock) _instance = newInstance;
        }
        private static List<AndroidPackage>? _packages = null;
        private static Dictionary<string, List<string>>? _versionMapping = null;
        private static readonly string _configurationFilePath = Path.Combine(MainWindow.PrimalPath, "PrimalEditor/Resources/SDKManager/SDKManager.primalconfig");
        private static readonly List<string> _commandOutput = new();
        private enum CommandMode
        {
            Vanilla,
            Installed,
            Updates,
        }
        private List<string> _androidPlatformHeadings = new()
        {
            nameof(ExpanderListMenuItem.Name),
            nameof(ExpanderListMenuItem.APILevel),
            nameof(ExpanderListMenuItem.Revision),
            nameof(ExpanderListMenuItem.Status)
        };
        [DataMember]
        public List<string> AndroidPlatformHeadings
        {
            get => _androidPlatformHeadings;
            set
            {
                if (_androidPlatformHeadings == value) return;
                _androidPlatformHeadings = value;
                OnPropertyChanged(nameof(AndroidPlatformHeadings));
            }
        }
        private List<string> _androidToolsHeadings = new()
        {
             nameof(ExpanderListMenuItem.Name),
             nameof(ExpanderListMenuItem.Version),
             nameof(ExpanderListMenuItem.Status)
        };
        [DataMember]
        public List<string> AndroidToolsHeadings
        {
            get => _androidToolsHeadings;
            set
            {
                if (_androidToolsHeadings == value) return;
                _androidToolsHeadings = value;
                OnPropertyChanged(nameof(AndroidToolsHeadings));
            }
        }
        private List<string> _androidUpdateSitesHeadings = new()
        {
            nameof(ExpanderListMenuItem.Name),
            nameof(ExpanderListMenuItem.URL)
        };
        [DataMember]
        public List<string> AndroidUpdateSitesHeadings
        {
            get => _androidUpdateSitesHeadings;
            set
            {
                if (value == _androidUpdateSitesHeadings) return;
                _androidUpdateSitesHeadings = value;
                OnPropertyChanged(nameof(AndroidUpdateSitesHeadings));
            }
        }
        private List<ExpanderListMenuItem>? _androidPlatformContent = null;
        [DataMember]
        public List<ExpanderListMenuItem>? AndroidPlatformContent
        {
            get => _androidPlatformContent;
            set
            {
                if (_androidPlatformContent == value) return;
                _androidPlatformContent = value;
                OnPropertyChanged(nameof(AndroidPlatformContent));
            }
        }
        private List<ExpanderListMenuItem>? _androidToolsContent = null;
        [DataMember]
        public List<ExpanderListMenuItem>? AndroidToolsContent
        {
            get => _androidToolsContent;
            set
            {
                if (_androidToolsContent == value) return;
                _androidToolsContent = value;
                OnPropertyChanged(nameof(AndroidToolsContent));
            }
        }
        private List<ExpanderListMenuItem>? _androidUpdateSitesContent = null;
        [DataMember]
        public List<ExpanderListMenuItem>? AndroidUpdateSitesContent
        {
            get => _androidUpdateSitesContent;
            set
            {
                if (_androidUpdateSitesContent == value) return;
                _androidUpdateSitesContent = value;
                OnPropertyChanged(nameof(AndroidUpdateSitesContent));
            }
        }
        private string? _androidSDKLocation = null;
        [DataMember]
        public string? AndroidSDKLocation
        {
            get => _androidSDKLocation;
            set
            {
                if (_androidSDKLocation == value) return;
                _androidSDKLocation = value;
                OnPropertyChanged(nameof(AndroidSDKLocation));
            }
        }
        private bool? _isAutoSyncEnabled = null;
        [DataMember]
        public bool? IsAutoSyncEnabled
        {
            get => _isAutoSyncEnabled;
            set
            {
                if (_isAutoSyncEnabled == value) return;
                _isAutoSyncEnabled = value;
                OnPropertyChanged(nameof(IsAutoSyncEnabled));
            }
        }
        private DateTime? _lastSyncTime = null;
        [DataMember]
        public DateTime? LastSyncTime
        {
            get => _lastSyncTime;
            set
            {
                if (_lastSyncTime == value) return;
                _lastSyncTime = value;
                OnPropertyChanged(nameof(LastSyncTime));
            }
        }
        private DataSourceType? _dataSource = null;
        [DataMember]
        public DataSourceType? DataSource
        {
            get => _dataSource;
            set
            {
                if (_dataSource == value) return;
                _dataSource = value;
                OnPropertyChanged(nameof(DataSource));
            }
        }
        private bool? _showObsoletePackages = null;
        [DataMember]
        public bool? ShowObsoletePackages
        {
            get => _showObsoletePackages;
            set
            {
                if (_showObsoletePackages == value) return;
                _showObsoletePackages = value;
                OnPropertyChanged(nameof(ShowObsoletePackages));
            }
        }
        private bool _isContentAvailable;
        public bool IsContentAvailable
        {
            get => _isContentAvailable;
            set
            {
                if (_isContentAvailable == value) return;
                _isContentAvailable = value;
                OnPropertyChanged(nameof(IsContentAvailable));
            }
        }
        private string _notificationText = string.Empty;
        public string NotificationText
        {
            get => _notificationText;
            set
            {
                if (_notificationText == value) return;
                _notificationText = value;
                OnPropertyChanged(nameof(NotificationText));
            }
        }
        private bool _notificationTextVisibility = false;
        public bool IsNotificationTextVisible
        {
            get => _notificationTextVisibility;
            set
            {
                if (_notificationTextVisibility == value) return;
                _notificationTextVisibility = value;
                OnPropertyChanged(nameof(IsNotificationTextVisible));
            }
        }
        private bool _notificationReloadButtonVisibility = false;
        public bool IsNotificationReloadButtonVisible
        {
            get => _notificationReloadButtonVisibility;
            set
            {
                if (_notificationReloadButtonVisibility == value) return;
                _notificationReloadButtonVisibility = value;
                OnPropertyChanged(nameof(IsNotificationReloadButtonVisible));
            }
        }
        private bool _notificationRefreshButtonVisibility = false;
        public bool IsNotificationRefreshButtonVisible
        {
            get => _notificationRefreshButtonVisibility;
            set
            {
                if (_notificationRefreshButtonVisibility == value) return;
                _notificationRefreshButtonVisibility = value;
                OnPropertyChanged(nameof(IsNotificationRefreshButtonVisible));
            }
        }
        internal void Save() { Serializer.ToFile(this, Path.Combine(MainWindow.PrimalPath, "PrimalEditor/Resources/SDKManager/SDKManager.primalconfig")); }
        private static Dictionary<string, List<string>> GetSDKVersionMappingFromWeb()
        {
            //Dictionary<string, List<string>> x = new()
            //{
            //	{ "Api level 33", new List<string> { "Android 13", "13" } },
            //	{ "Api level 32", new List<string> { "Android 12", "12" } },
            //	{ "Api level 31", new List<string> { "Android 11", "11" } },
            //	{ "Api level 30", new List<string> { "Android 10", "10" } },
            //	{ "Api level 29", new List<string> { "Android 9", "9" } },
            //	{ "Api level 28", new List<string> { "Android 8", "8" } },
            //	{ "Api level 27", new List<string> { "Android 7", "7" } }
            //};
            //return x;
            var web = new HtmlWeb();
            var doc = web.Load("https://source.android.com/docs/setup/about/build-numbers");
            // Select the first table on the page
            var table = doc.DocumentNode.SelectSingleNode("//table");
            // Select the rows of the first table
            var rows = table.SelectNodes(".//tbody//tr");
            var dictionary = new Dictionary<string, List<string>>();
            foreach (var row in rows)
            {
                var cells = row.SelectNodes(".//td");
                if (cells != null && cells.Count == 3)
                {
                    var name = cells[0].InnerText;
                    var sdk = cells[1].InnerText;
                    var version = cells[2].InnerText.Trim();
                    dictionary.Add(version, new List<string> { name, sdk });
                }
            }
            return dictionary;
        }
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GenerateConsoleCtrlEvent(uint dwCtrlEvent, uint dwProcessGroupId);
        private async Task GenerateAndParseOutputFromCommandLineForAndroidAsync(CommandMode commandMode = CommandMode.Vanilla)
        {
            var currentWorkingDirectory = AndroidSDKLocation;
            if (!Directory.Exists(currentWorkingDirectory)) return;
            currentWorkingDirectory = Path.Combine(currentWorkingDirectory, "cmdline-tools");
            if (!Directory.Exists(currentWorkingDirectory)) return;
            currentWorkingDirectory = Path.Combine(currentWorkingDirectory, "latest");
            if (!Directory.Exists(currentWorkingDirectory)) return;
            currentWorkingDirectory = Path.Combine(currentWorkingDirectory, "bin");
            if (!Directory.Exists(currentWorkingDirectory)) return;
            if (!File.Exists(Path.Combine(currentWorkingDirectory, "sdkmanager.bat"))) return;
            var process = new Process();
            process.StartInfo.WorkingDirectory = currentWorkingDirectory;
            process.StartInfo.FileName = "cmd.exe";
            string command = "/C ";
            switch (commandMode)
            {
                case CommandMode.Vanilla:
                    command += "sdkmanager --list" + (ShowObsoletePackages == true ? " --include_obsolete" : "");
                    break;
                case CommandMode.Installed:
                    command += "sdkmanager --list_installed" + (ShowObsoletePackages == true ? " --include_obsolete" : "");
                    break;
                case CommandMode.Updates:
                    command += "sdkmanager --list --newer" + (ShowObsoletePackages == true ? " --include_obsolete" : "");
                    break;
                default: break;
            }
            process.StartInfo.Arguments = command;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();
            GenerateConsoleCtrlEvent(0, (uint)process.Id);
            bool isUpdatesSection = false;
            process.OutputDataReceived += async (sender, e) =>
            {
                if (e.Data == null) return;
                await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    // Parse the output of the sdkmanager command
                    if (commandMode == CommandMode.Updates)
                    {
                        if (e.Data.StartsWith("Available Updates:"))
                        {
                            isUpdatesSection = true;
                        }
                        else if (isUpdatesSection && e.Data.Contains('|'))
                        {
                            _commandOutput.Add(e.Data);
                        }
                    }
                    else
                    {
                        if (!(e.Data.Contains('|') && e.Data.Contains(';'))) return;
                        _commandOutput.Add(e.Data);
                    }
                }));
            };
            process.BeginOutputReadLine();
            try
            {
                await process.WaitForExitAsync();
            }
            catch (TaskCanceledException)
            {
                GenerateConsoleCtrlEvent(0, (uint)process.Id);
                process.Kill(true);
            }
            catch (OperationCanceledException)
            {
                GenerateConsoleCtrlEvent(0, (uint)process.Id);
                process.Kill(true);
            }
        }
        internal async Task GenerateAndroidPlatformPackagesAsync(bool inSyncMode = false)
        {
            if (!inSyncMode)
            {
                if (File.Exists(_configurationFilePath) && Instance.AndroidPlatformContent == null)
                {
                    UpdateInstance(Serializer.FromFile<SDKManager>(_configurationFilePath));
                    return;
                }
                else if (Instance.AndroidPlatformContent?.Count > 0) return;
            }
            if (DataSource == DataSourceType.Local)
            {
                await GenerateAndroidPackagesFromLocalAsync();
                return;
            }
            _versionMapping ??= GetSDKVersionMappingFromWeb();
            _packages ??= await GetAndroidPackagesFromWebAsync();
            var menuItems = new List<ExpanderListMenuItem>();
            foreach (var sdk in _versionMapping)
            {
                var apiLevel = sdk.Key.Split()[2].Trim(',');
                var package = _packages.Find(x => x.APILevel == apiLevel);
                if (package == null) continue;
                var relevantPackages = _packages.FindAll(x => x.APILevel == apiLevel);
                var listMenuItems = new List<ExpanderListMenuItem>();
                foreach (var item in relevantPackages)
                {
                    var subMenuItem = new ExpanderListMenuItem
                    {
                        Name = item.DisplayName,
                        APILevel = apiLevel,
                        Revision = (int?)item.Revision,
                        Status = false ? "Installed" : "Not Installed",
                        IsChecked = false,
                        IsEnabled = true,
                        Version = item.Version
                    };
                    listMenuItems.Add(subMenuItem);
                }
                var menuItem = new ExpanderListMenuItem
                {
                    Name = "Android " + sdk.Value[1] + " (" + sdk.Value[0] + ") ",
                    APILevel = apiLevel,
                    Revision = (int?)package.Revision,
                    URL = package.DownloadUrl,
                    Status = false ? "Installed" : "Not Installed",
                    IsChecked = false,
                    Version = package.Version,
                    IsEnabled = true,
                    ListMenuItems = listMenuItems
                };
                menuItems.Add(menuItem);
            }
            Instance.AndroidPlatformContent = menuItems;
            Instance.Save();
        }
        internal async Task GenerateAndroidToolsPackagesAsync(bool inSyncMode = false)
        {
            if (!inSyncMode)
            {
                if (File.Exists(_configurationFilePath) && Instance.AndroidToolsContent == null)
                {
                    UpdateInstance(Serializer.FromFile<SDKManager>(_configurationFilePath));
                    return;
                }
                else if (Instance.AndroidToolsContent?.Count > 0) return;
            }
            if (DataSource == DataSourceType.Local)
            {
                if (Instance.AndroidPlatformContent == null || Instance.AndroidPlatformContent.Count <= 0)
                    await GenerateAndroidPackagesFromLocalAsync();
                return;
            }
            _packages ??= await GetAndroidPackagesFromWebAsync();
            var relevantPackages = _packages.FindAll(x => x.Path?.Split(';')[0] != "platforms" && x.Path?.Split(';')[0] != "sources");
            var menuItems = new List<ExpanderListMenuItem>();
            List<string> packagesAlreadyConsidered = new();
            foreach (var package in relevantPackages)
            {
                string? name = package.DisplayName;
                var version = package.Version;
                List<ExpanderListMenuItem>? listMenuItems = null;
                if (int.TryParse(name?[^1].ToString(), out _))
                {
                    name = name.Replace(" " + name.Split().LastOrDefault(), "");
                    listMenuItems = new List<ExpanderListMenuItem>();
                    var suitables = relevantPackages.FindAll(x => x.DisplayName != null && x.DisplayName.StartsWith(name));
                    foreach (var suitable in suitables)
                    {
                        var listMenuItem = new ExpanderListMenuItem
                        {
                            Name = suitable.DisplayName,
                            IsChecked = false,
                            Status = false ? "Installed" : "Not Installed",
                            Version = suitable.Version,
                            IsEnabled = true
                        };
                        listMenuItems.Add(listMenuItem);
                    }
                }
                if (name == null) continue;
                if (packagesAlreadyConsidered.Contains(name)) continue;
                packagesAlreadyConsidered.Add(name);
                var menuItem = new ExpanderListMenuItem
                {
                    Name = name,
                    IsChecked = false,
                    Status = false ? "Installed" : "Not Installed",
                    Version = version,
                    IsEnabled = true,
                    ListMenuItems = listMenuItems
                };
                menuItems.Add(menuItem);
            }
            Instance.AndroidToolsContent = menuItems;
            Instance.Save();
        }
        internal void GenerateAndroidUpadateSitesContent()
        {
            var json = File.ReadAllText(Path.Combine(MainWindow.PrimalPath, "PrimalEditor/Resources/SDKManager/AndroidUpdateSites.json"));
            var content = JsonConvert.DeserializeObject<List<ExpanderListMenuItem>?>(json);
            Instance.AndroidUpdateSitesContent = content;
            Instance.Save();
        }
        private async Task<List<AndroidPackage>> GetAndroidPackagesFromWebAsync()
        {
            try
            {
                var url = "https://dl.google.com/android/repository/repository2-1.xml";
                using var httpClient = new HttpClient();
                var xml = await httpClient.GetStringAsync(url);
                //var path = @"C:\Users\indus\Dropbox\PC\Downloads\repository2-1.xml";
                //var xml = File.ReadAllText(path);
                //var xdoc = XElement.Parse(xml);
                var xdoc = XElement.Load(url);
                // Query the XML data to extract the package elements
                var elements = xdoc.Descendants("remotePackage");
                // Create a list of AndroidPackage objects from the XML data
                var packages = elements.Select(element => new AndroidPackage
                {
                    Path = (string?)element.Attribute("path"),
                    DisplayName = (string?)element.Element("display-name"),
                    Version = (string?)element?.Element("revision")?.Element("major") + "." + (string?)element?.Element("revision")?.Element("minor") + "." + (string?)element?.Element("revision")?.Element("micro") + "." + (string?)element?.Element("revision")?.Element("preview"),
                    APILevel = (string?)(element?.Element("type-details")?.Element("api-level")),
                    Revision = (int?)element?.Element("revision")?.Element("major"),
                    DownloadUrl = "https://dl.google.com/android/repository/" + (string?)element?.Element("archives")?.Element("archive")?.Element("complete")?.Element("url"),
                }).ToList();
                return packages;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                Logger.Log(MessageType.Error, "Failed to get android packages from the web");
                return new List<AndroidPackage>();
            }
        }
        private async Task GenerateAndroidPackagesFromLocalAsync()
        {
            var packages = new List<AndroidPackage>();
            await GenerateAndParseOutputFromCommandLineForAndroidAsync(CommandMode.Vanilla);
            var allPackages = _commandOutput.ToList();
            _commandOutput.Clear();
            await GenerateAndParseOutputFromCommandLineForAndroidAsync(CommandMode.Installed);
            var installedPackages = _commandOutput.ToList();
            _commandOutput.Clear();
            await GenerateAndParseOutputFromCommandLineForAndroidAsync(CommandMode.Updates);
            var updates = _commandOutput.ToList();
            _commandOutput.Clear();
            var outputs = GroupByCategory(allPackages);
            var platforms = new List<ExpanderListMenuItem>();
            var tools = new List<ExpanderListMenuItem>();
            foreach (var item in outputs)
            {
                switch (item.Key)
                {
                    case "Platforms":
                        foreach (var value in item.Value)
                        {
                            var relevant = value.FirstOrDefault().Key.Split('|').Select(x => x.Trim());
                            var packageName = relevant.ElementAtOrDefault(2)?.Trim();
                            var potentialPackageRevision = relevant.ElementAtOrDefault(1)?.Trim();
                            if (potentialPackageRevision == null) continue;
                            var packageRevision = int.Parse(potentialPackageRevision);
                            var packagePath = relevant.FirstOrDefault();
                            var packageApiLevel = packagePath?.Split(';').Where(x => x.Split('-').Contains("android")).FirstOrDefault()?.Split('-').Where(x => x != "android").FirstOrDefault()?.Trim();
                            var subMenuItems = new List<ExpanderListMenuItem>();
                            var values = value.Values.FirstOrDefault()?.Where(x => x.Split('|').Count() <= 3);
                            if (values == null) continue;
                            foreach (var child in values)
                            {
                                var name = child.Split('|').LastOrDefault()?.Trim();
                                var potentialRevision = child.Split('|').ElementAtOrDefault(1)?.Trim();
                                if (potentialRevision == null) continue;
                                var revision = int.Parse(potentialRevision);
                                var path = child.Split('|').FirstOrDefault();
                                var apiLevel = path?.Split(';').Where(x => x.Split('-').Contains("android")).FirstOrDefault()?.Split('-').Where(x => x != "android").FirstOrDefault()?.Trim();
                                if (path == null) continue;
                                bool isChecked = installedPackages.Any(p => p.Contains(path));
                                bool? isUpdateAvailable = updates.Any(p => p.Contains(path)) ? (bool?)null : isChecked;
                                string status = isUpdateAvailable == null ? "Update Available" : isChecked ? "Installed" : "Not Installed";
                                var submenuItem = new ExpanderListMenuItem { Name = name, Revision = revision, APILevel = apiLevel, IsEnabled = true, IsChecked = isUpdateAvailable, Status = status, URL = path };
                                subMenuItems.Add(submenuItem);
                            }
                            if (packagePath == null) continue;
                            bool isPackageChecked = installedPackages.Any(p => p.Contains(packagePath));
                            bool? isPackageUpdateAvailable = updates.Any(p => p.Contains(packagePath)) ? (bool?)null : isPackageChecked;
                            string packageStatus = isPackageUpdateAvailable == null ? "Update Available" : isPackageChecked ? "Installed" : "Not Installed";
                            var menuItem = new ExpanderListMenuItem
                            {
                                Name = packageName,
                                Revision = packageRevision,
                                APILevel = packageApiLevel,
                                IsEnabled = true,
                                IsChecked = isPackageUpdateAvailable,
                                Status = packageStatus,
                                URL = packagePath,
                                ListMenuItems = subMenuItems
                            };
                            platforms.Add(menuItem);
                        }
                        break;
                    case "Tools":
                        foreach (var value in item.Value)
                        {
                            var relevant = value.FirstOrDefault().Key.Split('|').Select(x => x.Trim());
                            var packageName = relevant.ElementAtOrDefault(2)?.Trim();
                            if (packageName == null) continue;
                            packageName = Regex.Replace(packageName, @"\d+(\.\d+)*", "");
                            var packageVersion = relevant.ElementAtOrDefault(1)?.Trim();
                            var packagePath = relevant.FirstOrDefault()?.Trim();
                            var subMenuItems = new List<ExpanderListMenuItem>();
                            var values = value.Values.FirstOrDefault()?.Where(x => x.Split('|').Count() <= 3);
                            if (values == null) continue;
                            foreach (var child in values)
                            {
                                var name = child.Split('|').LastOrDefault()?.Trim();
                                var version = child.Split('|').ElementAtOrDefault(1)?.Trim();
                                var path = child.Split('|').FirstOrDefault();
                                if (path == null) continue;
                                bool isChecked = installedPackages.Any(p => p.Contains(path));
                                bool? isUpdateAvailable = updates.Any(p => p.Contains(path)) ? (bool?)null : isChecked;
                                string status = isUpdateAvailable == null ? "Update Available" : isChecked ? "Installed" : "Not Installed";
                                var subMenuItem = new ExpanderListMenuItem { Name = name, Version = version, IsChecked = isUpdateAvailable, IsEnabled = true, Status = status, URL = path };
                                subMenuItems.Add(subMenuItem);
                            }
                            if (packagePath == null) continue;
                            bool isPackageChecked = installedPackages.Any(p => p.Contains(packagePath));
                            bool? isPackageUpdateAvailable = updates.Any(p => p.Contains(packagePath)) ? (bool?)null : isPackageChecked;
                            string packageStatus = isPackageUpdateAvailable == null ? "Update Available" : isPackageChecked ? "Installed" : "Not Installed";
                            var menuItem = new ExpanderListMenuItem { Name = packageName, Version = packageVersion, IsChecked = isPackageChecked, IsEnabled = true, Status = "Not Installed", URL = packagePath, ListMenuItems = subMenuItems };
                            tools.Add(menuItem);
                        }
                        break;
                    default: break;
                }
            }
            Instance.AndroidPlatformContent = platforms;
            Instance.AndroidToolsContent = tools;
        }
        private static Dictionary<string, List<Dictionary<string, List<string>>>> GroupByCategory(List<string>? input)
        {
            var platforms = new Dictionary<string, List<string>>();
            var tools = new Dictionary<string, List<string>>();
            foreach (var (item, parts) in from item in input let parts = item.Split('|') select (item, parts))
            {
                if (parts.Length < 3) continue;
                var pathParts = parts[0].Split(';');
                if (pathParts.Length < 2) continue;
                var category = pathParts[0].Trim();
                var subcategory = pathParts[1].Trim();
                if (category == "platforms" || category == "system-images" || category == "sources")
                {
                    if (!platforms.ContainsKey(subcategory)) platforms.Add(subcategory, new List<string>());
                    platforms[subcategory].Add(item);
                }
                else
                {
                    if (!tools.ContainsKey(category)) tools.Add(category, new List<string>());
                    tools[category].Add(item);
                }
            }
            var result = new Dictionary<string, List<Dictionary<string, List<string>>>>
                {
                    { "Platforms", platforms.Select(x => new Dictionary<string, List<string>> { { x.Value.FirstOrDefault(), x.Value } }).ToList() },
                    { "Tools", tools.Select(x => new Dictionary<string, List<string>> { { x.Value.FirstOrDefault(), x.Value } }).ToList() }
                };
            return result;
        }
        public async Task SyncDataAsync()
        {
            await GenerateAndroidPlatformPackagesAsync(true);
            await GenerateAndroidToolsPackagesAsync(true);
            Instance.LastSyncTime = DateTime.Now;
            Instance.Save();
        }
        internal void UpdateInstance()
        {
            if (File.Exists(_configurationFilePath) && Instance.IsAutoSyncEnabled == null) UpdateInstance(Serializer.FromFile<SDKManager>(_configurationFilePath));
        }
    }
    /// <summary>
    /// Useful for inter conversion between the DataSourceType enum and a double.
    /// </summary>
    /// <remarks>
    /// This was implemented for the special case when the data source type can be toggled between web and local in andorid sdk manager.
    /// </remarks>
    public class DataSourceTypeToSliderValueConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataSourceType dataSourceType)
            {
                return dataSourceType == DataSourceType.Local ? 1 : 0;
            }
            return 1;
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return doubleValue == 1 ? DataSourceType.Local : DataSourceType.Web;
            }
            return DataSourceType.Local;
        }
    }
}
