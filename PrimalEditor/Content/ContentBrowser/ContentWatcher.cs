using PrimalEditor.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace PrimalEditor.Content
{
    /// <summary>
    /// An event arguments class that is used to indicate that a content file has been modified.
    /// </summary>
    public class ContentModifiedEventArgs : EventArgs
    {
        /// <summary>
        /// The path of the modified content file.
        /// </summary>
        public string FullPath { get; }
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentModifiedEventArgs"/> class.
        /// </summary>
        /// <param name="path">The path of the modified content file.</param>
        public ContentModifiedEventArgs(string path) => FullPath = path;
	}
	static class ContentWatcher
	{
		private static readonly DelayEventTimer _refreshTimer = new(TimeSpan.FromMilliseconds(250));
		/// <summary>
		/// The content watcher.
		/// </summary>
		private static readonly FileSystemWatcher _contentWatcher = new()
		{
			IncludeSubdirectories = true,
			Filter = "",
			NotifyFilter = NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastWrite
		};
		// File watcher is only enabled when this counter is 0.
		private static int _fileWatcherEnableCounter = 0;
		/// <summary>
		/// The event that is raised when content is modified.
		/// </summary>
		public static event EventHandler<ContentModifiedEventArgs>? ContentModified;
		/// <summary>
		/// Enables the file watcher.
		/// </summary>
		/// <param name="isEnabled">Whether the file watcher should be enabled.</param>
		public static void EnableFileWatcher(bool isEnabled)
		{
			if (_fileWatcherEnableCounter > 0 && isEnabled) --_fileWatcherEnableCounter;
			else if (!isEnabled) ++_fileWatcherEnableCounter;
		}
		/// <summary>
		/// Resets the content watcher.
		/// </summary>
		/// <param name="contentFolder">The content folder.</param>
		/// <param name="projectPath">The project path.</param>
		public static void Reset(string contentFolder, string projectPath)
		{
			_contentWatcher.EnableRaisingEvents = false;
			ContentInfoCache.Reset(projectPath);
			if (string.IsNullOrEmpty(contentFolder)) return;
			Debug.Assert(Directory.Exists(contentFolder));
			_contentWatcher.Path = contentFolder;
			_contentWatcher.EnableRaisingEvents = true;
			AssetRegistry.Reset(contentFolder);
		}
		/// <summary>
		/// Handles the <see cref="FileSystemEventArgs"/> event.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <see cref="FileSystemEventArgs"/> event arguments.</param>
		private static async void OnContentModified(object sender, FileSystemEventArgs e) => await Application.Current.Dispatcher.BeginInvoke(new Action(() => _refreshTimer.Trigger(e)));
		/// <summary>
		/// Refreshes the content watcher.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">The <see cref="DelayEventTimerArgs"/> event arguments.</param>
		private static void Refresh(object? sender, DelayEventTimerArgs e)
		{
			if (_fileWatcherEnableCounter > 0)
			{
				e.RepeatEvent = true;
				return;
			}
			e.Data.Cast<FileSystemEventArgs>().GroupBy(x => x.FullPath).Select(x => x.First()).ToList().ForEach(x => ContentModified?.Invoke(null, new ContentModifiedEventArgs(x.FullPath)));
		}
		/// <summary>
		/// Initializes a new instance of the <see cref="ContentWatcher"/> class.
		/// </summary>
		static ContentWatcher()
		{
			_contentWatcher.Changed += OnContentModified;
			_contentWatcher.Created += OnContentModified;
			_contentWatcher.Deleted += OnContentModified;
			_contentWatcher.Renamed += OnContentModified;
			_refreshTimer.Triggered += Refresh;
		}
	}
}