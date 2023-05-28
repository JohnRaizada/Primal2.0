using PrimalEditor.Content;
using PrimalEditor.GameProject;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace PrimalEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Gets or sets the path to the Primal Editor installation.
        /// </summary>
        public static string? PrimalPath { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Loaded += OnMainWindowLoaded;
                Closing += OnMainWindowClosing;
                ForceCursor = true;
            }
            catch (Exception ex)
            {
                // Log any exceptions that are thrown
                Debug.WriteLine(ex.ToString());
            }
        }
        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            GetEnginePath();
            OpenProjectBrowserDialog();
        }
        private void GetEnginePath()
        {
            var primalPath = Environment.GetEnvironmentVariable("PRIMAL_ENGINE", EnvironmentVariableTarget.User);
            if (primalPath == null || !Directory.Exists(Path.Combine(primalPath, @"Engine\EngineAPI")))
            {
                var dlg = new EnginePathDialog();
                if (dlg.ShowDialog() == true)
                {
                    PrimalPath = dlg.PrimalPath;
                    Environment.SetEnvironmentVariable("PRIMAL_ENGINE", PrimalPath?.ToUpper(), EnvironmentVariableTarget.User);
                    return;
                }
                Application.Current.Shutdown();
                return;
            }
            PrimalPath = primalPath;
        }
        private void OnMainWindowClosing(object? sender, CancelEventArgs e)
        {
            if (DataContext == null)
            {
                e.Cancel = true;
                Application.Current.MainWindow.Hide();
                OpenProjectBrowserDialog();
                if (DataContext != null) Application.Current.MainWindow.Show();
                return;
            }
            Closing -= OnMainWindowClosing;
            Project.Current?.Unload();
            DataContext = null;
        }
        private void OpenProjectBrowserDialog()
        {
            var projectBrowser = new ProjectBrowserDialog();
            if (projectBrowser.ShowDialog() == false || projectBrowser.DataContext == null)
            {
                Application.Current.Shutdown();
                return;
            }
            Project.Current?.Unload();
            var project = projectBrowser.DataContext as Project;
            Debug.Assert(project != null);
            ContentWatcher.Reset(project.ContentPath, project.Path);
            DataContext = project;
        }
    }
}
