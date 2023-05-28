namespace PrimalEditor.Utilities.DeviceManager
{
    class NewHardwareProfile : ViewModelBase
    {
        private static NewHardwareProfile? _instance;
        private static readonly object _lock = new();
        internal static NewHardwareProfile Instance
        {
            get
            {
                if (_instance == null) lock (_lock) _instance ??= new NewHardwareProfile();
                return _instance;
            }
        }
        private void UpdateInstance(NewHardwareProfile? newInstance)
        {
            lock (_lock) _instance = newInstance;
        }
        private string _name = string.Empty;
        private DeviceTypes _type = DeviceTypes.PhoneTablet;
        private bool _isRound = false;
        private uint _memory = 0;
        private DataStorageUnits _dataStorageUnit = DataStorageUnits.B;
        private bool _hasHardwareButtons = false;
        private bool _hasHardwareKeyboard = false;
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
        private DeviceSizes _size = DeviceSizes.small;
        private int _height = 0;
        private int _width = 0;
        private double _iconHeight = 0;
        private double _iconWidth = 0;
        private float _diagonalLength = 0;
        private string _density = string.Empty;
        private string? _ratio = string.Empty;
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
        public bool HasHardwareKeyboard
        {
            get => _hasHardwareKeyboard;
            set
            {
                if (_hasHardwareKeyboard == value) return;
                _hasHardwareKeyboard = value;
                OnPropertyChanged(nameof(HasHardwareKeyboard));
            }
        }
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
        internal void InitializeParameters(DeviceDefinition deviceDefinition)
        {
            foreach (var property in typeof(DeviceDefinition).GetProperties())
            {
                if (!property.CanRead) continue;
                var value = property.GetValue(deviceDefinition);
                var currentProperty = this.GetType().GetProperty(property.Name);
                if (currentProperty != null && currentProperty.CanWrite) currentProperty.SetValue(this, value);
            }
        }
    }
}
