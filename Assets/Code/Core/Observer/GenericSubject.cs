
using System.Collections.Generic;

namespace Observer
{
    public abstract class GenericSubject : ISubject
    {
        protected List<IObserver> observers = new List<IObserver>();
        public void Attach(IObserver observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
            }
        }
        public void Detach(IObserver observer)
        {
            observers.Remove(observer);
        }
        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update();
            }
        }
    }
}