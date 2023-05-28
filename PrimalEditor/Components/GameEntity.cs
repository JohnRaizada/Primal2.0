using PrimalEditor.DLLWrapper;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.Components
{
    /// <summary>
    /// Represents a <see cref="GameEntity"/>.
    /// </summary>
    [DataContract]
    [KnownType(typeof(Transform))]
    [KnownType(typeof(Script))]
    public class GameEntity : ViewModelBase
    {
        private int _entityId = ID.INVALID_ID;
        /// <summary>
        /// The unique identifier of the game entity.
        /// </summary>
        public int EntityId
        {
            get => _entityId;
            set
            {
                if (_entityId == value) return;
                _entityId = value;
                OnPropertyChanged(nameof(EntityId));
            }
        }
        private bool _isActive;
        /// <summary>
        /// Whether the game entity is active.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value) return;
                _isActive = value;
                if (_isActive)
                {
                    EntityId = EngineAPI.EntityAPI.CreateGameEntity(this);
                    Debug.Assert(ID.IsValid(_entityId));
                }
                else if (ID.IsValid(EntityId))
                {
                    EngineAPI.EntityAPI.RemoveGameEntity(this);
                    EntityId = ID.INVALID_ID;
                }
                OnPropertyChanged(nameof(IsActive));
            }
        }
        private bool _isEnabled = true;
        /// <summary>
        /// Whether the game entity is enabled.
        /// </summary>
        [DataMember]
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private string? _name;
        /// <summary>
        /// The name of the game entity.
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
        /// The scene that the game entity belongs to.
        /// </summary>
        [DataMember]
        public Scene ParentScene { get; private set; }
        [DataMember(Name = nameof(Components))]
        private readonly ObservableCollection<Component> _components = new();
        /// <summary>
        /// The collection of components that belong to the game entity.
        /// </summary>
        public ReadOnlyObservableCollection<Component>? Components { get; private set; }
        /// <summary>
        /// Gets the component of the specified type.
        /// </summary>
        /// <param name="type">The type of the component to get.</param>
        /// <returns>The component of the specified type, or null if no such component exists.</returns>
        /// <remarks>
        /// This method first checks if the Components collection is null. If it is, it returns null. Otherwise, it calls the FirstOrDefault() method on the Components collection, passing in a predicate that checks if the component's type is equal to the specified type. If the FirstOrDefault() method returns a value, it is returned. Otherwise, null is returned.
        /// </remarks>
        public Component? GetComponent(Type type) => Components?.FirstOrDefault(c => c.GetType() == type);
        /// <summary>
        /// Gets the component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>The component of the specified type, or null if no such component exists.</returns>
        public T? GetComponent<T>() where T : Component => (T?)GetComponent(typeof(T));
        /// <summary>
        /// Adds the specified component to the game entity.
        /// </summary>
        /// <param name="component">The component to add.</param>
        /// <returns>True if the component was added successfully; otherwise, false.</returns>
        public bool AddComponent(Component component)
        {
            Debug.Assert(component != null);
            if (Components == null) return false;
            if (!Components.Any(x => x.GetType() == component.GetType()))
            {
                IsActive = false;
                _components.Add(component);
                IsActive = true;
                return true;
            }
            Logger.Log(MessageType.Warning, $"Entity {Name} already has a {component.GetType().Name} component");
            return false;
        }
        /// <summary>
        /// Removes the specified component from the game entity.
        /// </summary>
        /// <param name="component">The component to remove.</param>
        public void RemoveComponent(Component component)
        {
            Debug.Assert(component != null);
            if (component is Transform) return; // Transform component can't be removed
            if (!_components.Contains(component)) return;
            IsActive = false;
            _components.Remove(component);
            IsActive = true;
        }
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (_components == null) return;
            Components = new ReadOnlyObservableCollection<Component>(_components);
            OnPropertyChanged(nameof(Components));
        }
        /// <summary>
        /// Initializes a new instance of the GameEntity class.
        /// </summary>
        /// <param name="scene">The scene that the game entity belongs to.</param>
        public GameEntity(Scene scene)
        {
            Debug.Assert(scene != null);
            ParentScene = scene;
            _components.Add(new Transform(this));
            OnDeserialized(new StreamingContext());
        }
    }
    /// <summary>
    /// Represents a collection of game entities that can be selected and edited together.
    /// </summary>
    public abstract class MSEntity : ViewModelBase
    {
        /// <summary>
        /// Whether updates to selected entities are enabled.
        /// </summary>
        private bool _enableUpdates = true;
        private bool? _isEnabled;
        /// <summary>
        /// Whether the game entity is enabled.
        /// </summary>
        public bool? IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }
        private string? _name;
        /// <summary>
        /// The name of the game entity.
        /// </summary>
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
        private readonly ObservableCollection<IMSComponent> _components = new();
        /// <summary>
        /// The collection of components that belong to the selected game entities.
        /// </summary>
        public ReadOnlyObservableCollection<IMSComponent> Components { get; }
        /// <summary>
        /// Gets the component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the component to get.</typeparam>
        /// <returns>The component of the specified type, or null if no such component exists.</returns>
        public T? GetMSComponent<T>() where T : IMSComponent => (T?)Components.FirstOrDefault(x => x.GetType() == typeof(T));
        /// <summary>
        /// Gets the list of selected game entities.
        /// </summary>
        public List<GameEntity?> SelectedEntities { get; }
        private void MakeComponentList()
        {
            _components.Clear();
            var firstEntity = SelectedEntities.FirstOrDefault();
            if (firstEntity == null) return;
            if (firstEntity.Components == null) return;
            foreach (var component in firstEntity.Components)
            {
                var type = component.GetType();
                if (SelectedEntities.Skip(1).Any(entity => entity?.GetComponent(type) == null)) continue;
                Debug.Assert(Components.FirstOrDefault(x => x.GetType() == type) == null);
                _components.Add(component.GetMultiselectionComponent(this));
            }
        }
        /// <summary>
        /// Gets the mixed value of a property from a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the list.</typeparam>
        /// <param name="objects">The list of objects.</param>
        /// <param name="getProperty">The function to get the property value from an object.</param>
        /// <returns>The mixed value of the property, or null if the property is not present on all objects in the list.</returns>
        public static float? GetMixedValue<T>(List<T?> objects, Func<T, float> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => !getProperty(x).IsTheSameAs(value)) ? (float?)null : value;
        }
        /// <summary>
        /// Gets the mixed value of a property from a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the list.</typeparam>
        /// <param name="objects">The list of objects.</param>
        /// <param name="getProperty">The function to get the property value from an object.</param>
        /// <returns>The mixed value of the property, or null if the property is not present on all objects in the list.</returns>
        public static bool? GetMixedValue<T>(List<T?> objects, Func<T?, bool> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => value != getProperty(x)) ? (bool?)null : value;
        }
        /// <summary>
        /// Gets the mixed value of a property from a list of objects.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the list.</typeparam>
        /// <param name="objects">The list of objects.</param>
        /// <param name="getProperty">The function to get the property value from an object.</param>
        /// <returns>The mixed value of the property, or null if the property is not present on all objects in the list.</returns>
        public static string? GetMixedValue<T>(List<T?> objects, Func<T?, string?> getProperty)
        {
            var value = getProperty(objects.First());
            return objects.Skip(1).Any(x => value != getProperty(x)) ? null : value;
        }
        /// <summary>
        /// Updates the enabled state of the selected game entities.
        /// </summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        protected virtual bool UpdateGameEntities(string? propertyName)
        {
            if (IsEnabled == null) return false;
            switch (propertyName)
            {
                case nameof(IsEnabled):
                    SelectedEntities.ForEach(x => x.IsEnabled = IsEnabled.Value);
                    return true;
                case nameof(Name):
                    SelectedEntities.ForEach(x => x.Name = Name);
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Updates the properties of the MSEntity class to match the properties of the selected game entities.
        /// </summary>
        protected virtual bool UpdateMSGameEntity()
        {
            IsEnabled = GetMixedValue(SelectedEntities, new Func<GameEntity?, bool>(x => x.IsEnabled));
            Name = GetMixedValue(SelectedEntities, new Func<GameEntity?, string?>(x => x?.Name));
            return true;
        }
        /// <summary>
        /// Refreshes the MSEntity class.
        /// </summary>
        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSGameEntity();
            MakeComponentList();
            _enableUpdates = true;
        }
        /// <summary>
        /// Creates a new instance of the MSEntity class.
        /// </summary>
        /// <param name="entities">The list of selected game entities.</param>
        public MSEntity(List<GameEntity?> entities)
        {
            Debug.Assert(entities?.Any() == true);
            Components = new ReadOnlyObservableCollection<IMSComponent>(_components);
            SelectedEntities = entities;
            PropertyChanged += (s, e) => { if (_enableUpdates) UpdateGameEntities(e.PropertyName); };
        }
    }
    class MSGameEntity : MSEntity
    {
        public MSGameEntity(List<GameEntity?> entities) : base(entities) => Refresh();
    }
}
