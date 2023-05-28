using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.Editors
{
	/// <summary>
	/// Converts bool from null type to not null type.
	/// </summary>
	/// <remarks>Returns true only when the value is true else false always</remarks>
	public class NullableBoolToBoolConverter : IValueConverter
	{
		/// <inheritdoc/>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && b == true;
		/// <inheritdoc/>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value is bool b && b == true;
	}
	/// <summary>
	/// Interaction logic for GameEntityView.xaml
	/// </summary>
	public partial class GameEntityView : UserControl
    {
        /// <summary>
        /// The undo action for the current property name.
        /// </summary>
        private Action? _undoAction;
        /// <summary>
        /// The current property name.
        /// </summary>
        private string? _propertyName;
        /// <summary>
        /// The instance of this view.
        /// </summary>
        public static GameEntityView? Instance { get; private set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="GameEntityView"/> class.
        /// </summary>
        public GameEntityView()
		{
			InitializeComponent();
			DataContext = null;
			Instance = this;
			DataContextChanged += (_, __) =>
			{
				if (DataContext != null) ((MSEntity)DataContext).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
			};
        }
        /// <summary>
        /// Gets an action that will rename the current game entity.
        /// </summary>
        private Action GetRenameAction()
		{
			var vm = (MSEntity)DataContext;
			var selection = vm.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();
			return new Action(() =>
			{
				selection.ForEach(item => item.entity.Name = item.Name);
				((MSEntity)DataContext).Refresh();
			});
        }
        /// <summary>
        /// Gets an action that will toggle the enabled state of the current game entity.
        /// </summary>
        private Action GetIsEnabledAction()
		{
			var vm = (MSEntity)DataContext;
			var selection = vm.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
			return new Action(() =>
			{
				selection.ForEach(item => item.entity.IsEnabled = item.IsEnabled);
				((MSEntity)DataContext).Refresh();
			});
        }
        /// <summary>
        /// Handles the <see cref="OnName_TextBox_GotKeyboardFocus"/> event.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The <see cref="KeyboardFocusChangedEventArgs"/> event arguments.</param>
        private void OnName_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			_propertyName = string.Empty;
			_undoAction = GetRenameAction();
		}
		private void OnName_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
		{
			if (_propertyName == nameof(MSEntity.Name) && _undoAction != null)
			{
				var redoAction = GetRenameAction();
				Project.UndoRedo.Add(new UndoRedoAction(_undoAction, redoAction, "Rename game entity"));
				_propertyName = null;
			}
			_undoAction = null;
		}
		private void OnIsEnabled_CheckBox_Click(object sender, RoutedEventArgs e)
		{
			var undoAction = GetIsEnabledAction();
			var vm = (MSEntity)DataContext;
			vm.IsEnabled = ((CheckBox)sender).IsChecked == true;
			var redoAction = GetIsEnabledAction();
			Project.UndoRedo.Add(new UndoRedoAction(undoAction, redoAction, vm.IsEnabled == true ? "Enable game entity" : "Disable game entity"));
		}
		private void OnAddComponent_Button_PreviewMouse_LBD(object sender, MouseButtonEventArgs e)
		{
			var menu = (ContextMenu)FindResource("addComponentMenu");
			var btn = (ToggleButton)sender;
			btn.IsChecked = true;
			menu.Placement = PlacementMode.Bottom;
			menu.PlacementTarget = btn;
			menu.MinWidth = btn.ActualWidth;
			menu.IsOpen = true;
		}
		private void AddComponent(ComponentType componentType, object? data)
		{
			var creationFunction = ComponentFactory.GetCreationFunction(componentType);
			var changedEntities = new List<(GameEntity entity, Component component)>();
			var vm = (MSEntity)DataContext;
			foreach (var entity in vm.SelectedEntities)
			{
				if (data == null) continue;
				var component = creationFunction(entity, data);
				if (entity.AddComponent(component)) changedEntities.Add((entity, component));
			}
			if (changedEntities.Any())
			{
				vm.Refresh();
				Project.UndoRedo.Add(new UndoRedoAction(
					() =>
					{
						changedEntities.ForEach(x => x.entity.RemoveComponent(x.component));
						((MSEntity)DataContext).Refresh();
					},
					() =>
					{
						changedEntities.ForEach(x => x.entity.AddComponent(x.component));
						((MSEntity)DataContext).Refresh();
					},
					$"Add {componentType} component"
					)
				);
			}
		}
		private void OnAddScriptComponent(object sender, RoutedEventArgs e) => AddComponent(ComponentType.Script, ((MenuItem)sender).Header.ToString());
	}
}
