using System.IO;
using System.Windows;

namespace PrimalEditor
{
    /// <summary>
    /// Interaction logic for EnginePathDialog.xaml
    /// </summary>
    public partial class EnginePathDialog : Window
    {
        /// <summary>
        /// The path to the Primal engine.
        /// </summary>
		public string? PrimalPath { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="EnginePathDialog"/> class.
        /// </summary>
		public EnginePathDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
            ForceCursor = true;
        }
        /// <summary>
        /// Handles the Click event for the Ok button.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event arguments.</param>
		private void OnOk_Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the path from the text box.
            var path = pathTextBox.Text.Trim();
            // Clear the message text block.
            messageTextBlock.Text = string.Empty;
            // Validate the path.
            if (string.IsNullOrEmpty(path)) messageTextBlock.Text = "Invalid path";
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1) messageTextBlock.Text = "Invalid character(s) used in path";
            else if (!Directory.Exists(Path.Combine(path, @"Engine\EngineAPI\"))) messageTextBlock.Text = "Unable to find the engine at the specified location";
            // If the path is invalid, return.
            if (!string.IsNullOrEmpty(messageTextBlock.Text)) return;
            // If the path does not end with a directory separator, append one.
            if (!Path.EndsInDirectorySeparator(path)) path += @"\";
            // Set the PrimalPath property.
            PrimalPath = path;
            // Set the DialogResult property to true.
            DialogResult = true;
            // Close the dialog box.
            Close();
        }
    }
}
