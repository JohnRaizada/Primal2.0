using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities.Controls
{
	internal class DeviceSchematic : Control
	{
		public static readonly DependencyProperty DeviceHeightProperty = DependencyProperty.Register(nameof(DeviceHeight), typeof(int), typeof(DeviceSchematic), new PropertyMetadata(0));
		public static readonly DependencyProperty DeviceWidthProperty = DependencyProperty.Register(nameof(DeviceWidth), typeof(int), typeof(DeviceSchematic), new PropertyMetadata(0));
		public static readonly DependencyProperty DeviceDiagonalProperty = DependencyProperty.Register(nameof(DeviceDiagonal), typeof(int), typeof(DeviceSchematic), new PropertyMetadata(0));
		public int DeviceHeight
		{
			get => (int)GetValue(DeviceHeightProperty);
			set => SetValue(DeviceHeightProperty, value);
		}
		public int DeviceWidth
		{
			get => (int)GetValue(DeviceWidthProperty);
			set => SetValue(DeviceWidthProperty, value);
		}
		public int DeviceDiagonal
		{
			get => (int)GetValue(DeviceDiagonalProperty);
			set => SetValue(DeviceDiagonalProperty, value);
		}
	}
}
