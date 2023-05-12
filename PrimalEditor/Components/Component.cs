using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimalEditor.Components
{
    /// <summary>
    /// It is an interface to interact with multi selection components like MSTransform.
    /// </summary>
    public interface IMSComponent { }
    /// <summary>
    /// It is a high level abstraction for a single selection component like Transform
    /// </summary>
    [DataContract]
    public abstract class Component : ViewModelBase
    {
        /// <summary>
        /// It maps the component to a given game entity parent and marks it as its sole owner
        /// </summary>
        [DataMember]
        public GameEntity Owner { get; private set; }
        /// <summary>
        /// This method just returns the multiselection component.
        /// </summary>
        /// <param name="msEntity">The multiselection entity required to perform the operation</param>
        /// <returns></returns>
        public abstract IMSComponent GetMultiselectionComponent(MSEntity msEntity);
        /// <summary>
        /// This method writes the game data to the binary file
        /// </summary>
        /// <param name="bw">The writer to be used for writing to the binary</param>
        public abstract void WriteToBinary(BinaryWriter bw);
        /// <summary>
        /// Constructor of Component class which inherits from ViewModelBase and initializes the Owner.
        /// </summary>
        /// <param name="owner">The game entity to which this component belongs</param>
        public Component(GameEntity owner)
        {
            Debug.Assert(owner != null);
            Owner = owner;
        }
    }
    abstract class MSComponent<T> : ViewModelBase, IMSComponent where T : Component
    {
        private bool _enableUpdates = true;
        public List<T> SelectedComponents { get; }
        protected abstract bool UpdateComponents(string? propertyName);
        protected abstract bool UpdateMSComponents();
        public void Refresh()
        {
            _enableUpdates = false;
            UpdateMSComponents();
            _enableUpdates = true;
        }
        public MSComponent(MSEntity msEntity)
        {
            Debug.Assert(msEntity?.SelectedEntities.Any() == true);
            SelectedComponents = msEntity.SelectedEntities.Select(entity => entity.GetComponent<T>()).ToList();
            PropertyChanged += (s, e) => { if (_enableUpdates) UpdateComponents(e.PropertyName); };
        }
    }
}
