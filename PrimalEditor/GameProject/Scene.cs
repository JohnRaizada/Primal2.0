using PrimalEditor.Components;
using PrimalEditor.Utilities;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Represents a scene in a game project.
    /// </summary>
    [DataContract]
    public class Scene : ViewModelBase
    {
        private string? _name;
        /// <summary>
        /// Gets or sets the name of the scene.
        /// </summary>
        [DataMember]
        public string? Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        /// <summary>
        /// Gets the project that the scene belongs to.
        /// </summary>
        [DataMember]
        public Project Project { get; private set; }
        private bool _isActive;
        /// <summary>
        /// Gets or sets a value indicating whether the scene is active.
        /// </summary>
        [DataMember]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }
        [DataMember(Name = nameof(GameEntities))]
        private readonly ObservableCollection<GameEntity> _gameEntities = new();
        /// <summary>
        /// Gets the collection of game entities in the scene.
        /// </summary>
        public ReadOnlyObservableCollection<GameEntity>? GameEntities { get; private set; }
        /// <summary>
        /// Gets the command for adding a new game entity to the scene.
        /// </summary>
        public ICommand? AddGameEntityCommand { get; private set; }
        /// <summary>
        /// Gets the command for removing a game entity from the scene.
        /// </summary>
        public ICommand? RemoveGameEntityCommand { get; private set; }
        private void AddGameEntity(GameEntity entity, int index = -1)
        {
            Debug.Assert(!_gameEntities.Contains(entity));
            entity.IsActive = IsActive;
            if (index == -1)
            {
                _gameEntities.Add(entity);
                return;
            }
            _gameEntities.Insert(index, entity);
        }
        private void RemoveGameEntity(GameEntity entity)
        {
            Debug.Assert(_gameEntities.Contains(entity));
            entity.IsActive = false;
            _gameEntities.Remove(entity);
        }
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (_gameEntities == null) return;
            GameEntities = new ReadOnlyObservableCollection<GameEntity>(_gameEntities);
            OnPropertyChanged(nameof(GameEntities));
            foreach (var entity in _gameEntities) entity.IsActive = IsActive;
            AddGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                AddGameEntity(x);
                var entityIndex = _gameEntities.Count - 1;
                Project.UndoRedo.Add(new UndoRedoAction(
                    () => RemoveGameEntity(x),
                    () => AddGameEntity(x, entityIndex),
                    $"Add {x.Name} to {Name}"
                ));
            });
            RemoveGameEntityCommand = new RelayCommand<GameEntity>(x =>
            {
                var entityIndex = _gameEntities.IndexOf(x);
                RemoveGameEntity(x);
                Project.UndoRedo.Add(new UndoRedoAction(
                    () => AddGameEntity(x, entityIndex),
                    () => RemoveGameEntity(x),
                    $"Remove {x.Name}"
                ));
            });
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Scene"/> class with the specified project and name.
        /// </summary>
        /// <param name="project">The project that the scene belongs to.</param>
        /// <param name="name">The name of the scene.</param>
        public Scene(Project project, string name)
        {
            Debug.Assert(project != null);
            Project = project;
            Name = name;
            OnDeserialized(new StreamingContext());
        }
    }
}
