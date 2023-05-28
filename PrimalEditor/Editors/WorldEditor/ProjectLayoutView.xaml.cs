using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for ProjectLayoutView.xaml
    /// </summary>
    public partial class ProjectLayoutView : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectLayoutView"/> class.
        /// </summary>
		public ProjectLayoutView() => InitializeComponent();
        /// <summary>
        /// Handles the Click event of the AddGameEntityButton control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args.</param>
		private void OnAddGameEntity_Button_Click(object sender, RoutedEventArgs e)
        {
            // Get the button that was clicked.
            var btn = (Button)sender;
            // Get the Scene that is the data context of the button.
            var vm = (Scene)btn.DataContext;
            // Execute the AddGameEntityCommand on the Scene.
            vm.AddGameEntityCommand?.Execute(new GameEntity(vm) { Name = "Empty Game Entity" });
            // If the current project is not null, set its IsModified property to true.
            if (Project.Current != null) Project.Current.IsModified = true;
        }
        /// <summary>
        /// Handles the SelectionChanged event of the GameEntities_ListBox control.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event args.</param>
		private void OnGameEntities_ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the ListBox that raised the event.
            var listBox = (ListBox)sender;
            // Get the new selection of GameEntities from the ListBox.
            var newSelection = listBox.SelectedItems.Cast<GameEntity>().ToList();
            // Get the previous selection of GameEntities from the ListBox.
            var previousSelection = newSelection.Except(e.AddedItems.Cast<GameEntity>()).Concat(e.RemovedItems.Cast<GameEntity>()).ToList();
            // Add an UndoRedoAction to the Project's UndoRedo collection.
            Project.UndoRedo.Add(new UndoRedoAction(
                () => // undo action
                {
                    // Unselect all items in the ListBox.
                    listBox.UnselectAll();
                    // Select all items in the previous selection.
                    previousSelection.ForEach(x => ((ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(x)).IsSelected = true);
                },
                () => //redo action
                {
                    // Unselect all items in the ListBox.
                    listBox.UnselectAll();
                    // Select all items in the new selection.
                    newSelection.ForEach(x => ((ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(x)).IsSelected = true);
                },
                "Selection changed"
            ));
            // Create a new MSGameEntity object from the new selection of GameEntities.
            MSGameEntity? msEntity = null;
            if (newSelection.Any()) msEntity = new MSGameEntity(newSelection);
            // If the GameEntityView instance is not null, set its DataContext to the MSGameEntity object.
            if (GameEntityView.Instance != null) GameEntityView.Instance.DataContext = msEntity;
        }
    }
}
