using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Inventory;
using Core.MVC.View.UI.Inventory;
using Core.MVC.View.UI.World;
using Core.Services;
using AC = Core.Utils.ArgumentChecker;

namespace Core.MVC.Presenter.World
{
    /// <summary>
    /// Decide que se ofrece al jugador sobre lo que tiene delante y ejecuta lo que elija.
    ///
    /// <para><b>Es el unico sitio que decide.</b> Las estrategias de camara solo traducen la
    /// tecla en intenciones (toque, mantener, soltar) y la vista solo pinta. Si la decision
    /// viviera en la estrategia, FPS y TPS tendrian cada una su copia de "que tengo delante y
    /// que hago con ello".</para>
    ///
    /// <para><b>Abierto significa "atendiendo al mundo".</b> No lo abre el jugador:
    /// <c>InputManager</c> lo abre y lo cierra cada fotograma segun haya una camara que sea
    /// fuente de interaccion y el inventario este cerrado. Asi las dos condiciones (W7, W8)
    /// son una sola regla en un solo sitio.</para>
    /// </summary>
    public class WorldInteractionPresenter : IPresenter
    {
        private readonly IWorldInteractionView _view;
        private readonly WorldInteractionService _service;

        private IEntity _actor;
        private bool _open;

        /// <summary>Objetivo de este fotograma, o null. Lo que ejecuta el toque.</summary>
        private IEntity _target;

        /// <summary>Acciones de <see cref="_target"/> en orden de prioridad.</summary>
        private List<WorldAction> _actions = new List<WorldAction>();

        /// <summary>Lo ultimo que se mando a la vista, para no repintar lo mismo cada fotograma.</summary>
        private WorldPromptData _shown;

        /* Menu radial. Mientras esta abierto el objetivo y sus acciones se CONGELAN: al mover
           el raton para elegir se mueve la mirada, y sin congelar podrias cambiar de objeto
           a mitad de eleccion. */
        private bool _menuOpen;
        private IEntity _menuTarget;
        private List<WorldAction> _menuActions = new List<WorldAction>();

        /// <summary>
        /// Opcion resaltada, o -1. Es el UNICO dato que decide que se ejecuta al soltar: la
        /// vista solo informa de donde esta el puntero y pinta lo que esta aqui, asi que lo
        /// resaltado y lo ejecutado no pueden discrepar.
        /// </summary>
        private int _highlighted = -1;

        /* Panel de inspeccion. Como el menu, congela su objetivo y se cierra solo si deja de
           alcanzarse. */
        private bool _inspecting;
        private IEntity _inspectTarget;

        public WorldInteractionPresenter(IWorldInteractionView view, WorldInteractionService service)
        {
            AC.CheckNotNull(view, nameof(view));
            AC.CheckNotNull(service, nameof(service));

            _view = view;
            _service = service;
            _view.Initialize();

            _view.OnMenuHighlighted += index => _highlighted = _menuOpen ? index : -1;
            _view.OnMenuClicked += OnMenuClicked;
            _view.OnMenuDismissed += CloseMenu;
            _view.OnInspectCloseRequested += CloseInspect;
        }

        #region IPresenter

        public void Open(IEntity actor)
        {
            _actor = actor;
            _open = actor != null;
            _view.SetAttending(_open);
        }

        /// <summary>
        /// Deja de atender al mundo y borra la marca. El objetivo se olvida: al volver a
        /// abrir se busca de nuevo, porque el que habia puede no seguir ahi.
        /// </summary>
        public void Close(bool absolute)
        {
            _open = false;
            CloseMenu();
            CloseInspect();
            Forget();
            _view.SetAttending(false);
        }

        public bool IsOpen() => _open;

        /// <summary>Vuelve a evaluar ya, sin esperar al siguiente fotograma.</summary>
        public void Refresh() => Tick();

        #endregion

        #region Cada fotograma

        /// <summary>
        /// Busca que hay delante, decide la marca y la coloca.
        ///
        /// <para>Independiente de la tecla: esto corre haya o no pulsacion, y la pulsacion
        /// llega por <see cref="OnTapped"/>. Son dos "if" separados, no uno con dos ramas.</para>
        /// </summary>
        public void Tick()
        {
            if (!_open) return;

            // Con el menu abierto no se busca: el objetivo esta congelado. Solo se vigila que
            // siga a mano; si no, el menu se cierra solo (la interfaz no se queda ofreciendo
            // algo que ya no se puede hacer).
            if (_menuOpen)
            {
                if (!_service.CanReach(_actor, _menuTarget)) CloseMenu();
                return;
            }

            if (_inspecting)
            {
                if (!_service.CanReach(_actor, _inspectTarget)) CloseInspect();
                return;
            }

            _target = _service.FindTarget(_actor);
            _actions = _target == null
                ? new List<WorldAction>()
                : _service.GetAvailableActions(_actor, _target);

            if (_target == null || _actions.Count == 0)
            {
                Forget();
                return;
            }

            WorldPromptData data = BuildPrompt(_target, _actions);
            if (!data.SameAs(_shown))
            {
                _view.ShowPrompt(data);
                _shown = data;
            }

            (float x, float y, float z)? anchor = AnchorOf(_target);
            if (anchor.HasValue)
                _view.PlacePrompt(anchor.Value.x, anchor.Value.y, anchor.Value.z);
        }

        #endregion

        #region Intenciones del jugador

        /// <summary>
        /// Pulsacion corta: la accion por defecto, que es la primera de la lista.
        ///
        /// Se ejecuta sobre el objetivo de este fotograma. El servicio vuelve a comprobar
        /// alcance y acciones, asi que un objetivo que ya no vale no hace nada.
        /// </summary>
        public void OnTapped()
        {
            if (!_open) return;

            // Con el panel abierto, la misma tecla lo cierra: es lo que se espera de la
            // tecla con la que se abrio.
            if (_inspecting)
            {
                CloseInspect();
                return;
            }

            if (_target == null || _actions.Count == 0) return;

            Perform(_target, _actions[0]);

            // El monton puede haber desaparecido o cambiado: que la marca lo refleje ya y
            // no un fotograma despues.
            Tick();
        }

        /// <summary>
        /// Ejecuta una accion por el servicio y, si es de las que abren interfaz, la abre.
        ///
        /// Inspeccionar pasa por el servicio aunque no cambie nada en el mundo: asi su
        /// comprobacion de alcance y de acciones es la misma que la de las demas. Solo si el
        /// servicio la da por buena se abre el panel.
        /// </summary>
        private void Perform(IEntity target, WorldAction action)
        {
            if (!_service.Execute(_actor, target, action)) return;

            if (action == WorldAction.Inspect) OpenInspect(target);
        }

        /// <summary>Si el menu radial esta abierto. InputManager lo consulta cada fotograma
        /// para bloquear la vista mientras lo este.</summary>
        public bool IsMenuOpen => _menuOpen;

        /// <summary>
        /// Mantener la tecla: abre el menu con las acciones del objetivo actual.
        /// Sin objetivo no hay menu: mantener sobre nada no hace nada.
        /// </summary>
        public void OpenMenu()
        {
            if (!_open || _menuOpen || _target == null || _actions.Count == 0) return;

            _menuOpen = true;
            _menuTarget = _target;
            _menuActions = new List<WorldAction>(_actions);
            _highlighted = -1;

            // La marca de la E sobraria con el menu encima.
            Forget();

            List<string> labels = new List<string>(_menuActions.Count);
            foreach (WorldAction action in _menuActions) labels.Add(action.GetDescription());
            _view.OpenMenu(labels);
        }

        /// <summary>
        /// Soltar la tecla tras mantener: ejecuta lo resaltado, o nada si el puntero esta en
        /// el centro o fuera. En los dos casos el menu se cierra.
        /// </summary>
        public void ReleaseMenu()
        {
            if (!_menuOpen) return;

            int chosen = _highlighted;
            if (chosen >= 0) ExecuteMenuOption(chosen);
            CloseMenu();
        }

        private void OnMenuClicked(int index)
        {
            if (!_menuOpen) return;

            ExecuteMenuOption(index);
            CloseMenu();
        }

        private void ExecuteMenuOption(int index)
        {
            if (index < 0 || index >= _menuActions.Count) return;
            Perform(_menuTarget, _menuActions[index]);
        }

        private void CloseMenu()
        {
            if (!_menuOpen) return;

            _menuOpen = false;
            _menuTarget = null;
            _menuActions = new List<WorldAction>();
            _highlighted = -1;
            _view.CloseMenu();

            // La marca vuelve ya, sin esperar al siguiente fotograma.
            Tick();
        }

        /// <summary>Si el panel de inspeccion esta abierto. InputManager bloquea la vista
        /// mientras lo este, para poder usar el cursor.</summary>
        public bool IsInspecting => _inspecting;

        private void OpenInspect(IEntity target)
        {
            InspectPanelData data = BuildInspectData(target);
            if (data == null) return;

            _inspecting = true;
            _inspectTarget = target;
            Forget();
            _view.ShowInspect(data);
        }

        /// <summary>Cierra el panel. Publico porque Esc lo pide desde fuera.</summary>
        public void CloseInspect()
        {
            if (!_inspecting) return;

            _inspecting = false;
            _inspectTarget = null;
            _view.HideInspect();
            Tick();
        }

        /// <summary>
        /// Describe un monton para el panel. El conjunto usa el representante para nombre,
        /// icono y descripcion, pero la cantidad y el peso son los de TODOS los lotes. Cada
        /// lote es una variante, con su propia durabilidad.
        /// </summary>
        private static InspectPanelData BuildInspectData(IEntity target)
        {
            // Un item suelto se describe a si mismo, una unidad y sin desglose.
            if (target is ItemEntity single)
                return new InspectPanelData
                {
                    Item = DisplayDTOsBuilder.BuildDisplayData(single, 1),
                    Lots = new List<ItemDisplayData>()
                };

            GroundLotComponent lot = target.GetComponent<GroundLotComponent>();
            if (lot == null || lot.Representative == null) return null;

            ItemDisplayData item = DisplayDTOsBuilder.BuildDisplayData(lot.Representative, lot.TotalUnits);
            item.TotalWeight = lot.TotalWeight;

            List<ItemDisplayData> lots = new List<ItemDisplayData>(lot.Lots.Count);
            foreach (SubLot sub in lot.Lots)
                lots.Add(DisplayDTOsBuilder.BuildDisplayData(sub.Item, sub.Amount));

            return new InspectPanelData { Item = item, Lots = lots };
        }

        #endregion

        #region Auxiliares

        private void Forget()
        {
            _target = null;
            _actions = new List<WorldAction>();

            if (_shown == null) return;
            _view.HidePrompt();
            _shown = null;
        }

        private static WorldPromptData BuildPrompt(IEntity target, List<WorldAction> actions)
        {
            return new WorldPromptData
            {
                ActionLabel = actions[0].GetDescription(),
                TargetLabel = DescribeTarget(target),
                HasMoreActions = actions.Count > 1
            };
        }

        /// <summary>
        /// Que se lee debajo de la accion. Hoy solo hay montones; cuando haya otros
        /// objetivos (un arcon del suelo, un NPC) cada uno anade aqui su forma de nombrarse.
        /// </summary>
        private static string DescribeTarget(IEntity target)
        {
            GroundLotComponent lot = target.GetComponent<GroundLotComponent>();
            if (lot != null && lot.Representative != null)
            {
                int units = lot.TotalUnits;
                string name = lot.Representative.GetDisplayName();
                return units > 1 ? $"{name} x{units}" : name;
            }

            if (target is ItemEntity item) return item.GetDisplayName();

            return string.Empty;
        }

        /// <summary>
        /// Donde va la marca: el punto mas alto del volumen de interaccion, en el mundo.
        /// Ver <see cref="InteractionVolume.TopPoint"/>.
        /// </summary>
        private static (float x, float y, float z)? AnchorOf(IEntity target)
        {
            InteractionVolumeComponent volume = target.GetComponent<InteractionVolumeComponent>();
            PositionComponent position = target.GetComponent<PositionComponent>();
            if (volume == null || position == null) return null;

            return volume.TopPointInWorld(position);
        }

        #endregion
    }
}
