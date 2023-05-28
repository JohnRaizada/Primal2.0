using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PrimalEditor.Content
{
	/// <summary>
	/// Interaction logic for SaveDialog.xaml
	/// </summary>
	public partial class SaveDialog : Window
    {
        /// <summary>
        /// The path of the asset to save.
        /// </summary>
		public string? SaveFilePath { get; private set; }
        /// <summary>
        /// The path of the asset to save.
        /// </summary>
		public SaveDialog()
		{
			InitializeComponent();
			Closing += OnSaveDialogClosing;
			ForceCursor = true;
        }
        /// <summary>
        /// Validates the file name and path.
        /// </summary>
        /// <param name="saveFilePath">The path of the asset to save.</param>
        /// <returns>
        /// <c>true</c> if the file name and path are valid; otherwise, <c>false</c>.
        /// </returns>
		private bool ValidateFileName(out string saveFilePath)
        {
            // Get the content browser.
            var contentBrowser = contentBrowserView.DataContext as ContentBrowser;
            // Get the path of the selected folder.
            var path = contentBrowser?.SelectedFolder;
            // If the path is null, return false.
            if (path == null)
			{
				saveFilePath = string.Empty;
				return false;
            }
            // If the path does not end in a directory separator, add one.
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            // Get the file name.
            var fileName = fileNameTextBox.Text.Trim();
            // If the file name is empty, return false.
            if (string.IsNullOrEmpty(fileName))
			{
				saveFilePath = string.Empty;
				return false;
            }
            // If the file name does not end in the asset file extension, add it.
            if (!fileName.EndsWith(Asset.AssetFileExtension)) fileName += Asset.AssetFileExtension;
            // Combine the path and file name.
            path += $@"{fileName}";
            // Initialize the validity flag to false.
            var isValid = false;
            // Get the error message.
            string errorMsg = string.Empty;
            // If the file name contains invalid characters, set the error message and return false.
            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) != -1) errorMsg = "Invalid character(s) used in asset file name.";
            // If the file already exists, show a confirmation dialog.
            else if (File.Exists(path) && MessageBox.Show("File already exists. Overwrite?", "Overwrite file", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
			{
                /*Do nothing. Just return false.*/
            }
            // If the file name is valid, set the validity flag to true.
            else isValid = true;
            // If the error message is not empty, show a message box.
            if (!string.IsNullOrEmpty(errorMsg)) MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            // Set the path of the asset to save.
            saveFilePath = path;
            // Return the validity flag.
            return isValid;
        }
        /// <summary>
        /// Handles the Click event of the Save button.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args.</param>
		private void OnSave_Button_Click(object sender, RoutedEventArgs? e)
        {
            // Validate the file name and path.
            if (!ValidateFileName(out var saveFilePath)) return;
            // Set the SaveFilePath property.
            SaveFilePath = saveFilePath;
            // Set the DialogResult property to true.
            DialogResult = true;
            // Close the dialog box.
            Close();
		}
		private void OnContentBrowser_Mouse_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (((FrameworkElement)e.OriginalSource).DataContext == contentBrowserView.SelectedItems.FirstOrDefault() && contentBrowserView.SelectedItems.FirstOrDefault()?.FileName == fileNameTextBox.Text) OnSave_Button_Click(sender, null);
		}
		private void OnSaveDialogClosing(object? sender, CancelEventArgs e) => contentBrowserView.Dispose();
	}
}
