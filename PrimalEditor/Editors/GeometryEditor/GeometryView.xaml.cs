using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Editors
{
	/// <summary>
	/// Interaction logic for GeometryView.xaml
	/// </summary>
	public partial class GeometryView : UserControl
	{
		private static readonly GeometryView _geometryView = new() { Background = (Brush)Application.Current.FindResource("Editor.Window.GrayBrush4") };
		private Point _clickedPosition;
		private bool _capturedLeft;
		private bool _capturedRight;
        /// <summary>
        /// Sets the mesh that is being displayed.
        /// </summary>
        /// <param name="index">The index of the mesh to display. If -1, all meshes will be displayed.</param>
        public void SetGeometry(int index = -1)
        {
            // Returns if the DataContext is not a MeshRenderer.
            if (DataContext is not MeshRenderer vm) return;
            // Removes the second child of the viewport if it is a ModelVisual3D.
            if (vm.Meshes.Any() && viewport.Children.Count == 2) viewport.Children.RemoveAt(1);
			//The index of the current mesh.
            var meshIndex = 0;
            // A group that will contain all of the models.
            var modelGroup = new Model3DGroup();
            // Iterates over the meshes in the MeshRenderer.
            foreach (var mesh in vm.Meshes)
			{
				// Skip over meshes that we don't want to display
				if (index != -1 && meshIndex != index)
				{
					++meshIndex;
					continue;
				}
                // Creates a MeshGeometry3D from the current mesh.
                var mesh3D = new MeshGeometry3D()
				{
					Positions = mesh.Positions,
					Normals = mesh.Normals,
					TriangleIndices = mesh.Indices,
					TextureCoordinates = mesh.UVs
				};
                // Creates a DiffuseMaterial from the current mesh.
                var diffuse = new DiffuseMaterial(mesh.Diffuse);
                // Creates a SpecularMaterial from the current mesh.
                var specular = new SpecularMaterial(mesh.Specular, 50);
                // A group that will contain the diffuse and specular materials.
                var matGroup = new MaterialGroup();
				matGroup.Children.Add(diffuse);
				matGroup.Children.Add(specular);
                // Creates a GeometryModel3D from the current mesh and materials.
                var model = new GeometryModel3D(mesh3D, matGroup);
                // Adds the model to the model group.
                modelGroup.Children.Add(model);
                // Binds the diffuse material to the current mesh.
                var binding = new Binding(nameof(mesh.Diffuse)) { Source = mesh };
				BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);
                // Breaks the loop if the current mesh is the one specified by the index.
                if (meshIndex == index) break;
			}
            // Creates a ModelVisual3D from the model group.
            var visual = new ModelVisual3D() { Content = modelGroup };
            // Adds the model visual to the viewport.
            viewport.Children.Add(visual);
		}
		private void OnGrid_Moue_LBD(object sender, MouseButtonEventArgs e)
		{
			_clickedPosition = e.GetPosition(this);
			_capturedLeft = true;
			Mouse.Capture(sender as UIElement);
		}
		private void OnGrid_MouseMove(object sender, MouseEventArgs e)
		{
			if (!_capturedLeft && !_capturedRight) return;
			var pos = e.GetPosition(this);
			var d = pos - _clickedPosition;
			if (_capturedLeft && !_capturedRight) MoveCamera(d.X, d.Y, 0);
			else if (!_capturedLeft && _capturedRight)
			{
				var vm = (MeshRenderer)DataContext;
				var cp = vm.CameraPosition;
				var yOffset = d.Y * 0.001 * Math.Sqrt(cp.X *cp.X + cp.Z * cp.Z);
				vm.CameraTarget = new Point3D(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
			}
			_clickedPosition = pos;
		}
		private void OnGrid_Mouse_LBU(object sender, MouseButtonEventArgs e)
		{
			_capturedLeft = false;
			if (!_capturedRight) Mouse.Capture(null);
		}
		private void OnGrid_MouseWheel(object sender, MouseWheelEventArgs e) => MoveCamera(0, 0, Math.Sign(e.Delta));
		private void OnGrid_Mouse_RBD(object sender, MouseButtonEventArgs e)
		{
			_clickedPosition = e.GetPosition(this);
			_capturedRight = true;
			Mouse.Capture(sender as UIElement);
		}
		private void OnGrid_Mouse_RBU(object sender, MouseButtonEventArgs e)
		{
			_capturedRight = false;
			if (!_capturedLeft) Mouse.Capture(null);
		}
		private void MoveCamera(double dx, double dy, int dz)
		{
			var vm = (MeshRenderer)DataContext;
			var v = new Vector3D(vm.CameraPosition.X, vm.CameraPosition.Y, vm.CameraPosition.Z);
			var r = v.Length;
			var theta = Math.Acos(v.Y / r);
			var phi = Math.Atan2(-v.Z, v.X);
			theta -= dy * 0.01;
			phi -= dx * 0.01;
			r *= 1.0 - 0.1 * dz; // dx is either +1 or -1
			theta = Math.Clamp(theta, 0.0001, Math.PI - 0.0001);
			v.X = r * Math.Sin(theta) * Math.Cos(phi);
			v.Z = -r * Math.Sin(theta) * Math.Sin(phi);
			v.Y = r * Math.Cos(theta);
			vm.CameraPosition = new Point3D(v.X, v.Y, v.Z);
		}
		internal static BitmapSource RenderToBitmap(MeshRenderer mesh, int width, int height)
		{
			var bmp = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Default);
			_geometryView.DataContext = mesh;
			_geometryView.Width = width;
			_geometryView.Height = height;
			_geometryView.Measure(new Size(width, height));
			_geometryView.Arrange(new Rect(0, 0, width, height));
			_geometryView.UpdateLayout();
			bmp.Render(_geometryView);
			return bmp;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryView"/> class.
        /// </summary>
        public GeometryView()
		{
			InitializeComponent();
			DataContextChanged += (s, e) => SetGeometry();
		}
	}
}
