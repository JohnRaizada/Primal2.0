using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// A view that displays the splash screen.
    /// </summary>
    public partial class SplashScreenView : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SplashScreenView"/> class.
        /// </summary>
        public SplashScreenView()
        {
            InitializeComponent();
            ForceCursor = true;
        }
        /// <summary>
        /// Updates the progress bar with the specified value.
        /// </summary>
        /// <param name="value">The value to update the progress bar with.</param>
        public void UpdateProgress(double value) => progressBar.Value = value;
    }
    /// <summary>
    /// Converts a progress value to an offset.
    /// </summary>
    public class ProgressToOffsetConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double)value;
            double threshold = System.Convert.ToDouble(parameter);
            return Math.Min(progress / threshold, 1.0);
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Converts a progress value to a text description.
    /// </summary>
    public class ProgressToTextConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double)value;
            if (progress >= 0 && progress < 40)
                return "Loading...";
            else if (progress >= 40 && progress < 80)
                return "Almost there...";
            else
                return "Expecting any moment...";
        }
        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}