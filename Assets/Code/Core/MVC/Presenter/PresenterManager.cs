using System.Collections.Generic;

namespace Core.MVC.Presenter 
{
    public class PresenterManager
    {
        private Dictionary<PresenterType, IPresenter> presenters; 

        public PresenterManager()
        {
            presenters = new Dictionary<PresenterType, IPresenter>();
        }

        /// <summary>
        /// Registers a presenter for a type. Does nothing if one is already registered:
        /// first registration wins, so an accidental duplicate cannot silently take over.
        /// To deliberately swap one out, use <see cref="ReplacePresenter"/>.
        /// </summary>
        public void RegisterPresenter(PresenterType type, IPresenter presenter)
        {
            if (!presenters.ContainsKey(type))
            {
                presenters[type] = presenter;
            }
        }

        /// <summary>
        /// Replaces the presenter registered for a type, or registers it if absent.
        /// Used when the presenter and its view are rebuilt from scratch — currently
        /// on UI Toolkit live reload, where the old view's VisualElements are orphaned
        /// and everything downstream must point at the new instance.
        /// </summary>
        public void ReplacePresenter(PresenterType type, IPresenter presenter)
        {
            presenters[type] = presenter;
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