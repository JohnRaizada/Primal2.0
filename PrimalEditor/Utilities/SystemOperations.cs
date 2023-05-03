using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
namespace PrimalEditor.Utilities
{
    class ClipboardContent
    {
        internal string Path { get; }
        internal ClipboardContent(string path)
        {
            Path = path;
        }
    }
    internal class SystemOperations
    {
        private static readonly ObservableCollection<ClipboardContent> _clipboard = new ObservableCollection<ClipboardContent>();
        public static ReadOnlyObservableCollection<ClipboardContent> Clipboard
        {
            get;
        } = new ReadOnlyObservableCollection<ClipboardContent>(_clipboard);
        private static bool _wasPreviousCommandCut = false;
        private static int _numberOfOperationsLeft = 0;
        private static async void LocalCopy(string path)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _clipboard.Add(new ClipboardContent(path));
            }));
        }
        internal static async void LocalCopy(List<string> paths)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _clipboard.Clear();
                foreach (string path in paths)
                {
                    LocalCopy(path);
                }
            }));
            _wasPreviousCommandCut = false;
        }
        internal static async void CopyToClipboard(string text)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Windows.Clipboard.SetText(text);
            }));
        }
        internal static async void CopyToClipboard(BitmapImage image)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Windows.Clipboard.SetImage(image);
            }));
        }
        internal static async void CopyToClipboard(byte[] audio)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Windows.Clipboard.SetAudio(audio);
            }));
        }
        private static void LocalCut(string path)
        {
            LocalCopy(path);
        }
        internal static void LocalCut(List<string> paths)
        {
            _clipboard.Clear();
            foreach (string path in paths)
            {
                LocalCut(path);
            }
            _wasPreviousCommandCut = true;
        }
        internal static void CutToClipboard(string text)
        {
            CopyToClipboard(text);
        }
        internal static void CutToClipboard(BitmapImage image)
        {
            CopyToClipboard(image);
        }
        internal static void CutToClipboard(byte[] audio)
        {
            CopyToClipboard(audio);
        }
        private static async void Delete(string path, bool permanently = false)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (Directory.Exists(path))
                    {
                        FileSystem.DeleteDirectory(path, UIOption.OnlyErrorDialogs, permanently ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin);
                        return;
                    }
                    FileSystem.DeleteFile(path, UIOption.OnlyErrorDialogs, permanently ? RecycleOption.DeletePermanently : RecycleOption.SendToRecycleBin);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Logger.Log(MessageType.Error, $"Failed to delete {path}");
                }
            }));
        }
        internal static void Delete(List<string> paths, bool permanently = false)
        {
            foreach (string path in paths)
            {
                if (permanently)
                {
                    switch (MessageBox.Show("Are you sure you want to permanently delete this file?", "Delete file", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.None, MessageBoxOptions.ServiceNotification))
                    {
                        case MessageBoxResult.None:
                            return;
                        case MessageBoxResult.Cancel:
                            return;
                        case MessageBoxResult.No:
                            return;
                        default:
                            break;
                    }
                }
                Delete(path, permanently);
            }
        }
        private static async void Paste(string sourcePath, string destinationPath)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (File.Exists(sourcePath))
                {
                    // The source path is a file
                    string fileName = Path.GetFileName(sourcePath);
                    try
                    {
                        File.Copy(sourcePath, Path.Combine(destinationPath, fileName));
                        OnPasteSuccessful(sourcePath);
                    }
                    catch (IOException)
                    {
                        try
                        {
                            File.Copy(sourcePath, Path.Combine(destinationPath, fileName), MessageBox.Show("File already exists. Overwrite?", "Overwrite file", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                            OnPasteSuccessful(sourcePath);
                        }
                        catch (Exception exception)
                        {
                            Debug.WriteLine(exception.Message);
                            Logger.Log(MessageType.Error, $"Failed to paste {sourcePath} to {destinationPath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        Logger.Log(MessageType.Error, $"Failed to paste {sourcePath} to {destinationPath}");
                    }
                }
                else if (Directory.Exists(sourcePath))
                {
                    // The source path is a directory
                    PasteFolder(sourcePath, destinationPath);
                }
            }));
        }

        private static void OnPasteSuccessful(string path)
        {
            if (_wasPreviousCommandCut)
            {
                Delete(path);
                _numberOfOperationsLeft--;
            }
            if (_numberOfOperationsLeft == 0) _wasPreviousCommandCut = false;
        }

        private static void PasteFolder(string sourcePath, string destinationPath)
        {
            // Create the destination directory if it doesn't exist
            destinationPath += Path.DirectorySeparatorChar + Path.GetFileName(sourcePath.TrimEnd(Path.DirectorySeparatorChar));
            Directory.CreateDirectory(destinationPath);
            // Copy all files
            foreach (string file in Directory.GetFiles(sourcePath))
            {
                string fileName = Path.GetFileName(file);
                try
                {
                    File.Copy(file, Path.Combine(destinationPath, fileName));
                    OnPasteSuccessful(sourcePath);
                }
                catch (IOException)
                {
                    try
                    {
                        File.Copy(file, Path.Combine(destinationPath, fileName), MessageBox.Show("File already exists. Overwrite?", "Overwrite file", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes);
                        OnPasteSuccessful(sourcePath);
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(exception.Message);
                        Logger.Log(MessageType.Error, $"Failed to paste {sourcePath} to {destinationPath}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Logger.Log(MessageType.Error, $"Failed to paste {sourcePath} to {destinationPath}");
                }
            }

            // Recursively copy all subdirectories
            foreach (string directory in Directory.GetDirectories(sourcePath))
            {
                string directoryName = Path.GetFileName(directory);
                PasteFolder(directory, Path.Combine(destinationPath, directoryName));
            }
        }
        public static void Paste(string destinationFolderPath)
        {
            foreach (ClipboardContent clipboardContent in Clipboard)
            {
                _numberOfOperationsLeft++;
                Paste(clipboardContent.Path, destinationFolderPath);
            }
        }
        internal static async void ClearLocalClipboard()
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                _clipboard.Clear();
            }));
            _wasPreviousCommandCut = false;
        }
        internal static async void ClearSystemClipboard()
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                System.Windows.Clipboard.Clear();
            }));
        }

        internal static async void Rename(string name, List<string> paths)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (string path in paths)
                {
                    Rename(name, path);
                }
            }));
        }
        internal static async void Rename(string name, string path)
        {
            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    string? directoryName = Path.GetDirectoryName(path);
                    if (directoryName == null) return;
                    if (File.Exists(path))
                    {
                        string newPath = Path.Combine(directoryName, name + Path.GetExtension(path));
                        File.Move(path, newPath);
                    }
                    else if (Directory.Exists(path))
                    {
                        string newPath = Path.Combine(directoryName, name);
                        Directory.Move(path, newPath);
                    }
                    else
                    {
                        Debug.WriteLine("File or directory not found.");
                        Logger.Log(MessageType.Error, $"File or directory not found.");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Logger.Log(MessageType.Error, $"Failed to rename {path} to {name}");
                }
            }));
        }

        internal static void CreateNewFolder(string path)
        {
            string name = "New Folder";
            string folderPath = Path.Combine(path, name);
            int count = 1;
            while (Directory.Exists(folderPath))
            {
                name = $"New Folder ({count})";
                folderPath = Path.Combine(path, name);
                count++;
            }
            Directory.CreateDirectory(folderPath);
        }
        internal static void CreateNewFolderWithName(string path)
        {
            if (Directory.Exists(path)) return;
            Directory.CreateDirectory(path);
        }
    }
}
