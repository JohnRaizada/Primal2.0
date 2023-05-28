using System.ComponentModel;
using System.Runtime.Serialization;

namespace PrimalEditor
{
    /// <summary>
    /// A base class for view models that implements the <see cref="INotifyPropertyChanged"/> interface.
    /// </summary>
    [DataContract(IsReference = true)]
    public class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the event that is raised when a property changes.
        /// </summary>
        public event PropertyChangedEventHandler? PropertyChanged;
        /// <summary>
        /// Raises the `PropertyChanged` event.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
