using PrimalEditor.GameProject;
using PrimalEditor.Utilities.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PrimalEditor.Utilities
{
	/// <summary>
	/// Interaction logic for SDKManagerView.xaml
	/// </summary>
	public partial class SDKManagerView : Window
	{
		private bool _wasCheckboxCheckedManually = false;

		public SDKManagerView()
		{
			InitializeComponent();
			DataContext = Project.Current;
			ForceCursor = true;

			// Create a list of sample data
			var items = new List<ExpanderListMenuItem>
		{
			new ExpanderListMenuItem { Name = "Item 1", APILevel = 1 },
			new ExpanderListMenuItem { Name = "Item 2", APILevel = 2 },
			new ExpanderListMenuItem { Name = "Item 3", APILevel = 3 }
		};

			// Set the ItemsSource of the ListView to the list of sample data
			//myList.ItemsSource = items;
			var myItems = new List<ExpanderListMenuItem>
			{
			new ExpanderListMenuItem { Name = "Item2", APILevel = 5, Status = false, IsChecked = false },
			new ExpanderListMenuItem { Name = "Item3", APILevel = 6, Status = true, ListMenuItems = new List<ExpanderListMenuItem>{
				new ExpanderListMenuItem {Name = "ItemC", APILevel = 7, Status = true, IsChecked = true },
				new ExpanderListMenuItem {Name = "ItemD", APILevel = 8, Status = true, IsChecked = false}
			}, IsChecked = true },
			new ExpanderListMenuItem { Name = "Item5", APILevel = 9, Status = false, IsChecked = false },
			new ExpanderListMenuItem { Name = "Item6", APILevel = 19, Status = false, IsChecked = false },
			new ExpanderListMenuItem { Name = "Item7", APILevel = 29, Status = false, IsChecked = false },
			new ExpanderListMenuItem { Name = "Item8", APILevel = 39, Status = false, IsChecked = false },
			// Add more items here
		};

			// Set the ContentSource property
			myCustomElement.ContentSource = myItems;
		}

		private void OnSDKManager_Android_EditButton_Click(object sender, RoutedEventArgs e)
		{
			SDKPath.IsToggled = true;
		}

		private void OnSDKManager_DismissButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OnSDKManager_ActionButton_Click(object sender, RoutedEventArgs e)
		{

		}
		private void ShowPackageDetails_Checked(object sender, RoutedEventArgs e)
		{
			//// Expand all items when the CheckBox is checked
			//foreach (var item in myList.Items)
			//{
			//    var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//    var expander = FindVisualChild<Expander>(container);
			//    if (expander == null) continue;
			//    expander.IsExpanded = true;

		}

		private void ShowPackageDetails_Unchecked(object sender, RoutedEventArgs e)
		{
			//// Collapse all items when the CheckBox is unchecked
			//foreach (var item in myList.Items)
			//{
			//    var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//    var expander = FindVisualChild<Expander>(container);
			//    if (expander == null) continue;
			//    if (!_wasCheckboxCheckedManually) return;
			//    expander.IsExpanded = false;
			//}
		}

		private void Expander_Expanded(object sender, RoutedEventArgs e)
		{
			//// Check if all items are expanded
			//bool allExpanded = true;
			//foreach (var item in myList.Items)
			//{
			//    var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//    var expander = FindVisualChild<Expander>(container);
			//    if (expander != null && !expander.IsExpanded)
			//    {
			//        allExpanded = false;
			//        break;
			//    }
			//}
			//// Check the CheckBox if all items are expanded
			//showPackageDetails.IsChecked = allExpanded;
		}

		private void Expander_Collapsed(object sender, RoutedEventArgs e)
		{
			//// Uncheck the CheckBox when an item is collapsed
			//showPackageDetails.IsChecked = false;
		}

		private T FindVisualChild<T>(DependencyObject obj) where T : DependencyObject
		{
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
			{
				var child = VisualTreeHelper.GetChild(obj, i);
				if (child is T visualChild)
				{
					return visualChild;
				}
				else
				{
					var descendant = FindVisualChild<T>(child);
					if (descendant != null)
					{
						return descendant;
					}
				}
			}
			return null;
		}

		private void ShowPackageDetails_Clicked(object sender, RoutedEventArgs e)
		{
			//_wasCheckboxCheckedManually = true;
			//if ((bool)showPackageDetails.IsChecked)
			//{
			//    // Expand all items when the CheckBox is checked
			//    foreach (var item in myList.Items)
			//    {
			//        var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//        var expander = FindVisualChild<Expander>(container);
			//        if (expander == null) continue;
			//        expander.IsExpanded = true;
			//    }
			//    return;
			//}
			//// Collapse all items when the CheckBox is unchecked
			//foreach (var item in myList.Items)
			//{
			//    var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//    var expander = FindVisualChild<Expander>(container);
			//    if (expander == null) continue;
			//    if (!_wasCheckboxCheckedManually) return;
			//    expander.IsExpanded = false;
			//}
		}

		private void Expander_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			_wasCheckboxCheckedManually = false;
			// Uncheck the CheckBox when an item is collapsed
			showPackageDetails.IsChecked = false;
		}

		private void GridSplitter_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
		{

			//        var newWidth = ColumnHeaders.ColumnDefinitions[0].ActualWidth + e.HorizontalChange;
			//        if (newWidth <= ColumnHeaders.ColumnDefinitions[0].MinWidth) return;
			//        /*int i = 0;
			//        foreach (var item in ColumnHeaders.Children)
			//        {
			//            if (!(item is GridSplitter)) continue;
			//            var container = item as GridSplitter;
			//            container.DragDelta += (sender, e) => GridSplitter_DragDelta(sender, e, i++);
			//        }*/
			//        ColumnHeaders.ColumnDefinitions[0].Width = new GridLength(newWidth);
			//        foreach (var item in myList.Items)
			//        {
			//            var container = myList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
			//            if (container == null) return;
			//            var expander = FindVisualChild<Expander>(container);
			//            if (expander == null) return;
			//            // Access the Expander and its child elements here
			//            /*var headerTextBlock = expander.Header as TextBlock;
			//if (headerTextBlock == null) return;
			//	var text = headerTextBlock.Text;
			//	// Do something with the text
			//	*/
			//            var grid = expander.Content as Grid;
			//            if (grid == null) return;
			//            grid.ColumnDefinitions[0].Width = new GridLength(newWidth);
			//            grid.ColumnDefinitions[1].Width = new GridLength(newWidth);
			//            grid.ColumnDefinitions[2].Width = new GridLength(newWidth);
			//            grid.ColumnDefinitions[3].Width = new GridLength(newWidth);
			//        }
		}

		private void RadioButton_Checked(object sender, RoutedEventArgs e)
		{
			var radioButton = sender as RadioButton;
			if (radioButton == null) return;
			if (myCustomElement == null) return;
			if (radioButton == listViewRadioButton)
				myCustomElement.ContentViewMode = ContentViewMode.List;
			else
				myCustomElement.ContentViewMode = ContentViewMode.Expander;
		}

		private void RadioButton_UnChecked(object sender, RoutedEventArgs e)
		{

			var radioButton = sender as RadioButton;
			if (radioButton == null) return;
			if (myCustomElement == null) return;

			if (radioButton == listViewRadioButton)
				myCustomElement.ContentViewMode = ContentViewMode.Expander;
			else
				myCustomElement.ContentViewMode = ContentViewMode.List;
		}
	}
	public class AndroidSdkDetails
	{
		public ObservableCollection<Platform> Platforms { get; set; }
		public ObservableCollection<Tool> Tools { get; set; }
		public ObservableCollection<UpdateSite> UpdateSites { get; set; }
	}

	public class Platform
	{
		public string Name { get; set; }
		public string APILevel { get; set; }
		public string Revision { get; set; }
		public string Status { get; set; }
	}

	public class Tool
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public string Status { get; set; }
	}

	public class UpdateSite
	{
		public string Name { get; set; }
		public string URL { get; set; }
	}
}
