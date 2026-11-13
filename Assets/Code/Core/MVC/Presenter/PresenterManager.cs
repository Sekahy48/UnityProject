using System.Collections.Generic;
using AC = Core.Utils.ArgumentChecker;

namespace Core.MVC.Presenter 
{
    public class PresenterManager
    {
        private Dictionary<PresenterType, IPresenter> _presenters; 

        public PresenterManager()
        {
            _presenters = new Dictionary<PresenterType, IPresenter>();
        }

        /// <summary>
        /// Registra un presentador para un tipo. Si ya habia uno, no hace nada: gana el
        /// primer registro, para que un duplicado accidental no lo sustituya sin avisar.
        /// Para cambiarlo a proposito, <see cref="ReplacePresenter"/>.
        /// </summary>
        public void RegisterPresenter(PresenterType type, IPresenter presenter)
        {
            AC.CheckNotNull(presenter, nameof(presenter));
            if (!_presenters.ContainsKey(type))
            {
                _presenters[type] = presenter;
            }
        }

        /// <summary>
        /// Sustituye el presentador registrado para un tipo, o lo registra si no habia.
        /// Se usa cuando el presentador y su vista se reconstruyen desde cero (hoy, en la
        /// recarga en vivo de UI Toolkit, que deja huerfanos los VisualElements de la vista vieja
        /// y obliga a que todo lo que depende de ella apunte a la instancia nueva).
        /// </summary>
        public void ReplacePresenter(PresenterType type, IPresenter presenter)
        {
            AC.CheckNotNull(presenter, nameof(presenter));
            _presenters[type] = presenter;
        }

        public T GetPresenter<T>(PresenterType type) where T : IPresenter
        {
            if (_presenters.TryGetValue(type, out IPresenter presenter))
            {
                return (T)presenter;
            }       
            return default(T);
        }
    }   
}