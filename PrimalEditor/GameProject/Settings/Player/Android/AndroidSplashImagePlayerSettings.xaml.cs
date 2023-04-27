using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace PrimalEditor.GameProject.Settings.Player.Android
{
    /// <summary>
    /// Interaction logic for AndroidSplashImageSettings.xaml
    /// </summary>
    public partial class AndroidSplashImagePlayerSettings : UserControl
    {
        public AndroidSplashImagePlayerSettings()
        {
            InitializeComponent();
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            ForceCursor = true;
        }

        private void OnProjectSettings_Player_Icon_EditButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {

        }
        private void DropperButton_Click(object sender, RoutedEventArgs e)
        {

            // Change the cursor to a dropper
            Mouse.OverrideCursor = Cursors.Cross;

            // Capture the mouse to receive events even if the cursor leaves the window
            this.CaptureMouse();
        }
        private async void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.Captured == this)
            {
                // Release the mouse capture
                this.ReleaseMouseCapture();

                // Change the cursor back to default
                Mouse.OverrideCursor = null;

                // Get the position of the mouse click
                var position = e.GetPosition(this);

                // Convert the position to screen coordinates
                var point = PointToScreen(position);

                // Get the color of the pixel at the screen coordinates
                var color = await GetPixelColorAsync((int)point.X, (int)point.Y);

                // Update the color picker with the selected color
                var colorPicker = this.FindName("ColorPicker") as Xceed.Wpf.Toolkit.ColorPicker;
                if (colorPicker != null)
                {
                    var mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                    colorPicker.SelectedColor = mediaColor;
                }
            }
        }
        private async void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (Mouse.Captured == this)
            {
                // Get the position of the mouse
                var position = e.GetPosition(this);

                // Update the position of the TextBlock
                Canvas.SetLeft(ColorTextBlock, position.X + 10);
                Canvas.SetTop(ColorTextBlock, position.Y + 10);

                // Convert the position to screen coordinates
                var point = PointToScreen(position);

                // Get the color of the pixel at the screen coordinates
                var color = await GetPixelColorAsync((int)point.X, (int)point.Y);

                // Update the text of the TextBlocks
                RedTextBlock.Text = $"R: {color.R}";
                GreenTextBlock.Text = $"G: {color.G}";
                BlueTextBlock.Text = $"B: {color.B}";
                ColorTextBlock.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";

                // Update the fill of the ColorRectangle
                ColorRectangle.Fill = new SolidColorBrush(color);
                var redColor = new System.Windows.Media.Color();
                redColor.A = 255;
                redColor.B = 0;
                redColor.G = 0;
                redColor.R = color.R;
                RedRectangle.Fill = new SolidColorBrush(redColor);
                var greenColor = new System.Windows.Media.Color();
                greenColor.A = 255;
                greenColor.B = 0;
                greenColor.G = color.G;
                greenColor.R = 0;
                GreenRectangle.Fill = new SolidColorBrush(greenColor);
                var blueColor = new System.Windows.Media.Color();
                blueColor.A = 255;
                blueColor.B = color.B;
                blueColor.G = 0;
                blueColor.R = 0;
                BlueRectangle.Fill = new SolidColorBrush(blueColor);

                // Show the Grid
                ColorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                // Hide the Grid
                ColorGrid.Visibility = Visibility.Collapsed;
            }
        }
        private async Task<System.Windows.Media.Color> GetPixelColorAsync(int x, int y)
        {
            await Task.Delay(100);

            using (var bitmap = new Bitmap(1, 1))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    var dpi = VisualTreeHelper.GetDpi(this);
                    var scaledX = (int)(x * dpi.DpiScaleX);
                    var scaledY = (int)(y * dpi.DpiScaleY);

                    graphics.CopyFromScreen(scaledX, scaledY, 0, 0, new System.Drawing.Size(1, 1));
                }

                var pixel = bitmap.GetPixel(0, 0);

                return System.Windows.Media.Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
            }
        }

        private void OnProjectSettings_Player_Icon_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
