using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.MVC.View.UI.HUD;
using Core.MVC.View.UI.HUD.States;
using Core.MVC.View.UI.HUD.States.Category;
using Core.Observer;
using AC = Core.Utils.ArgumentChecker;

namespace Core.MVC.Presenter.HUD
{
    /// <summary>
    /// HUD del jugador: barras de energia y (pronto) estados. Un solo presentador con una
    /// region por pieza, para que al revisar las barras se sepa que es de cada una.
    ///
    /// <para>Escucha el EventBus como el resto de presentadores, y no observa al sistema de
    /// energia directamente: el evento dice de que entidad es, y el HUD solo pinta al
    /// jugador.</para>
    /// </summary>
    public class HUDPresenter : IPresenter, IEventObserver
    {
        private readonly IHUDView _view;
        private IEntity _player;
        private bool _open;

        public HUDPresenter(IHUDView view)
        {
            AC.CheckNotNull(view, nameof(view));
            _view = view;
            _view.Initialize(); 
        }

        #region IPresenter

        /// <summary>
        /// Muestra el HUD del jugador y empieza a escuchar. Pinta ya con los valores actuales:
        /// el primer evento puede tardar (con la energia llena no se publica nada).
        /// </summary>
        public void Open(IEntity player)
        {
            _player = player;
            if (_open) return;

            _open = true;
            Subscribe();
            _view.Show();
            Refresh();
        }

        /// <summary>
        /// Oculta el HUD y deja de escuchar. Tambien lo usa la recarga en caliente, para que
        /// el presentador viejo no siga suscrito.
        /// </summary>
        public void Close(bool absolute)
        {
            if (!_open) return;

            _open = false;
            Unsubscribe();
            _view.Hide();
        }

        public bool IsOpen() => _open;

        public void Refresh()
        {
            if (_player == null) return;
            PaintEnergy(_player.GetComponent<EnergyComponent>());
            RefreshLoad();
        }

        #endregion

        #region Events
        public void UpdateOnEvent(GameEvent gameEvent)
        {
            // El bus avisa de cualquier entidad; el HUD es solo del jugador.
            if (!ReferenceEquals(gameEvent.GetEntity(), _player)) return;

            switch (gameEvent.GetEventType())
            {
                case GameEventType.EnergyChanged:
                {
                    PaintEnergy(gameEvent.GetComponent<EnergyComponent>());
                    break;
                }
                case GameEventType.NormalWeight:
                case GameEventType.ExtraWeight:
                case GameEventType.Overweight:
                case GameEventType.Immobile:
                {
                    ShowLoad(gameEvent.GetEventType());
                    break;
                }
            }
        }

        private void Subscribe()
        {
            EventBus bus = EventBus.GetInstance();
            bus.Subscribe(GameEventType.EnergyChanged, this);
            bus.Subscribe(GameEventType.NormalWeight, this);
            bus.Subscribe(GameEventType.ExtraWeight, this);
            bus.Subscribe(GameEventType.Overweight, this);
            bus.Subscribe(GameEventType.Immobile, this);
        }

        private void Unsubscribe()
        {
            EventBus bus = EventBus.GetInstance();
            bus.Unsubscribe(GameEventType.EnergyChanged, this);
            bus.Unsubscribe(GameEventType.NormalWeight, this);
            bus.Unsubscribe(GameEventType.ExtraWeight, this);
            bus.Unsubscribe(GameEventType.Overweight, this);
            bus.Unsubscribe(GameEventType.Immobile, this);
        }
        #endregion 

        #region Barras de energia (provisionales: pendientes de revision)

        private void PaintEnergy(EnergyComponent energy)
        {
            if (energy == null) return;

            _view.SetStamina(Ratio(energy.Stamina, energy.MaxStamina));
            _view.SetFatigue(Ratio(energy.Fatigue, energy.MaxFatigue));
        }

        private static float Ratio(float value, float max) => max > 0 ? value / max : 0f;

        #endregion

        #region Estados

        /* Franja de carga -> nivel de peso. Una sola traduccion, la usan el evento y Refresh. */
        private static readonly Dictionary<GameEventType, WeightLevel> LoadLevels = new()
        {
            { GameEventType.NormalWeight, WeightLevel.None     },
            { GameEventType.ExtraWeight,  WeightLevel.Extra    },
            { GameEventType.Overweight,   WeightLevel.Over     },
            { GameEventType.Immobile,     WeightLevel.Immobile },
        };

        /// <summary>
        /// Muestra el estado de peso de una franja de carga. Lo llaman el evento de franja y
        /// Refresh: los dos pintan igual, y nadie mas se entera (no se publica nada).
        /// </summary>
        private void ShowLoad(GameEventType band)
        {
            if (!LoadLevels.TryGetValue(band, out WeightLevel level)) return;
            _view.AddState(Weight(level), level.GetDescription());
        }

        /// <summary>
        /// Calcula la franja actual del jugador y la muestra. Hace falta al abrir el HUD y tras
        /// una recarga: hasta el siguiente cambio de inventario no llega ningun evento. Mismo
        /// calculo que InventoryPanelPresenter (sin techo, cuenta como al limite).
        /// </summary>
        private void RefreshLoad()
        {
            InventoryComponent inventory = _player.GetComponent<InventoryComponent>();
            if (inventory == null) return;

            float weight = inventory.Inventory.GetTotalWeight();
            float maxLoad = CarryCapacity.GetMaxLoad(_player);
            ShowLoad(CarryCapacity.ClassifyLoad(maxLoad > 0 ? weight / maxLoad : 1f));
        }

        private static PlayerStatus Weight(WeightLevel level) => new PlayerStatus(StatusCategory.Weight, (int)level);

        #endregion
    }
}
