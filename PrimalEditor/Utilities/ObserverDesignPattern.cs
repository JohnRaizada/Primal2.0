using System.Collections.Generic;

namespace PrimalEditor.Utilities
{
    interface ISubject
    {
        void Attach(IObserver observer);
        void Detach(IObserver observer);
        void Notify();
    }
    interface IObserver
    {
        void Update(ISubject subject);
    }
    class Subject : ISubject
    {
        private readonly List<IObserver> _observers = new();
        public void Attach(IObserver observer) => _observers.Add(observer);
        public void Detach(IObserver observer) => _observers.Remove(observer);

        public void Notify()
        {
            foreach (var observer in _observers) observer.Update(this);
        }
    }
}
