using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.GameProject.Settings.Player
{
    /// <summary>
    /// Interaction logic for AndroidPlayerSettings.xaml
    /// </summary>
    public partial class AndroidPlayerSettingsView : UserControl
    {
        public AndroidPlayerSettingsView()
        {
            InitializeComponent();
            ForceCursor = true;
        }

        private void OnProjectSettings_Player_Icon_MouseUp(object sender, MouseButtonEventArgs e)
        {

        }

        private void OnProjectSettings_Player_Icon_EditButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void listBox_Drop(object sender, DragEventArgs e)
        {/*
            YourDataType droppedData = e.AndroidPackage.GetData(typeof(YourDataType)) as YourDataType;
            YourDataType target = ((ListBoxItem)(sender)).DataContext as YourDataType;

            int removedIdx = listBox.Items.IndexOf(droppedData);
            int targetIdx = listBox.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                YourItemsSource.Insert(targetIdx + 1, droppedData);
                YourItemsSource.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (YourItemsSource.Count + 1 > remIdx)
                {
                    YourItemsSource.Insert(targetIdx, droppedData);
                    YourItemsSource.RemoveAt(remIdx);
                }
            }*/
        }

    }
}
