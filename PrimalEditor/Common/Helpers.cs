using PrimalEditor.Content;
using PrimalEditor.Utilities.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PrimalEditor
{
    static class VisualExtensions
    {
        public static T? FindVisualParent<T>(this DependencyObject depObject) where T : DependencyObject
        {
            if (depObject is not Visual) return null;
            var parent = VisualTreeHelper.GetParent(depObject);
            while (parent != null)
            {
                if (parent is T type) return type;
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
    /// <summary>
    /// Contains accessibility methods which might be useful when performing a plethora of files and folders related tasks.
    /// </summary>
    public static class ContentHelper
    {
        /// <summary>
        /// Generates a string of given length with random characters
        /// </summary>
        /// <param name="length">The length of desired string. Defaults to 8.</param>
        /// <returns>A string which contains system generated random characters of given length</returns>
        public static string GetRandomString(int length = 8)
        {
            if (length <= 0) length = 8;
            var n = length / 11;
            var sb = new StringBuilder();
            for (int i = 0; i <= n; ++i) sb.Append(Path.GetRandomFileName().Replace(".", ""));
            return sb.ToString(0, length);
        }
        /// <summary>
        /// Determines whether the given path is a valid directory
        /// </summary>
        /// <param name="path">The path of the directory whose existence is in question</param>
        /// <returns>A boolean value based on whether the given path was indeed a directory</returns>
        public static bool IsDirectory(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return false;
        }
        /// <summary>
        /// Determines whether the given fileinfo belongs to a directory
        /// </summary>
        /// <param name="info">The fileinfo of the directory whose validity is in question</param>
        /// <returns>A non-nullable boolean value based on whether the given fileinfo was indeed a directory</returns>
        public static bool IsDirectory(this FileInfo info) => info.Attributes.HasFlag(FileAttributes.Directory);
        /// <summary>
        /// Compares between two given datetimes and checks if the later is indeed the later one
        /// </summary>
        /// <param name="date">The first datetime object to be compared</param>
        /// <param name="other">The second datetime object to be compared</param>
        /// <returns></returns>
        public static bool IsOlder(this DateTime date, DateTime other) => date < other;
        /// <summary>
        /// Cleans up invalid file characters and generates a very valid file name closest to the given one
        /// </summary>
        /// <param name="name">Takes the file name which needs to be fixed</param>
        /// <returns>A </returns>
        public static string SanitizeFileName(string name)
        {
            StringBuilder path = new(name[..(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)]);
            var file = new StringBuilder(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);
            foreach (var c in Path.GetInvalidPathChars()) path.Replace(c, '_');
            foreach (var c in Path.GetInvalidFileNameChars()) file.Replace(c, '_');
            return path.Append(file).ToString();
        }
        /// <summary>
        /// Computes the SHA-256 hash of the specified data.
        /// </summary>
        /// <param name="data">The data to compute the hash for.</param>
        /// <param name="offset">The offset in the data to start computing the hash from.</param>
        /// <param name="count">The number of bytes to compute the hash for. If this value is 0, the entire data is used.</param>
        /// <returns>The computed SHA-256 hash, or null if the data is null or empty.</returns>
        public static byte[]? ComputeHash(byte[] data, int offset = 0, int count = 0)
        {
            if (data?.Length <= 0) return null;
            using var sha256 = SHA256.Create();
            if (data == null) return data;
            return sha256.ComputeHash(data, offset, count > 0 ? count : data.Length);
        }
        /// <summary>
        /// Intializes the various methods required to actually import the file without disturbing the UI thread by running async.
        /// </summary>
        /// <param name="files">The file paths of files which need to be imported.</param>
        /// <param name="destination">The destination folder path to store the imported files.</param>
        /// <returns></returns>
        public static async Task ImportFilesAsync(string[] files, string destination)
        {
            try
            {
                Debug.Assert(!string.IsNullOrEmpty(destination));
                ContentWatcher.EnableFileWatcher(false);
                var tasks = files.Select(async file => await Task.Run(() => { Import(file, destination); }));
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to import files to {destination}");
                Debug.WriteLine(ex.Message);
            }
            finally
            {
                ContentWatcher.EnableFileWatcher(true);
            }
        }
        private static void Import(string file, string destination)
        {
            Debug.Assert(!string.IsNullOrEmpty(file));
            if (IsDirectory(file)) return;
            if (!destination.EndsWith(Path.DirectorySeparatorChar)) destination += Path.DirectorySeparatorChar;
            var name = Path.GetFileNameWithoutExtension(file).ToLower();
            var ext = Path.GetExtension(file).ToLower();
            Asset? asset = null;
            switch (ext)
            {
                case ".fbx": asset = new Content.Geometry(); break;
                case ".bmp": break;
                case ".png": break;
                case ".jpg": break;
                case ".jpeg": break;
                case ".tiff": break;
                case ".tif": break;
                case ".tga": break;
                case ".wav": break;
                case ".ogg": break;
                default: break;
            }
            if (asset != null) Import(asset, name, file, destination);
        }
        private static void Import(Asset asset, string name, string file, string destination)
        {
            Debug.Assert(asset != null);
            asset.FullPath = destination + name + Asset.AssetFileExtension;
            if (!string.IsNullOrEmpty(file)) asset.Import(file);
            asset.Save(asset.FullPath);
        }
        public static string VerifyDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "Path is null or empty.";
            }
            else if (!Directory.Exists(path))
            {
                return "Path does not exist.";
            }
            else if ((File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory)
            {
                return "Path is not a directory.";
            }
            else
            {
                return string.Empty;
            }
        }
    }
    /// <summary>
    /// Useful in calling commands from code behind without the boilerplate code.
    /// </summary>
    public static class CommandHelper
    {
        private static CancellationTokenSource _cts = new CancellationTokenSource();
        private static List<string> _sourceCommands = new List<string>();
        private static List<string> _processedCommands = new List<string>();
        private static CommandOutputRelay _commandOutputRelay = new CommandOutputRelay();
        internal static void CallCommand(ICommand? command, object? parameter = null)
        {
            if (command != null && command.CanExecute(parameter)) command.Execute(parameter);
        }
        internal static async Task RunCommandAsync(CommandOutputRelay commandOutputRelay, string command)
        {
            if (_cts.IsCancellationRequested) return;
            commandOutputRelay.Command = command;
            await commandOutputRelay.RunCommandAsync(_cts.Token);
            _processedCommands.Add(command);
        }
        internal static void TryStopCurrentCommandAndDisableFurtherProcessing()
        {
            _cts.Cancel();
        }
        internal static async Task RestartCallChainAsync()
        {
            _cts = new CancellationTokenSource();
            await CallCommandChainAsync(_commandOutputRelay, _sourceCommands);
        }
        internal static async Task ContinueCallChainAsync(bool fromNextItem = true)
        {
            _cts = new CancellationTokenSource();
            foreach (var command in _sourceCommands.GetRange(_processedCommands.Count - (fromNextItem ? 1 : 0), _sourceCommands.Count - _processedCommands.Count - (fromNextItem ? 1 : 0)))
            {
                await RunCommandAsync(_commandOutputRelay, command);
            }
        }
        internal static async Task CallCommandChainAsync(CommandOutputRelay commandOutputRelay, List<string> commands)
        {
            _commandOutputRelay = commandOutputRelay;
            _sourceCommands = commands;
            foreach (var item in commands)
            {
                await RunCommandAsync(commandOutputRelay, item);
            }
        }
    }
    internal class Node<T>
    {
        internal T? Value { get; set; }
        internal List<Node<T>?> Children { get; set; }

        internal Node(T value)
        {
            Value = value;
            Children = new List<Node<T>?>();
        }

        internal void AddChild(Node<T> child)
        {
            Children.Add(child);
        }
        internal static void AddChild(Node<T> child, Node<T> parent)
        {
            parent.Children.Add(child);
        }
        internal void RemoveChild(Node<T> child)
        {
            Children.Remove(child);
        }
        public void RemoveChildren()
        {
            Children.Clear();
        }
        public static void RemoveChildren(Node<T?> parent)
        {
            parent.Children.Clear();
        }
        internal List<Node<T>?> GetChildren()
        {
            return Children;
        }

        internal static List<Node<T>?>? GetChildren(Node<T> parent)
        {
            return parent?.Children;
        }
        public Node<T?>? FindParent(Node<T> child)
        {
            if (Children.Contains(child)) return this;
            foreach (var parent in from Node<T?> node in Children let parent = node.FindParent(child) where parent != null select parent) return parent;
            return null;
        }
        public Node<T?>? Find(Predicate<T?> match)
        {
            if (match(Value)) return this;
            foreach (var found in from Node<T?> child in Children let found = child.Find(match) where found != null select found) return found;
            return null;
        }
        public bool Contains(Predicate<T?> match)
        {
            return Find(match) != null;
        }
        public void Traverse(Action<Node<T?>?> action)
        {
            action(this);
            foreach (var child in Children) child?.Traverse(action);
        }
        public int Count()
        {
            int count = 1;
            foreach (var child in Children) if (child != null) count += child.Count();
            return count;
        }
        public static int Count(Node<T?> parent)
        {
            return parent.Children.Count;
        }
        public int Height()
        {
            int height = 0;
            foreach (var child in Children) if (child != null) height = Math.Max(height, child.Height());
            return height + 1;
        }
        public void CopyTo(T?[] array, int arrayIndex)
        {
            array[arrayIndex++] = Value;
            foreach (var child in Children) child?.CopyTo(array, arrayIndex);
        }
        public void Clear()
        {
            Value = default;
            Children.Clear();
        }
        internal int FindIndex(Predicate<Node<T>?> match)
        {
            var queue = new Queue<Node<T>>();
            queue.Enqueue(this);
            int index = -1;
            while (queue.Count > 0)
            {
                index++;
                Node<T> current = queue.Dequeue();
                if (match(current)) return index;
                foreach (var child in current.Children) if (child != null) queue.Enqueue(child);
            }
            return -1;
        }
        internal void RemoveAt(int index)
        {
            var queue = new Queue<Node<T>>();
            queue.Enqueue(this);
            int currentIndex = -1;
            while (queue.Count > 0)
            {
                currentIndex++;
                Node<T> current = queue.Dequeue();
                if (currentIndex == index)
                {
                    if (current == this) throw new InvalidOperationException("Cannot remove root node");
                    else
                    {
                        Node<T> parent = queue.Peek();
                        parent.Children.Remove(current);
                        return;
                    }
                }
                queue.Enqueue(current);
                foreach (var child in current.Children) if (child != null) queue.Enqueue(child);
            }
        }
        internal void Insert(int index, Node<T> item)
        {
            Children.Insert(index, item);
        }
        internal Node<T>? this[int index]
        {
            get => GetElementAtIndex(index);
            set => Children[index] = value;
        }
        internal Node<T>? GetElementAtIndex(int index)
        {
            Queue<Node<T>> queue = new();
            queue.Enqueue(this);
            int currentIndex = -1;
            while (queue.Count > 0)
            {
                currentIndex++;
                Node<T> current = queue.Dequeue();
                if (currentIndex == index) return current;
                foreach (var child in current.Children) if (child != null) queue.Enqueue(child);
            }
            return null;
        }
    }
}
