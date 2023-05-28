using PrimalEditor.Utilities;
using System.Threading.Tasks;
using System.Windows;

namespace PrimalEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App() => ShutdownMode = ShutdownMode.OnMainWindowClose;
        protected override void OnStartup(StartupEventArgs e)
        {
            SplashScreenView splashScreen = new SplashScreenView();
            splashScreen.Show();
            base.OnStartup(e);
            Task.Run(() =>
            {
                for (int i = 0; i <= 100; i++)
                {
                    // Use the Dispatcher to update the progress bar on the UI thread
                    splashScreen.Dispatcher.Invoke(() => splashScreen.UpdateProgress(i));
                    System.Threading.Thread.Sleep(1);
                }
                splashScreen.Dispatcher.Invoke(() =>
                {
                    splashScreen.Hide();
                    MainWindow mainWindow = new MainWindow();
                    Current.MainWindow = mainWindow;
                    mainWindow.Show();
                    mainWindow.Loaded += (s, args) => splashScreen.Close();
                });
            });
        }
    }
}
