using PrimalEditor.Utilities.Controls;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.AvalonDock.Controls;

namespace PrimalEditor.Utilities
{
	/// <summary>
	/// Interaction logic for DeleteThenDownload.xaml
	/// </summary>
	public partial class SDKDownloadsView : Window
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SDKDownloadsView"/> class.
		/// </summary>
		public SDKDownloadsView()
		{
			InitializeComponent();
			Loaded += (sender, e) => Downloads.Instance.DeleteThenDownload(commandOutputRelay);
			DataContext = Downloads.Instance;
		}
		private void OnSDKDownloads_BackgroundDoneButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is not Button button) return;
			switch (button.Content)
			{
				case "Background":
					Hide();
					break;
				case "Done":
					Close();
					break;
				default:
					break;
			}
		}
		private void OnSDKDownloads_CancelButton_Click(object sender, RoutedEventArgs e)
		{
			CommandHelper.TryStopCurrentCommandAndDisableFurtherProcessing();
			Close();
		}
		private void OnSDKDownloads_VisibilityRadioButton_LeftChecked(object sender, RoutedEventArgs e)
		{
			commandOutputRelay.Visibility = Visibility.Collapsed;
			downloadProgressBox.Visibility = Visibility.Visible;
			Grid.SetColumnSpan(downloadProgressBox, 2);
			Grid.SetColumn(downloadProgressBox, 0);
		}
		private void OnSDKDownloads_VisibilityRadioButton_CenterChecked(object sender, RoutedEventArgs e)
		{
			commandOutputRelay.Visibility = Visibility.Visible;
			downloadProgressBox.Visibility = Visibility.Visible;
			Grid.SetColumnSpan(downloadProgressBox, 1);
			Grid.SetColumn(downloadProgressBox, 0);
			Grid.SetColumnSpan(commandOutputRelay, 1);
			Grid.SetColumn(commandOutputRelay, 1);
		}
		private void OnSDKDownloads_VisibilityRadioButton_RightChecked(object sender, RoutedEventArgs e)
		{
			commandOutputRelay.Visibility = Visibility.Visible;
			downloadProgressBox.Visibility = Visibility.Collapsed;
			Grid.SetColumnSpan(commandOutputRelay, 2);
			Grid.SetColumn(commandOutputRelay, 0);
		}
		private void OnSDKDownloads_ProgressBar_Value_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			if (sender is not ProgressBar progressBar) return;
			var progressBars = progressBar.Parent.FindVisualParent<ListBox>().FindVisualChildren<ProgressBar>();
			double totalProgress = 0;
			foreach (var item in progressBars) totalProgress += item.Value;
			Downloads.Instance.AverageProgress = (totalProgress / progressBars.Count()).ToString();
		}
		private void OnSDKDownloads_CommandOutputRelay_ProgressChanged(object sender, System.EventArgs e)
		{
			if (sender is not CommandOutputRelay commandOutputRelay) return;
			foreach (var item in Downloads.Instance.DownloadItems)
			{
				if (item.URL != commandOutputRelay.CurrentPath) continue;
				item.Progress = commandOutputRelay.Progress;
			}
		}
	}
}
