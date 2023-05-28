using System.Diagnostics;
using System.Windows;

namespace PrimalEditor.Utilities.DeviceManager
{
    /// <summary>
    /// Interaction logic for NewHardwareProfileView.xaml
    /// </summary>
    public partial class NewHardwareProfileView : Window
    {
        public NewHardwareProfileView()
        {
            InitializeComponent();
            DataContext = NewHardwareProfile.Instance;
        }
        private void OnCreateHardwareProfile_CustomSkinLink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e) => _ = Process.Start(new ProcessStartInfo("cmd", $"/c start https://developer.android.com/studio/run/managing-avds#skins") { CreateNoWindow = true });
        private void OnNewHardwareProfile_NameTextBoxBlock_ValueChanged(object sender, Controls.ValueChangedEventArgs e)
        {
            NewHardwareProfile.Instance.Name = e.Value;
        }
        private void OnNewHardwareProfile_DiagonalTextBoxBlock_ValueChanged(object sender, Controls.ValueChangedEventArgs e)
        {
            var result = float.TryParse(e.Value, out var value);
            if (!result) return;
            NewHardwareProfile.Instance.DiagonalLength = value;
            NewHardwareProfile.Instance.Size = DeviceManager.ParseSize(value);
            NewHardwareProfile.Instance.Density = DeviceManager.ParseDensityBucket(NewHardwareProfile.Instance.DiagonalLength, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Height);
            NewHardwareProfile.Instance.Ratio = DeviceManager.ParseRatio(NewHardwareProfile.Instance.Height, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Density);
        }
        private void OnNewHardwareProfile_WidthTextBoxBlock_ValueChanged(object sender, Controls.ValueChangedEventArgs e)
        {
            var result = int.TryParse(e.Value, out var value);
            if (!result) return;
            NewHardwareProfile.Instance.Width = value;
            NewHardwareProfile.Instance.IconWidth = DeviceManager.ParseIconDimensionalConstraint(NewHardwareProfile.Instance.Height, NewHardwareProfile.Instance.Width, ConstraintType.Width);
            NewHardwareProfile.Instance.Density = DeviceManager.ParseDensityBucket(NewHardwareProfile.Instance.DiagonalLength, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Height);
            NewHardwareProfile.Instance.Ratio = DeviceManager.ParseRatio(NewHardwareProfile.Instance.Height, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Density);
        }
        private void OnNewHardwareProfile_HeightTextBoxBlock_ValueChanged(object sender, Controls.ValueChangedEventArgs e)
        {
            var result = int.TryParse(e.Value, out var value);
            if (!result) return;
            NewHardwareProfile.Instance.Height = value;
            NewHardwareProfile.Instance.IconHeight = DeviceManager.ParseIconDimensionalConstraint(NewHardwareProfile.Instance.Height, NewHardwareProfile.Instance.Width, ConstraintType.Height);
            NewHardwareProfile.Instance.Density = DeviceManager.ParseDensityBucket(NewHardwareProfile.Instance.DiagonalLength, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Height);
            NewHardwareProfile.Instance.Ratio = DeviceManager.ParseRatio(NewHardwareProfile.Instance.Height, NewHardwareProfile.Instance.Width, NewHardwareProfile.Instance.Density);
        }
    }
}
