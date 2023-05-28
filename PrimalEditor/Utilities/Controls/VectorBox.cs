using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities.Controls
{
    /// <summary>
    /// An enumeration that represents the type of a vector.
    /// </summary>
	public enum VectorType
    {
        /// <summary>
        /// A 2D vector.
        /// </summary>
		Vector2,
        /// <summary>
        /// A 3D vector.
        /// </summary>
		Vector3,
        /// <summary>
        /// A 4D vector.
        /// </summary>
		Vector4
    }
    /// <summary>
    /// A class that represents a <see cref="VectorBox"/>.
    /// </summary>
	class VectorBox : Control
	{
		public static readonly DependencyProperty VectorTypeProperty = DependencyProperty.Register(nameof(VectorType), typeof(VectorType), typeof(VectorBox), new PropertyMetadata(VectorType.Vector3));
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(VectorBox), new PropertyMetadata(Orientation.Horizontal));
		public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(nameof(Multiplier), typeof(double), typeof(VectorBox), new PropertyMetadata(1.0));
		public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(VectorBox), new PropertyMetadata(double.MinValue));
		public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(VectorBox), new PropertyMetadata(double.MaxValue));
		public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(string), typeof(VectorBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(string), typeof(VectorBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty ZProperty = DependencyProperty.Register(nameof(Z), typeof(string), typeof(VectorBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		public static readonly DependencyProperty WProperty = DependencyProperty.Register(nameof(W), typeof(string), typeof(VectorBox), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        /// <summary>
        /// The type of the vector.
        /// </summary>
		public VectorType VectorType
		{
			get => (VectorType)GetValue(VectorTypeProperty);
			set => SetValue(VectorTypeProperty, value);
        }
        /// <summary>
        /// The orientation of the vector box.
        /// </summary>
		public Orientation Orientation
		{
			get => (Orientation)GetValue(OrientationProperty);
			set => SetValue(OrientationProperty, value);
        }
        /// <summary>
        /// The multiplier used to scale the vector.
        /// </summary>
		public double Multiplier
		{
			get => (double)GetValue(MultiplierProperty);
			set => SetValue(MultiplierProperty, value);
        }
        /// <summary>
        /// The minimum value of the vector.
        /// </summary>
		public double Minimum
		{
			get => (double)GetValue(MinimumProperty);
			set => SetValue(MinimumProperty, value);
        }
        /// <summary>
        /// The maximum value of the vector.
        /// </summary>
		public double Maximum
		{
			get => (double)GetValue(MaximumProperty);
			set => SetValue(MaximumProperty, value);
        }
        /// <summary>
        /// The X coordinate of the vector.
        /// </summary>
		public string X
		{
			get => (string)GetValue(XProperty);
			set => SetValue(XProperty, value);
        }
        /// <summary>
        /// The Y coordinate of the vector.
        /// </summary>
		public string Y
		{
			get => (string)GetValue(YProperty);
			set => SetValue(YProperty, value);
        }
        /// <summary>
        /// The Z coordinate of the vector.
        /// </summary>
		public string Z
		{
			get => (string)GetValue(ZProperty);
			set => SetValue(ZProperty, value);
        }
        /// <summary>
        /// The W coordinate of the vector.
        /// </summary>
		public string W
		{
			get => (string)GetValue(WProperty);
			set => SetValue(WProperty, value);
        }
        /// <summary>
        /// Initializes a new instance of the VectorBox class.
        /// </summary>
		static VectorBox() => DefaultStyleKeyProperty.OverrideMetadata(typeof(VectorBox), new FrameworkPropertyMetadata(typeof(VectorBox)));
	}
}
