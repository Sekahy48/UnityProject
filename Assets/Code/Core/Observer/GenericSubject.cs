
using System.Collections.Generic;

namespace Core.Observer
{
    public abstract class GenericSubject : ISubject
    {
        private List<IObserver> _observers = new List<IObserver>();
        public void Attach(IObserver observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
            }
        }
        public void Detach(IObserver observer)
        {
            _observers.Remove(observer);
        }
        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Update();
            }
        }
    }
}