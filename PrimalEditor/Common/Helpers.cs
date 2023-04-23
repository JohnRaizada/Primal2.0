using PrimalEditor.Content;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

namespace PrimalEditor
{
    static class VisualExtensions
    {
        public static T FindVisualParent<T>(this DependencyObject depObject) where T : DependencyObject
        {
            if (!(depObject is Visual)) return null;
            var parent = VisualTreeHelper.GetParent(depObject);
            while (parent != null)
            {
                if (parent is T type)
                {
                    return type;
                }
                parent = VisualTreeHelper.GetParent(parent);
            }
            return null;
        }
    }
    public static class ContentHelper
    {
        public static string GetRandomString(int length = 8)
        {
            if (length <= 0) length = 8;
            var n = length / 11;
            var sb = new StringBuilder();
            for (int i = 0; i <= n; ++i)
            {
                sb.Append(Path.GetRandomFileName().Replace(".", ""));
            }
            return sb.ToString(0, length);
        }
        public static bool IsDirectory(string path)
        {
            try
            {
                return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
            }
            catch (Exception ex) { Debug.WriteLine(ex.Message); }
            return false;
        }
        public static bool IsDirectory(this FileInfo info) => info.Attributes.HasFlag(FileAttributes.Directory);
        public static bool IsOlder(this DateTime date, DateTime other) => date < other;
        public static string SanitizeFileName(string name)
        {
            var path = new StringBuilder(name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar) + 1));
            var file = new StringBuilder(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);
            foreach (var c in Path.GetInvalidPathChars())
            {
                path.Replace(c, '_');
            }
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                file.Replace(c, '_');
            }
            return path.Append(file).ToString();
        }

        public static byte[] ComputeHash(byte[] data, int offset = 0, int count = 0)
        {
            if (data?.Length > 0)
            {
                using var sha256 = SHA256.Create();
                return sha256.ComputeHash(data, offset, count > 0 ? count : data.Length);
            }
            return null;
        }

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
            Asset asset = null;
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
                default:
                    break;
            }
            if (asset != null)
            {
                Import(asset, name, file, destination);
            }
        }
        private static void Import(Asset asset, string name, string file, string destination)
        {
            Debug.Assert(asset != null);
            asset.FullPath = destination + name + Asset.AssetFileExtension;
            if (!string.IsNullOrEmpty(file))
            {
                asset.Import(file);
            }
            asset.Save(asset.FullPath);
            return;
        }
    }
    public static class CommandHelper
    {
        internal static void CallCommand(ICommand command, object parameter = null)
        {
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
    }
    internal class Node<T>
    {
        /*TODO: Implement these functionality based on need.
         * AddRange: Adds a range of nodes to the tree.
         * AsReadOnly: Returns a read-only wrapper for the tree.
         * BinarySearch: Searches the tree for a node using a binary search algorithm.
         * ConvertAll: Converts the nodes in the tree to another type.
         * Exists: Determines whether the tree contains a node that matches a specific condition.
         * FindAll: Searches the tree for all nodes with a specific value and returns them.
         * FindLast: Searches the tree for the last node with a specific value and returns it.
         * FindLastIndex: Searches the tree for the last node with a specific value and returns its index.
         * ForEach: Performs an action on each node in the tree.
         * IndexOf: Returns the index of a specific node in the tree.
         * InsertRange: Inserts a range of nodes into the tree at a specific position.
         * LastIndexOf: Returns the index of the last occurrence of a specific node in the tree.
         * RemoveAll: Removes all nodes from the tree that match a specific condition.
         * RemoveRange: Removes a range of nodes from the tree.
         * Reverse: Reverses the order of the nodes in the tree.
         * Sort: Sorts the nodes in the tree.
         * GetRange: Returns a range of nodes from the tree.*/
        internal T Value { get; set; }
        internal List<Node<T>> Children { get; set; }

        internal Node(T value)
        {
            Value = value;
            Children = new List<Node<T>>();
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
        public static void RemoveChildren(Node<T> parent)
        {
            parent.Children.Clear();
        }
        internal List<Node<T>> GetChildren()
        {
            return Children;
        }

        internal static List<Node<T>> GetChildren(Node<T> parent)
        {
            return parent.Children;
        }
        public Node<T> FindParent(Node<T> child)
        {
            if (Children.Contains(child))
                return this;

            foreach (Node<T> node in Children)
            {
                Node<T> parent = node.FindParent(child);
                if (parent != null)
                    return parent;
            }

            return null;
        }
        public Node<T> Find(Predicate<T> match)
        {
            if (match(Value))
                return this;

            foreach (Node<T> child in Children)
            {
                Node<T> found = child.Find(match);
                if (found != null)
                    return found;
            }

            return null;
        }
        public bool Contains(Predicate<T> match)
        {
            return Find(match) != null;
        }

        public void Traverse(Action<Node<T>> action)
        {
            action(this);

            foreach (Node<T> child in Children)
                child.Traverse(action);
        }

        public int Count()
        {
            int count = 1;

            foreach (Node<T> child in Children)
                count += child.Count();

            return count;
        }
        public static int Count(Node<T> parent)
        {
            return parent.Children.Count;
        }
        public int Height()
        {
            int height = 0;

            foreach (Node<T> child in Children)
                height = Math.Max(height, child.Height());

            return height + 1;
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            array[arrayIndex++] = Value;

            foreach (Node<T> child in Children)
                child.CopyTo(array, arrayIndex);
        }

        public void Clear()
        {
            Value = default(T);
            Children.Clear();
        }
        internal int FindIndex(Predicate<Node<T>> match)
        {
            Queue<Node<T>> queue = new Queue<Node<T>>();
            queue.Enqueue(this);
            int index = -1;

            while (queue.Count > 0)
            {
                index++;
                Node<T> current = queue.Dequeue();
                if (match(current))
                    return index;

                foreach (Node<T> child in current.Children)
                    queue.Enqueue(child);
            }

            return -1;
        }
        internal void RemoveAt(int index)
        {
            Queue<Node<T>> queue = new Queue<Node<T>>();
            queue.Enqueue(this);
            int currentIndex = -1;

            while (queue.Count > 0)
            {
                currentIndex++;
                Node<T> current = queue.Dequeue();
                if (currentIndex == index)
                {
                    if (current == this)
                        throw new InvalidOperationException("Cannot remove root node");
                    else
                    {
                        Node<T> parent = queue.Peek();
                        parent.Children.Remove(current);
                        return;
                    }
                }

                queue.Enqueue(current);
                foreach (Node<T> child in current.Children)
                    queue.Enqueue(child);
            }
        }

        internal void Insert(int index, Node<T> item)
        {
            Children.Insert(index, item);
        }
        internal Node<T> this[int index]
        {
            get => GetElementAtIndex(index);
            set => Children[index] = value;
        }
        internal Node<T> GetElementAtIndex(int index)
        {
            Queue<Node<T>> queue = new Queue<Node<T>>();
            queue.Enqueue(this);
            int currentIndex = -1;

            while (queue.Count > 0)
            {
                currentIndex++;
                Node<T> current = queue.Dequeue();
                if (currentIndex == index)
                    return current;

                foreach (Node<T> child in current.Children)
                    queue.Enqueue(child);
            }

            return null;
        }
    }
}
