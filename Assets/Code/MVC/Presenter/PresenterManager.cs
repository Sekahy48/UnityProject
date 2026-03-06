using System.Collections.Generic;

namespace MVC.Presenter 
{
    public class PresenterManager
    {
        private Dictionary<PresenterType, IPresenter> presenters; 

        public PresenterManager()
        {
            presenters = new Dictionary<PresenterType, IPresenter>();
        }

        public void RegisterPresenter(PresenterType type, IPresenter presenter)
        {
            if (!presenters.ContainsKey(type))
            {
                presenters[type] = presenter;
            }
        }

        public T GetPresenter<T>(PresenterType type) where T : IPresenter
        {
            if (presenters.TryGetValue(type, out IPresenter presenter))
            {
                return (T)presenter;
            }       
            return default(T);
        }
    }   
}