using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Represents an undo/redo action.
    /// </summary>
    public interface IUndoRedo
    {
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <returns>
        /// The name of the action.
        /// </returns>
        string Name { get; }
        /// <summary>
        /// Performs the undo operation.
        /// </summary>
        void Undo();
        /// <summary>
        /// Performs the redo operation.
        /// </summary>
        void Redo();
    }
    /// <summary>
    /// Represents an undo/redo action that can be undone and redone.
    /// </summary>
    public class UndoRedoAction : IUndoRedo
    {
        /// <summary>
        /// The action to perform when undoing.
        /// </summary>
        private readonly Action _undoAction;
        /// <summary>
        /// The action to perform when redoing.
        /// </summary>
        private readonly Action _redoAction;
        /// <summary>
        /// Gets the name of the action.
        /// </summary>
        /// <returns>
        /// The name of the action.
        /// </returns>
        public string Name { get; }
        /// <summary>
        /// Performs the undo operation.
        /// </summary>
        public void Undo() => _undoAction();
        /// <summary>
        /// Performs the redo operation.
        /// </summary>
        public void Redo() => _redoAction();
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedoAction"/> class.
        /// </summary>
        /// <param name="name">The name of the action.</param>
        public UndoRedoAction(string name) => Name = name;
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedoAction"/> class.
        /// </summary>
        /// <param name="undo">The action to perform when undoing.</param>
        /// <param name="redo">The action to perform when redoing.</param>
        /// <param name="name">The name of the action.</param>
        public UndoRedoAction(Action undo, Action redo, string name) : this(name)
        {
            Debug.Assert(undo != null && redo != null);
            _undoAction = undo;
            _redoAction = redo;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedoAction"/> class.
        /// </summary>
        /// <param name="property">The name of the property to modify.</param>
        /// <param name="instance">The object to modify.</param>
        /// <param name="undoValue">The value of the property to set when undoing.</param>
        /// <param name="redoValue">The value of the property to set when redoing.</param>
        /// <param name="name">The name of the action.</param>
        public UndoRedoAction(string property, object instance, object undoValue, object redoValue, string name) : this(
            () => instance.GetType().GetProperty(property)?.SetValue(instance, undoValue),
            () => instance.GetType().GetProperty(property)?.SetValue(instance, redoValue),
            name
        )
        { }
    }
    /// <summary>
    /// Represents an undo/redo manager.
    /// </summary>
    public class UndoRedo
    {
        private bool _enableAdd = true;
        private readonly ObservableCollection<IUndoRedo> _redoList = new();
        private readonly ObservableCollection<IUndoRedo> _undoList = new();
        /// <summary>
        /// Gets the list of undoable actions that can be redone.
        /// </summary>
        /// <returns>
        /// A read-only collection of undoable actions.
        /// </returns>
        public ReadOnlyObservableCollection<IUndoRedo> RedoList { get; }
        /// <summary>
        /// Gets the list of undoable actions that can be undone.
        /// </summary>
        /// <returns>
        /// A read-only collection of undoable actions.
        /// </returns>
        public ReadOnlyObservableCollection<IUndoRedo> UndoList { get; }
        /// <summary>
        /// Resets the undo/redo manager.
        /// </summary>
        public void Reset()
        {
            _redoList.Clear();
            _undoList.Clear();
        }
        /// <summary>
        /// Adds an undoable action to the undo/redo manager.
        /// </summary>
        /// <param name="cmd">The undoable action to add.</param>
        public void Add(IUndoRedo cmd)
        {
            if (!_enableAdd) return;
            _undoList.Add(cmd);
            _redoList.Clear();
        }
        /// <summary>
        /// Performs the undo operation.
        /// </summary>
        public void Undo()
        {
            if (!_undoList.Any()) return;
            var cmd = _undoList.Last();
            _undoList.RemoveAt(_undoList.Count - 1);
            _enableAdd = false;
            cmd.Undo();
            _enableAdd = true;
            _redoList.Insert(0, cmd);
        }
        /// <summary>
        /// Performs the redo operation.
        /// </summary>
        public void Redo()
        {
            if (!_redoList.Any()) return;
            var cmd = _redoList.First();
            _redoList.RemoveAt(0);
            _enableAdd = false;
            cmd.Redo();
            _enableAdd = true;
            _undoList.Add(cmd);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="UndoRedo"/> class.
        /// </summary>
        public UndoRedo()
        {
            RedoList = new ReadOnlyObservableCollection<IUndoRedo>(_redoList);
            UndoList = new ReadOnlyObservableCollection<IUndoRedo>(_undoList);
        }
    }
}
