using PrimalEditor.ContentToolsAPIStructs;
using PrimalEditor.DLLWrapper;
using PrimalEditor.Editors;
using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
namespace PrimalEditor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        private static readonly List<ImageBrush> _textures = new();
        private void OnPrimitiveType_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();
        private void OnSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdatePrimitive();
        private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();
        private float Value(ScalarBox scalarBox, float min)
        {
            _ = float.TryParse(scalarBox.Value, out float result);
            return Math.Max(result, min);
        }
        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;
            var primitiveType = (PrimitiveMeshType)primTypeComboBox.SelectedItem;
            var info = new PrimitiveInitInfo() { Type = primitiveType };
            var smoothingAngle = 0;
            switch (primitiveType)
            {
                case PrimitiveMeshType.Plane:
                    {
                        info.SegmentX = (int)xSliderPlane.Value;
                        info.SegmentZ = (int)zSliderPlane.Value;
                        info.Size.X = Value(widthScalarBoxPlane, 0.001f);
                        info.Size.Z = Value(lengthScalarBoxPlane, 0.001f);
                        break;
                    }
                case PrimitiveMeshType.Cube: return;
                case PrimitiveMeshType.UvSphere:
                    {
                        info.SegmentX = (int)xSliderUvSphere.Value;
                        info.SegmentY = (int)ySliderUvSphere.Value;
                        info.Size.X = Value(xScalarBoxUvSphere, 0.001f);
                        info.Size.Y = Value(yScalarBoxUvSphere, 0.001f);
                        info.Size.Z = Value(zScalarBoxUvSphere, 0.001f);
                        smoothingAngle = (int)angleSliderUvSphere.Value;
                    }
                    break;
                case PrimitiveMeshType.IcoSphere: return;
                case PrimitiveMeshType.Cylinder: return;
                case PrimitiveMeshType.Capsule: return;
                default: break;
            }
            var geometry = new Geometry();
            geometry.ImportSettings.SmoothingAngle = smoothingAngle;
            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);
            ((GeometryEditor)DataContext).SetAsset(geometry);
            OnTexture_CheckBox_Click(textureCheckBox, null);
        }
        private static void LoadTextures()
        {
            var Uris = new List<Uri>
            {
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/Screenshot.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/Screenshot.png"),
                new Uri("pack://application:,,,/Resources/PrimitiveMeshView/Screenshot.png"),
            };
            _textures.Clear();
            foreach (var uri in Uris)
            {
                var resources = Application.GetResourceStream(uri);
                using var reader = new BinaryReader(resources.Stream);
                var data = reader.ReadBytes((int)resources.Stream.Length);
                var imageSource = new ImageSourceConverter().ConvertFrom(data) as BitmapSource;
                imageSource?.Freeze();
                var brush = new ImageBrush(imageSource)
                {
                    Transform = new ScaleTransform(1, -1, 0.5, 0.5),
                    ViewportUnits = BrushMappingMode.Absolute
                };
                brush.Freeze();
                _textures.Add(brush);
            }
        }
        static PrimitiveMeshDialog() => LoadTextures();
        /// <summary>
        /// Initializes a new instance of the <see cref="PrimitiveMeshDialog"/> class.
        /// </summary>
        public PrimitiveMeshDialog()
        {
            InitializeComponent();
            Loaded += (s, e) => UpdatePrimitive();
            ForceCursor = true;
        }
        private void OnTexture_CheckBox_Click(object sender, RoutedEventArgs? e)
        {
            Brush brush = Brushes.White;
            if (((CheckBox)sender).IsChecked == true) brush = _textures[(int)primTypeComboBox.SelectedItem];
            if (DataContext is not GeometryEditor vm) return;
            foreach (var mesh in vm.MeshRenderer.Meshes) mesh.Diffuse = brush;
        }
        private void OnSave_Button_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveDialog();
            if (dlg.ShowDialog() != true) return;
            Debug.Assert(!string.IsNullOrEmpty(dlg.SaveFilePath));
            var asset = ((IAssetEditor)DataContext).Asset;
            Debug.Assert(asset != null);
            asset.Save(dlg.SaveFilePath);
            // NOTE: you can choose to close this window after saving.
        }
    }
}
