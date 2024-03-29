﻿using PrimalEditor.DLLWrapper;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PrimalEditor.Content
{
    enum PrimitiveMeshType
    {
        Plane,
        Cube,
        UvSphere,
        IcoSphere,
        Cylinder,
        Capsule
    }
    class Mesh : ViewModelBase
    {
        private int _vertexSize;
        public int VertexSize
        {
            get => _vertexSize;
            set
            {
                if (_vertexSize == value) return;
                _vertexSize = value;
                OnPropertyChanged(nameof(VertexSize));
            }
        }
        private int _vertexCount;
        public int VertexCount
        {
            get => _vertexCount;
            set
            {
                if (_vertexCount == value) return;
                _vertexCount = value;
                OnPropertyChanged(nameof(VertexCount));
            }
        }
        private int _indexSize;
        public int IndexSize
        {
            get => _indexSize;
            set
            {
                if (_indexSize == value) return;
                _indexSize = value;
                OnPropertyChanged(nameof(IndexSize));
            }
        }
        private int _indexCount;
        public int IndexCount
        {
            get => _indexCount; set
            {
                if (_indexCount == value) return;
                _indexCount = value;
                OnPropertyChanged(nameof(IndexCount));
            }
        }
        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public byte[]? Vertices { get; set; }
        public byte[]? Indices { get; set; }
    }
    class MeshLOD : ViewModelBase
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private float _lodThreshold;
        public float LodThreshold
        {
            get => _lodThreshold;
            set
            {
                if (_lodThreshold == value) return;
                _lodThreshold = value;
                OnPropertyChanged(nameof(LodThreshold));
            }
        }
        public ObservableCollection<Mesh> Meshes { get; } = new ObservableCollection<Mesh>();
    }
    class LODGroup : ViewModelBase
    {
        private string? _name;
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        public ObservableCollection<MeshLOD> LODs { get; } = new ObservableCollection<MeshLOD>();
    }
    class GeometryImportSettings : ViewModelBase
    {
        private bool _calculateNormals;
        public bool CalculateNormals
        {
            get => _calculateNormals;
            set
            {
                if (_calculateNormals == value) return;
                _calculateNormals = value;
                OnPropertyChanged(nameof(CalculateNormals));
            }
        }
        private bool _calculateTangents;
        public bool CalculateTangents
        {
            get => _calculateTangents;
            set
            {
                if (_calculateTangents == value) return;
                _calculateTangents = value;
                OnPropertyChanged(nameof(CalculateTangents));
            }
        }
        private float _smoothingAngle;
        public float SmoothingAngle
        {
            get => _smoothingAngle;
            set
            {
                if (_smoothingAngle == value) return;
                _smoothingAngle = value;
                OnPropertyChanged(nameof(SmoothingAngle));
            }
        }
        private bool _reverseHandedness;
        public bool ReverseHandedness
        {
            get => _reverseHandedness;
            set
            {
                if (_reverseHandedness == value) return;
                _reverseHandedness = value;
                OnPropertyChanged(nameof(ReverseHandedness));
            }
        }
        private bool _importEmbeddedTextures;
        public bool ImportEmbeddedTextures
        {
            get => _importEmbeddedTextures;
            set
            {
                if (_importEmbeddedTextures == value) return;
                _importEmbeddedTextures = value;
                OnPropertyChanged(nameof(ImportEmbeddedTextures));
            }
        }
        private bool _importAnimations;
        public bool ImportAnimations
        {
            get => _importAnimations;
            set
            {
                if (_importAnimations == value) return;
                _importAnimations = value;
                OnPropertyChanged(nameof(ImportAnimations));
            }
        }
        public GeometryImportSettings()
        {
            CalculateNormals = false;
            CalculateTangents = false;
            SmoothingAngle = 178f;
            ReverseHandedness = false;
            ImportEmbeddedTextures = true;
            ImportAnimations = true;
        }
        public void ToBinary(BinaryWriter writer)
        {
            writer.Write(CalculateNormals);
            writer.Write(CalculateTangents);
            writer.Write(SmoothingAngle);
            writer.Write(ReverseHandedness);
            writer.Write(ImportEmbeddedTextures);
            writer.Write(ImportAnimations);
        }
        internal void FromBinary(BinaryReader reader)
        {
            CalculateNormals = reader.ReadBoolean();
            CalculateTangents = reader.ReadBoolean();
            SmoothingAngle = reader.ReadSingle();
            ReverseHandedness = reader.ReadBoolean();
            ImportEmbeddedTextures = reader.ReadBoolean();
            ImportAnimations = reader.ReadBoolean();
        }
    }
    class Geometry : Asset
    {
        private readonly List<LODGroup> _lodGroups = new();
        private readonly object _lock = new();
        public GeometryImportSettings ImportSettings { get; } = new GeometryImportSettings();
        public LODGroup? GetLODGroup(int lodGroup = 0)
        {
            Debug.Assert(lodGroup >= 0 && lodGroup < _lodGroups.Count);
            return (lodGroup < _lodGroups.Count) ? _lodGroups[lodGroup] : null;
        }
        internal void FromRawData(byte[] data)
        {
            Debug.Assert(data?.Length > 0);
            _lodGroups.Clear();
            using var reader = new BinaryReader(new MemoryStream(data));
            // skip scene name string
            var s = reader.ReadInt32();
            reader.BaseStream.Position += s;
            // get number of LODs
            var numLODGroups = reader.ReadInt32();
            Debug.Assert(numLODGroups > 0);
            for (int i = 0; i < numLODGroups; ++i)
            {
                // get LOD group's name
                s = reader.ReadInt32();
                string lodGroupName;
                if (s > 0)
                {
                    var nameBytes = reader.ReadBytes(s);
                    lodGroupName = Encoding.UTF8.GetString(nameBytes);
                }
                else lodGroupName = $"lod_{ContentHelper.GetRandomString()}";
                // get number of meshes in this LOD group
                var numMeshes = reader.ReadInt32();
                Debug.Assert(numMeshes > 0);
                var lods = ReadMeshLODs(numMeshes, reader);
                var lodGroup = new LODGroup() { Name = lodGroupName };
                lods.ForEach(l => lodGroup.LODs.Add(l));
                _lodGroups.Add(lodGroup);
            }
        }
        private static List<MeshLOD> ReadMeshLODs(int numMeshes, BinaryReader reader)
        {
            var lodIds = new List<int>();
            var lodList = new List<MeshLOD>();
            for (int i = 0; i < numMeshes; ++i) ReadMeshes(reader, lodIds, lodList);
            return lodList;
        }
        private static void ReadMeshes(BinaryReader reader, List<int> lodIds, List<MeshLOD> lodList)
        {
            // get mesh's name
            var s = reader.ReadInt32();
            string meshName;
            if (s > 0)
            {
                var nameBytes = reader.ReadBytes(s);
                meshName = Encoding.UTF8.GetString(nameBytes);
            }
            else meshName = $"mesh_{ContentHelper.GetRandomString()}";
            var mesh = new Mesh() { Name = meshName };
            var lodId = reader.ReadInt32();
            mesh.VertexSize = reader.ReadInt32();
            mesh.VertexCount = reader.ReadInt32();
            mesh.IndexSize = reader.ReadInt32();
            mesh.IndexCount = reader.ReadInt32();
            var lodThreshold = reader.ReadInt32();
            var vertexBufferSize = mesh.VertexSize * mesh.VertexCount;
            var indexBufferSize = mesh.IndexSize * mesh.IndexCount;
            mesh.Vertices = reader.ReadBytes(vertexBufferSize);
            mesh.Indices = reader.ReadBytes(indexBufferSize);
            MeshLOD lod;
            if (ID.IsValid(lodId) && lodIds.Contains(lodId))
            {
                lod = lodList[lodIds.IndexOf(lodId)];
                Debug.Assert(lod != null);
            }
            else
            {
                lodIds.Add(lodId);
                lod = new MeshLOD() { Name = meshName, LodThreshold = lodThreshold };
                lodList.Add(lod);
            }
            lod.Meshes.Add(mesh);
        }
        public override void Import(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(!string.IsNullOrEmpty(FullPath));
            var ext = Path.GetExtension(file).ToLower();
            SourcePath = file;
            try
            {
                if (ext == ".fbx") ImportFbx(file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                var msg = $"Failed to read {file} for import.";
                Debug.WriteLine(msg);
                Logger.Log(MessageType.Error, msg);
            }
        }
        private void ImportFbx(string file)
        {
            Logger.Log(MessageType.Info, $"Importing FBX file {file}");
            var tempPath = Application.Current.Dispatcher.Invoke(() => Project.Current?.TempFolder);
            if (string.IsNullOrEmpty(tempPath)) return;
            lock (_lock)
            {
                if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);
            }
            var tempFile = $"{tempPath}{ContentHelper.GetRandomString()}.fbx";
            File.Copy(file, tempFile, true);
            ContentToolsAPI.ImportFbx(tempFile, this);
        }
        public override void Load(string file)
        {
            Debug.Assert(File.Exists(file));
            Debug.Assert(Path.GetExtension(file).ToLower() == AssetFileExtension);
            try
            {
                byte[]? data = null;
                using (var reader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read)))
                {
                    ReadAssetFileHeader(reader);
                    ImportSettings.FromBinary(reader);
                    int dataLength = reader.ReadInt32();
                    Debug.Assert(dataLength > 0);
                    data = reader.ReadBytes(dataLength);
                }
                Debug.Assert(data.Length > 0);
                using (var reader = new BinaryReader(new MemoryStream(data)))
                {
                    LODGroup lodGroup = new()
                    {
                        Name = reader.ReadString()
                    };
                    var lodCount = reader.ReadInt32();
                    for (int i = 0; i < lodCount; ++i) lodGroup.LODs.Add(BinaryToLOD(reader));
                    _lodGroups.Clear();
                    _lodGroups.Add(lodGroup);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to load geometry asset from file: {file}");
            }
        }
        public override IEnumerable<string>? Save(string file)
        {
            Debug.Assert(_lodGroups.Any());
            var savedFiles = new List<string>();
            if (!_lodGroups.Any()) return savedFiles;
            var path = Path.GetDirectoryName(file) + Path.DirectorySeparatorChar;
            var filename = Path.GetFileNameWithoutExtension(file);
            try
            {
                foreach (var lodGroup in _lodGroups)
                {
                    Debug.Assert(lodGroup.LODs.Any());
                    // Use the name of most detailed LOD for file name
                    var meshFileName = ContentHelper.SanitizeFileName(path + filename + ((_lodGroups.Count > 1) ? "_" + ((lodGroup.LODs.Count > 1) ? lodGroup.Name : lodGroup.LODs[0].Name) : string.Empty)) + AssetFileExtension;
                    // NOTE: we have to make a different id for each new asset file, but if a geometry asset file with the same name already exists then we use its guid instead.
                    Guid = TryGetAssetInfo(meshFileName) is AssetInfo info && info.Type == Type ? info.Guid : Guid.NewGuid();
                    byte[]? data = null;
                    using (var writer = new BinaryWriter(new MemoryStream()))
                    {
                        if (lodGroup.Name == null) return null;
                        writer.Write(lodGroup.Name);
                        writer.Write(lodGroup.LODs.Count);
                        var hashes = new List<byte>();
                        foreach (var lod in lodGroup.LODs)
                        {
                            LODToBinary(lod, writer, out var hash);
                            if (hash == null) continue;
                            hashes.AddRange(hash);
                        }
                        Hash = ContentHelper.ComputeHash(hashes.ToArray());
                        data = (writer.BaseStream as MemoryStream)?.ToArray();
                        Icon = GenerateIcon(lodGroup.LODs[0]);
                    }
                    Debug.Assert(data?.Length > 0);
                    using (var writer = new BinaryWriter(File.Open(meshFileName, FileMode.Create, FileAccess.Write)))
                    {
                        WriteAssetFileHeader(writer);
                        ImportSettings.ToBinary(writer);
                        writer.Write(data.Length);
                        writer.Write(data);
                    }
                    Logger.Log(MessageType.Info, $"Saved geometry to {meshFileName}");
                    savedFiles.Add(meshFileName);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Logger.Log(MessageType.Error, $"Failed to save geometry to {file}");
            }
            return savedFiles;
        }
        private void LODToBinary(MeshLOD lod, BinaryWriter writer, out byte[]? hash)
        {
            hash = null;
            if (lod.Name == null) return;
            writer.Write(lod.Name);
            writer.Write(lod.LodThreshold);
            writer.Write(lod.Meshes.Count);
            var meshDataBegin = writer.BaseStream.Position;
            foreach (var mesh in lod.Meshes)
            {
                if (mesh.Name == null) continue;
                writer.Write(mesh.Name);
                writer.Write(mesh.VertexSize);
                writer.Write(mesh.VertexCount);
                writer.Write(mesh.IndexSize);
                writer.Write(mesh.IndexCount);
                if (mesh.Vertices == null) continue;
                writer.Write(mesh.Vertices);
                if (mesh.Indices == null) continue;
                writer.Write(mesh.Indices);
            }
            var meshDataSize = writer.BaseStream.Position - meshDataBegin;
            Debug.Assert(meshDataSize > 0);
            var buffer = (writer.BaseStream as MemoryStream)?.ToArray();
            hash = ContentHelper.ComputeHash(buffer, (int)meshDataBegin, (int)meshDataSize);
        }
        private MeshLOD BinaryToLOD(BinaryReader reader)
        {
            var lod = new MeshLOD
            {
                Name = reader.ReadString(),
                LodThreshold = reader.ReadSingle()
            };
            var meshCount = reader.ReadInt32();
            for (int i = 0; i < meshCount; ++i)
            {
                var mesh = new Mesh()
                {
                    Name = reader.ReadString(),
                    VertexSize = reader.ReadInt32(),
                    VertexCount = reader.ReadInt32(),
                    IndexSize = reader.ReadInt32(),
                    IndexCount = reader.ReadInt32()
                };
                mesh.Vertices = reader.ReadBytes(mesh.VertexSize * mesh.VertexCount);
                mesh.Indices = reader.ReadBytes(mesh.IndexSize * mesh.IndexCount);
                lod.Meshes.Add(mesh);
            }
            return lod;
        }
        private byte[] GenerateIcon(MeshLOD lod)
        {
            var width = ContentInfo.IconWidth * 4;
            using var memStream = new MemoryStream();
            BitmapSource? bmp = null;
            // NOTE: it's not good practice to use a WPF control (view) in the ViewModel. But we need to make an exception for this case, for as long as we don't have a graphics renderer that we can use for screenshots.
            Application.Current.Dispatcher.Invoke(() =>
            {
                bmp = Editors.GeometryView.RenderToBitmap(new Editors.MeshRenderer(lod, null), width, width);
                bmp = new TransformedBitmap(bmp, new ScaleTransform(0.25, 0.25, 0.5, 0.5));
                memStream.SetLength(0);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(memStream);
            });
            return memStream.ToArray();
        }
        public Geometry() : base(AssetType.Mesh) { }
    }
}
