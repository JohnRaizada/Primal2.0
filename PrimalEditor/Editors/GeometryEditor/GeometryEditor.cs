﻿using PrimalEditor.Content;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Editors
{
    // NOTE: the purpose of this class is to enable viewing 3D geometry in WPF while we don't have a graphics renderer in the game engine. When we have a renderer, this class and the WPF viewer will become obsolete.
    class MeshRendererVertexData : ViewModelBase
    {
        private bool _isHighlighted;
        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted == value) return;
                _isHighlighted = value;
                OnPropertyChanged(nameof(IsHighlighted));
                OnPropertyChanged(nameof(Diffuse));
            }
        }
        private bool _isIsolated;
        public bool IsIsolated
        {
            get => _isIsolated;
            set
            {
                if (_isIsolated == value) return;
                _isIsolated = value;
                OnPropertyChanged(nameof(IsIsolated));
            }
        }
        private Brush _specular = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#ff111111"));
        public Brush Specular
        {
            get => _specular;
            set
            {
                if (_specular == value) return;
                _specular = value;
                OnPropertyChanged(nameof(Specular));
            }
        }
        private Brush _diffuse = Brushes.White;
        public Brush Diffuse
        {
            get => _isHighlighted ? Brushes.Orange : _diffuse;
            set
            {
                if (_diffuse == value) return;
                _diffuse = value;
                OnPropertyChanged(nameof(Diffuse));
            }
        }
        public string? Name { get; set; }
        public Point3DCollection Positions { get; } = new Point3DCollection();
        public Vector3DCollection Normals { get; } = new Vector3DCollection();
        public PointCollection UVs { get; } = new PointCollection();
        public Int32Collection Indices { get; } = new Int32Collection();
    }
    // NOTE: the purpose of this class is to enable viewing 3D geometry in WPF while we don't have a graphics renderer in the game engine. When we have a renderer, this class and the WPF viewer will become obsolete.
    class MeshRenderer : ViewModelBase
    {
        public ObservableCollection<MeshRendererVertexData> Meshes { get; } = new ObservableCollection<MeshRendererVertexData>();
        private Vector3D _cameraDirection = new(0, 0, -10);
        public Vector3D CameraDirection
        {
            get => _cameraDirection;
            set
            {
                if (_cameraDirection == value) return;
                _cameraDirection = value;
                OnPropertyChanged(nameof(CameraDirection));
            }
        }
        private Point3D _cameraPosition = new(0, 0, 10);
        public Point3D CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (_cameraPosition == value) return;
                _cameraPosition = value;
                CameraDirection = new Vector3D(-value.X, -value.Y, -value.Z);
                OnPropertyChanged(nameof(OffsetCameraPosition));
                OnPropertyChanged(nameof(CameraPosition));
            }
        }
        private Point3D _cameraTarget = new(0, 0, 0);
        public Point3D CameraTarget
        {
            get => _cameraTarget; set
            {
                if (_cameraTarget == value) return;
                _cameraTarget = value;
                OnPropertyChanged(nameof(OffsetCameraPosition));
                OnPropertyChanged(nameof(CameraTarget));
            }
        }
        public Point3D OffsetCameraPosition => new(CameraPosition.X + CameraTarget.X, CameraPosition.Y + CameraTarget.Y, CameraPosition.Z + CameraTarget.Z);
        private Color _keyLight = (Color)ColorConverter.ConvertFromString("#ffaeaeae");
        public Color KeyLight
        {
            get => _keyLight;
            set
            {
                if (_keyLight == value) return;
                _keyLight = value;
                OnPropertyChanged(nameof(KeyLight));
            }
        }
        private Color _skyLight = (Color)ColorConverter.ConvertFromString("#ff111b30");
        public Color SkyLight
        {
            get => _skyLight;
            set
            {
                if (value == _skyLight) return;
                _skyLight = value;
                OnPropertyChanged(nameof(SkyLight));
            }
        }
        private Color _groundLight = (Color)ColorConverter.ConvertFromString("#ff3f2f1e");
        public Color GroundLight
        {
            get => _groundLight;
            set
            {
                if (_groundLight == value) return;
                _groundLight = value;
                OnPropertyChanged(nameof(GroundLight));
            }
        }
        private Color _ambientLight = (Color)ColorConverter.ConvertFromString("#ff3b3b3b");
        public Color AmbientLight
        {
            get => _ambientLight;
            set
            {
                if (_ambientLight == value) return;
                _ambientLight = value;
                OnPropertyChanged(nameof(AmbientLight));
            }
        }
        public MeshRenderer(MeshLOD? lod, MeshRenderer? old)
        {
            Debug.Assert(lod?.Meshes.Any() == true);
            // Calculate vertex size minus the position and normal vectors;
            var offset = lod.Meshes[0].VertexSize - 3 * sizeof(float) - sizeof(int) - 2 * sizeof(short);
            // In order to set up camera position and target properly, we need to figure out how big this object is that we're rendering. Hence, we need to know its bounding box.
            double minX, minY, minZ; minX = minY = minZ = double.MaxValue;
            double maxX, maxY, maxZ; maxX = maxY = maxZ = double.MinValue;
            Vector3D avgNormal = new();
            // This is to unpack the packed normals;
            var intervals = 2.0f / ((1 << 16) - 1);
            foreach (var mesh in lod.Meshes)
            {
                var vertexData = new MeshRendererVertexData() { Name = mesh.Name };
                // Unpack all vertices
                using (var reader = new BinaryReader(new MemoryStream(mesh.Vertices)))
                    for (int i = 0; i < mesh.VertexCount; ++i)
                    {
                        // Read positions
                        var posX = reader.ReadSingle();
                        var posY = reader.ReadSingle();
                        var posZ = reader.ReadSingle();
                        var signs = (reader.ReadUInt32() >> 24) & 0x000000ff;
                        vertexData.Positions.Add(new Point3D(posX, posY, posZ));
                        // Adjust the bounding box;
                        minX = Math.Min(minX, posX); maxX = Math.Max(maxX, posX);
                        minY = Math.Min(minY, posY); maxY = Math.Max(maxY, posY);
                        minZ = Math.Min(minZ, posZ); maxZ = Math.Max(minZ, posZ);
                        //Read normals
                        var nrmX = reader.ReadUInt16() * intervals - 1.0f;
                        var nrmY = reader.ReadUInt16() * intervals - 1.0f;
                        var nrmZ = Math.Sqrt(Math.Clamp(1f - (nrmX * nrmX + nrmY * nrmY), 0f, 1f)) * ((signs & 0x2) - 1f);
                        var normal = new Vector3D(nrmX, nrmY, nrmZ);
                        normal.Normalize();
                        vertexData.Normals.Add(normal);
                        avgNormal += normal;
                        // Read UVs (skip tangent and joint data)
                        reader.BaseStream.Position += (offset - sizeof(float) * 2);
                        var u = reader.ReadSingle();
                        var v = reader.ReadSingle();
                        vertexData.UVs.Add(new Point(u, v));
                    }
                using (var reader = new BinaryReader(new MemoryStream(mesh.Indices)))
                    if (mesh.IndexSize == sizeof(short)) for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadUInt16());
                    else for (int i = 0; i < mesh.IndexCount; ++i) vertexData.Indices.Add(reader.ReadInt32());
                vertexData.Positions.Freeze();
                vertexData.Normals.Freeze();
                vertexData.UVs.Freeze();
                vertexData.Indices.Freeze();
                Meshes.Add(vertexData);
            }
            // set camera target and position
            if (old != null)
            {
                CameraTarget = old.CameraTarget;
                CameraPosition = old.CameraPosition;
            }
            else
            {
                // compute bounding box dimensions
                var width = maxX - minX;
                var height = maxY - minY;
                var depth = maxZ - minZ;
                var radius = new Vector3D(height, width, depth).Length * 1.2;
                if (avgNormal.Length > 0.8)
                {
                    avgNormal.Normalize();
                    avgNormal *= radius;
                    CameraPosition = new Point3D(avgNormal.X, avgNormal.Y, avgNormal.Z);
                }
                else CameraPosition = new Point3D(width, height * 0.5, radius);
                CameraTarget = new Point3D(minX + width * 0.5, minY + height * 0.5, minZ + depth * 0.5);
            }
        }
    }
    class GeometryEditor : ViewModelBase, IAssetEditor
    {
        Asset? IAssetEditor.Asset => Geometry;
        private Content.Geometry? _geometry;
        public Content.Geometry? Geometry
        {
            get => _geometry;
            set
            {
                if (_geometry == value) return;
                _geometry = value;
                OnPropertyChanged(nameof(Geometry));
            }
        }
        private MeshRenderer? _meshRenderer;
        public MeshRenderer? MeshRenderer
        {
            get => _meshRenderer;
            set
            {
                if (_meshRenderer == value) return;
                _meshRenderer = value;
                OnPropertyChanged(nameof(MeshRenderer));
                var lods = Geometry?.GetLODGroup()?.LODs;
                MaxLODIndex = (lods?.Count > 0) ? lods.Count - 1 : 0;
                OnPropertyChanged(nameof(MaxLODIndex));
                if (lods?.Count <= 1) return;
                if (MeshRenderer == null) return;
                MeshRenderer.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MeshRenderer.OffsetCameraPosition) && AutoLOD) ComputeLOD(lods);
                };
                ComputeLOD(lods);
            }
        }
        private bool _autoLOD = true;
        public bool AutoLOD
        {
            get => _autoLOD;
            set
            {
                if (_autoLOD == value) return;
                _autoLOD = value;
                OnPropertyChanged(nameof(AutoLOD));
            }
        }
        public int MaxLODIndex { get; private set; }
        private int _lodIndex;
        public int LODIndex
        {
            get => _lodIndex;
            set
            {
                var lods = Geometry?.GetLODGroup()?.LODs;
                if (lods == null) return;
                value = Math.Clamp(value, 0, lods.Count);
                if (_lodIndex == value) return;
                _lodIndex = value;
                OnPropertyChanged(nameof(LODIndex));
                MeshRenderer = new MeshRenderer(lods[value], MeshRenderer);
            }
        }
        private void ComputeLOD(IList<MeshLOD>? lods)
        {
            if (!AutoLOD) return;
            if (MeshRenderer == null) return;
            var p = MeshRenderer.OffsetCameraPosition;
            var distance = new Vector3D(p.X, p.Y, p.Z).Length;
            for (int i = MaxLODIndex; i >= 0; --i)
            {
                if (lods?[i].LodThreshold >= distance) continue;
                LODIndex = i;
                break;
            }
        }
        public void SetAsset(Asset asset)
        {
            Debug.Assert(asset is Content.Geometry);
            if (asset is not Content.Geometry geometry) return;
            Geometry = geometry;
            var numLods = geometry.GetLODGroup()?.LODs.Count;
            if (LODIndex >= numLods)
            {
                LODIndex = (int)(numLods - 1);
                return;
            }
            MeshRenderer = new MeshRenderer(Geometry.GetLODGroup()?.LODs[0], MeshRenderer);
        }
        public async void SetAsset(AssetInfo info)
        {
            try
            {
                Debug.Assert(info != null && File.Exists(info.FullPath));
                var geometry = new Content.Geometry();
                await Task.Run(() =>
                {
                    geometry.Load(info.FullPath);
                });
                SetAsset(geometry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
