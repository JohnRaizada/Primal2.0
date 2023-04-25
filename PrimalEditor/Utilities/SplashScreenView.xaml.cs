using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for SplashScreenView.xaml
    /// </summary>
    public partial class SplashScreenView : Window
    {
        public SplashScreenView()
        {
            InitializeComponent();
            ForceCursor = true;
        }

        public void UpdateProgress(double value)
        {
            progressBar.Value = value;
        }
    }
    public class ProgressToOffsetConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double progress = (double)value;
            double threshold = System.Convert.ToDouble(parameter);
            return Math.Min(progress / threshold, 1.0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class ProgressToTextConverter : IValueConverter
    {
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}