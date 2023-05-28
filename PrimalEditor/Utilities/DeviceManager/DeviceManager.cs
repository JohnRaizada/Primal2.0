using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Data;
using System.Windows.Media;

namespace PrimalEditor.Utilities.DeviceManager
{
    enum ConstraintType
    {
        Width,
        Height
    }
    /// <summary>
    /// An enumeration of data storage units.
    /// </summary>
    public enum DataStorageUnits
    {
        /// <summary>
        /// A byte.
        /// </summary>
        B,
        /// <summary>
        /// A kilobyte.
        /// </summary>
        KB,
        /// <summary>
        /// A megabyte.
        /// </summary>
        MB,
        /// <summary>
        /// A gigabyte.
        /// </summary>
        GB,
        /// <summary>
        /// A terabyte.
        /// </summary>
        TB
    }
    enum DeviceCategories
    {
        Phone,
        Tablet,
        WearOS,
        Desktop,
        TV,
        Automotive
    }
    /// <summary>
    /// An enumeration of device types.
    /// </summary>
    public enum DeviceTypes
    {
        /// <summary>
        /// A phone or tablet.
        /// </summary>
        PhoneTablet,
        /// <summary>
        /// A wearable device running Wear OS.
        /// </summary>
        WearOS,
        /// <summary>
        /// A desktop computer.
        /// </summary>
        Desktop,
        /// <summary>
        /// A television running Android TV.
        /// </summary>
        AndroidTV,
        /// <summary>
        /// A television running Google TV.
        /// </summary>
        GoogleTV,
        /// <summary>
        /// A Chromebook.
        /// </summary>
        ChromeOSDevice,
        /// <summary>
        /// An automotive device running Android Automotive.
        /// </summary>
        AndroidAutomotive
    }
    /// <summary>
    /// An enumeration of navigation styles.
    /// </summary>
    public enum NavigationStyles
    {
        /// <summary>
        /// No navigation.
        /// </summary>
        None,
        /// <summary>
        /// No navigation.
        /// </summary>
        Dpad,
        /// <summary>
        /// No navigation.
        /// </summary>
        Trackball,
        /// <summary>
        /// Navigation using a wheel.
        /// </summary>
        Wheel
    }
    /// <summary>
    /// An enumeration of device sizes.
    /// </summary>
    public enum DeviceSizes
    {
        /// <summary>
        /// A small device.
        /// </summary>
        small,
        /// <summary>
        /// A normal-sized device.
        /// </summary>
        normal,
        /// <summary>
        /// A large device.
        /// </summary>
        large,
        /// <summary>
        /// An extra-large device.
        /// </summary>
        xlarge
    }
    class Device : ViewModelBase
    {
        private string _name = string.Empty;
        private string _api = string.Empty;
        private string _size = string.Empty;
        private Geometry? _path = Geometry.Parse(IconTypes.Default);
        private string? _version = string.Empty;
        private FontFamily? _fontFamily = Fonts.SystemFontFamilies.FirstOrDefault();
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public string Api
        {
            get => _api;
            set
            {
                if (_api == value) return;
                _api = value;
                OnPropertyChanged(nameof(Api));
            }
        }
        public string Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
        public Geometry? Path
        {
            get => _path;
            set
            {
                if (_path == value) return;
                _path = value;
                OnPropertyChanged(nameof(Path));
            }
        }
        public string? Version
        {
            get => _version;
            set
            {
                if (_version == value) return;
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public FontFamily? FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily == value) return;
                _fontFamily = value;
                OnPropertyChanged(nameof(FontFamily));
            }
        }
    }
    /// <summary>
    /// A class that represents a device definition.
    /// </summary>
    public class DeviceDefinition : ViewModelBase
    {
        private string _name = string.Empty;
        private DeviceTypes _type = DeviceTypes.PhoneTablet;
        private bool _isRound = false;
        private uint _memory = 0;
        private DataStorageUnits _dataStorageUnit = DataStorageUnits.B;
        private bool _hasHardwareButtons = false;
        private bool _hasKeyboardHardware = false;
        private NavigationStyles _navigationStyle = NavigationStyles.None;
        private bool _supportsPortraitDeviceState = false;
        private bool _supportsLandscapeDeviceState = false;
        private bool _hasBackFacingCamera = false;
        private bool _hasFrontFacingCamera = false;
        private bool _hasAccelerometer = false;
        private bool _hasGyroscope = false;
        private bool _hasGPS = false;
        private bool _hasProximitySensor = false;
        private bool _supportsPlayStore = false;
        private string _skin = string.Empty;
        private DeviceSizes _size = DeviceSizes.small;
        private int _height = 0;
        private int _width = 0;
        private double _iconHeight = 0;
        private double _iconWidth = 0;
        private float _diagonalLength = 0;
        private string _density = string.Empty;
        private string? _ratio = string.Empty;
        /// <summary>
        /// The name of the device.
        /// </summary>
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        /// <summary>
        /// The type of the device.
        /// </summary>
        public DeviceTypes Type
        {
            get => _type;
            set
            {
                if (_type == value) return;
                _type = value;
                OnPropertyChanged(nameof(Type));
            }
        }
        /// <summary>
        /// Whether the device is round.
        /// </summary>
        public bool IsRound
        {
            get => _isRound;
            set
            {
                if (_isRound == value) return;
                _isRound = value;
                OnPropertyChanged(nameof(IsRound));
            }
        }
        /// <summary>
        /// The amount of memory on the device.
        /// </summary>
        public uint Memory
        {
            get => _memory;
            set
            {
                if (_memory == value) return;
                _memory = value;
                OnPropertyChanged(nameof(Memory));
            }
        }
        /// <summary>
        /// The unit of measurement for the memory on the device.
        /// </summary>
        public DataStorageUnits DataStorageUnit
        {
            get => _dataStorageUnit;
            set
            {
                if (_dataStorageUnit == value) return;
                _dataStorageUnit = value;
                OnPropertyChanged(nameof(DataStorageUnit));
            }
        }
        /// <summary>
        /// Whether the device has hardware buttons.
        /// </summary>
        public bool HasHardwareButtons
        {
            get => _hasHardwareButtons;
            set
            {
                if (_hasHardwareButtons == value) return;
                _hasHardwareButtons = value;
                OnPropertyChanged(nameof(HasHardwareButtons));
            }
        }
        /// <summary>
        /// Whether the device has a keyboard.
        /// </summary>
        public bool HasKeyboardHardware
        {
            get => _hasKeyboardHardware;
            set
            {
                if (_hasKeyboardHardware == value) return;
                _hasKeyboardHardware = value;
                OnPropertyChanged(nameof(HasKeyboardHardware));
            }
        }
        /// <summary>
        /// The navigation style for the device.
        /// </summary>
        public NavigationStyles NavigationStyle
        {
            get => _navigationStyle;
            set
            {
                if (_navigationStyle == value) return;
                _navigationStyle = value;
                OnPropertyChanged(nameof(NavigationStyle));
            }
        }
        /// <summary>
        /// Whether the device supports portrait mode.
        /// </summary>
        public bool SupportsPortraitDeviceState
        {
            get => _supportsPortraitDeviceState;
            set
            {
                if (_supportsPortraitDeviceState == value) return;
                _supportsPortraitDeviceState = value;
                OnPropertyChanged(nameof(SupportsPortraitDeviceState));
            }
        }
        /// <summary>
        /// Whether the device supports landscape mode.
        /// </summary>
        public bool SupportsLandscapeDeviceState
        {
            get => _supportsLandscapeDeviceState;
            set
            {
                if (_supportsLandscapeDeviceState == value) return;
                _supportsLandscapeDeviceState = value;
                OnPropertyChanged(nameof(SupportsLandscapeDeviceState));
            }
        }
        /// <summary>
        /// Whether the device has a back-facing camera.
        /// </summary>
        public bool HasBackFacingCamera
        {
            get => _hasBackFacingCamera;
            set
            {
                if (_hasBackFacingCamera == value) return;
                _hasBackFacingCamera = value;
                OnPropertyChanged(nameof(HasBackFacingCamera));
            }
        }
        /// <summary>
        /// Whether the device has a front-facing camera.
        /// </summary>
        public bool HasFrontFacingCamera
        {
            get => _hasFrontFacingCamera;
            set
            {
                if (value == _hasFrontFacingCamera) return;
                _hasFrontFacingCamera = value;
                OnPropertyChanged(nameof(HasFrontFacingCamera));
            }
        }
        /// <summary>
        /// Whether the device has an accelerometer.
        /// </summary>
        public bool HasAccelerometer
        {
            get => _hasAccelerometer;
            set
            {
                if (_hasAccelerometer == value) return;
                _hasAccelerometer = value;
                OnPropertyChanged(nameof(HasAccelerometer));
            }
        }
        /// <summary>
        /// Whether the device has a gyroscope.
        /// </summary>
        public bool HasGyroscope
        {
            get => _hasGyroscope;
            set
            {
                if (_hasGyroscope == value) return;
                _hasGyroscope = value;
                OnPropertyChanged(nameof(HasGyroscope));
            }
        }
        /// <summary>
        /// Whether the device has a GPS.
        /// </summary>
        public bool HasGPS
        {
            get => _hasGPS;
            set
            {
                if (_hasGPS == value) return;
                _hasGPS = value;
                OnPropertyChanged(nameof(HasGPS));
            }
        }
        /// <summary>
        /// Whether the device has a proximity sensor.
        /// </summary>
        public bool HasProximitySensor
        {
            get => _hasProximitySensor;
            set
            {
                if (_hasProximitySensor == value) return;
                _hasProximitySensor = value;
                OnPropertyChanged(nameof(HasProximitySensor));
            }
        }
        /// <summary>
        /// Whether the device supports the Play Store.
        /// </summary>
        public bool SupportsPlayStore
        {
            get => _supportsPlayStore;
            set
            {
                if (_supportsPlayStore == value) return;
                _supportsPlayStore = value;
                OnPropertyChanged(nameof(SupportsPlayStore));
            }
        }
        /// <summary>
        /// The skin of the device
        /// </summary>
        public string Skin
        {
            get => _skin;
            set
            {
                if (_skin == value) return;
                _skin = value;
                OnPropertyChanged(nameof(Skin));
            }
        }
        /// <summary>
        /// The size of the device.
        /// </summary>
        public DeviceSizes Size
        {
            get => _size;
            set
            {
                if (_size == value) return;
                _size = value;
                OnPropertyChanged(nameof(Size));
            }
        }
        /// <summary>
        /// The height of the device.
        /// </summary>
        public int Height
        {
            get => _height;
            set
            {
                if (_height == value) return;
                _height = value;
                OnPropertyChanged(nameof(Height));
            }
        }
        /// <summary>
        /// The width of the device.
        /// </summary>
        public int Width
        {
            get => _width;
            set
            {
                if (_width == value) return;
                _width = value;
                OnPropertyChanged(nameof(Width));
            }
        }
        /// <summary>
        /// The width of the device.
        /// </summary>
        public double IconHeight
        {
            get => _iconHeight;
            set
            {
                if (_iconHeight == value) return;
                _iconHeight = value;
                OnPropertyChanged(nameof(IconHeight));
            }
        }
        /// <summary>
        /// The width of the device.
        /// </summary>
        public double IconWidth
        {
            get => _iconWidth;
            set
            {
                if (_iconWidth == value) return;
                _iconWidth = value;
                OnPropertyChanged(nameof(IconWidth));
            }
        }
        /// <summary>
        /// The diagonal length of the device.
        /// </summary>
        public float DiagonalLength
        {
            get => _diagonalLength;
            set
            {
                if (_diagonalLength == value) return;
                _diagonalLength = value;
                OnPropertyChanged(nameof(DiagonalLength));
            }
        }
        /// <summary>
        /// The density of the device's screen.
        /// </summary>
        public string Density
        {
            get => _density;
            set
            {
                if (_density == value) return;
                _density = value;
                OnPropertyChanged(nameof(Density));
            }
        }
        /// <summary>
        /// The ratio of the device's screen.
        /// </summary>
        public string? Ratio
        {
            get => _ratio;
            set
            {
                if (_ratio == value) return;
                _ratio = value;
                OnPropertyChanged(nameof(Ratio));
            }
        }
    }
    class AndroidDeviceIcon : ViewModelBase
    {
        private string _version = string.Empty;
        private Geometry _icon = Geometry.Parse(IconTypes.Default);
        private FontFamily? _fontFamily = Fonts.SystemFontFamilies.FirstOrDefault();
        public string Version
        {
            get => _version;
            set
            {
                if (_version == value) return;
                _version = value;
                OnPropertyChanged(nameof(Version));
            }
        }
        public Geometry Icon
        {
            get => _icon;
            set
            {
                if (_icon == value) return;
                _icon = value;
                OnPropertyChanged(nameof(Icon));
            }
        }
        public FontFamily? FontFamily
        {
            get => _fontFamily;
            set
            {
                if (_fontFamily == value) return;
                _fontFamily = value;
                OnPropertyChanged(nameof(FontFamily));
            }
        }
    }
    class GeometryConverter : JsonConverter<Geometry>
    {
        public override Geometry Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return Geometry.Parse(value);
        }
        public override void Write(Utf8JsonWriter writer, Geometry value, JsonSerializerOptions options) => writer.WriteStringValue(value.ToString());
    }
    class FontFamilyConverter : JsonConverter<FontFamily>
    {
        public override FontFamily Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return new FontFamily(value);
        }
        public override void Write(Utf8JsonWriter writer, FontFamily value, JsonSerializerOptions options) => writer.WriteStringValue(value.Source);
    }
    class DeviceManager : ViewModelBase
    {
        private static DeviceManager? _instance;
        private static readonly object _lock = new();
        internal static DeviceManager Instance
        {
            get
            {
                if (_instance == null) lock (_lock) _instance ??= new DeviceManager();
                return _instance;
            }
        }
        private void UpdateInstance(DeviceManager? newInstance)
        {
            lock (_lock) _instance = newInstance;
        }
        public ObservableCollection<Device>? Devices { get; set; }
        //public ObservableCollection<string> DeviceCategories { get; set; } = new() { "Phone", "Tablet", "Wear OS", "Desktop", "TV", "Automotive" };
        //public ObservableCollection<string> DeviceTypes { get; set; } = new() { "Phone/Tablet", "Wear OS", "Desktop", "Android TV", "Google TV", "Chrome OS Device", "Android Automotive" };
        //public ObservableCollection<string> NavigationStyles { get; set; } = new() { "None", "D-pad", "Trackball", "Wheel" };
        public ObservableCollection<string> DeviceSkins { get; set; } = new() { "None", "D-pad", "Trackball", "Wheel" };
        public ObservableCollection<DeviceDefinition> DeviceDefinitions { get; set; } = new()
        {
            new DeviceDefinition{ Name = "Pixel 6 Pro", Density = "420", Size = ParseSize(8), SupportsPlayStore = true, Height = 1080, Width = 1440, Ratio = ParseRatio(1080, 1440, "100"), IconHeight = ParseIconDimensionalConstraint(1080, 1440, ConstraintType.Height), IconWidth = ParseIconDimensionalConstraint(1080, 1440, ConstraintType.Width), DiagonalLength = 7 },
            new DeviceDefinition{ Name = "Pixel 5 Pro", Density = "xxh", Size = ParseSize(4), SupportsPlayStore = false, Height = 108, Width = 1440, Ratio = ParseRatio(108, 1440, "100"), IconHeight = ParseIconDimensionalConstraint(108, 1440, ConstraintType.Height), IconWidth = ParseIconDimensionalConstraint(108, 1340, ConstraintType.Width), DiagonalLength = 9 },
            new DeviceDefinition{ Name = "Pixel 3 Pro", Density = "420", Size = ParseSize(6), SupportsPlayStore = true, Height = 1080, Width = 1440, Ratio = ParseRatio(1080, 1440, "100"), IconHeight = ParseIconDimensionalConstraint(1080, 1440, ConstraintType.Height), IconWidth = ParseIconDimensionalConstraint(880, 1240, ConstraintType.Width), DiagonalLength = 8 },
        };
        private Device? GenerateDevice(string name, string api, string size, string version)
        {
            var path = MainWindow.PrimalPath;
            if (path == null) return null;
            var json = File.ReadAllText(Path.Combine(path, "PrimalEditor/Resources/DeviceManager/AndroidDeviceIconList.json"));
            JsonSerializerOptions options = new()
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                Converters = { new GeometryConverter(), new FontFamilyConverter() },
                WriteIndented = true,
            };
            var androidDeviceIcons = JsonSerializer.Deserialize<ObservableCollection<AndroidDeviceIcon>>(json, options);
            var relevantItem = androidDeviceIcons?.Where(x => x.Version == version).FirstOrDefault();
            var device = new Device { Name = name, Api = api, Size = size, Version = relevantItem?.Version, FontFamily = relevantItem?.FontFamily, Path = relevantItem?.Icon };
            return device;
        }
        internal static string? ParseRatio(int height, int width, string? density = null)
        {
            if (density is null) return density;
            if (density.Split().Length > 1 || density.Contains(' ')) return null;
            var success = int.TryParse(density, out int evaluatedDensity);
            if (!success) switch (density)
                {
                    case "l":
                        evaluatedDensity = 120;
                        break;
                    case "m":
                        evaluatedDensity = 160;
                        break;
                    case "tv":
                        evaluatedDensity = 213;
                        break;
                    case "h":
                        evaluatedDensity = 240;
                        break;
                    case "xh":
                        evaluatedDensity = 320;
                        break;
                    case "xxh":
                        evaluatedDensity = 480;
                        break;
                    case "xxxh":
                        evaluatedDensity = 640;
                        break;
                    default: break;
                }
            if (evaluatedDensity == 0) return null;
            float evaluatedWidth = width / (float)evaluatedDensity;
            float evaluatedHeight = height / (float)evaluatedDensity;
            if (evaluatedWidth == 0 || evaluatedHeight == 0) return null;
            string? evaluatedRatio;
            if (evaluatedWidth > evaluatedHeight) evaluatedRatio = (evaluatedWidth / evaluatedHeight > 5 / 3) ? "long" : "notlong";
            else evaluatedRatio = (evaluatedHeight / evaluatedWidth > 5 / 3) ? "long" : "notlong";
            return evaluatedRatio;
        }
        internal static double ParseIconDimensionalConstraint(double equivalentHeight, double equivalentWidth, ConstraintType constraintType)
        {
            double multiplicationFactor;
            if (equivalentHeight > equivalentWidth) multiplicationFactor = 200 / equivalentHeight;
            else multiplicationFactor = 200 / equivalentWidth;
            return constraintType switch
            {
                ConstraintType.Width => equivalentWidth * multiplicationFactor,
                ConstraintType.Height => equivalentHeight * multiplicationFactor,
                _ => 0,
            };
        }
        internal static DeviceSizes ParseSize(float diagonalLength)
        {
            if (diagonalLength < 3.554) return DeviceSizes.small;
            else if (diagonalLength < 5) return DeviceSizes.normal;
            else if (diagonalLength < 7.5) return DeviceSizes.large;
            return DeviceSizes.xlarge;
        }
        internal static string ParseDensityBucket(float diagonalLengthInInches, int pixelWidth, int pixelHeight)
        {
            double numberOfPixelsThatFitTheDiagonal = Math.Sqrt((pixelWidth * pixelWidth) + (pixelHeight * pixelHeight));
            var ppi = numberOfPixelsThatFitTheDiagonal / diagonalLengthInInches;
            if (ppi == 1) return "no";
            else if (ppi == 213) return "tv";
            else if (ppi <= 120) return "l";
            else if (ppi <= 160) return "m";
            else if (ppi <= 240) return "h";
            else if (ppi <= 320) return "xh";
            else if (ppi <= 480) return "xxh";
            else if (ppi <= 640) return "xxxh";
            return string.Empty;
        }
    }
    class DoubleValueConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue) return doubleValue * 2;
            return value;
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class HalfValueConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue) return doubleValue / 2;
            return value;
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}