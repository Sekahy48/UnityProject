using System;
using System.Collections.Generic;
using System.Text;
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

        /// <summary>
        /// Los paneles de contenedores, vistos solo a traves de lo que el mundo necesita:
        /// preguntar que hay abierto y abrir o cerrar. No es el InventoryPresenter entero a
        /// proposito; los presentadores no se manipulan entre si.
        /// </summary>
        private readonly IContainerPanels _panels;

        private IEntity _actor;
        private bool _open;

        /// <summary>Objetivo de este fotograma, o null. Lo que ejecuta el toque.</summary>
        private IEntity _target;

        /// <summary>Acciones de <see cref="_target"/> en orden de prioridad.</summary>
        private List<WorldAction> _actions = new List<WorldAction>();

        /// <summary>
        /// Lista que representa si las opciones son finales o llevana a más opciones.
        /// </summary>
        private List<bool> _menuHasChildren = new List<bool>();

        /// <summary>Lo ultimo que se mando a la vista, para no repintar lo mismo cada fotograma.</summary>
        private WorldPromptData _shown;

        /* Menu radial. Mientras esta abierto el objetivo y sus acciones se CONGELAN: al mover
           el raton para elegir se mueve la mirada, y sin congelar podrias cambiar de objeto
           a mitad de eleccion. */
        private bool _menuOpen;
        private IEntity _menuTarget;
        private List<WorldAction> _menuActions = new List<WorldAction>();

        private int _hiLevel = -1;   // anillo resaltado (0 = primero, 1 = exterior)
        private int _hiIndex = -1;   // opcion resaltada dentro de ese anillo
        // Las hijas de "Abrir inventario", en el orden en que se pintan.
        private static readonly PanelType[] RING_SLOTS = { PanelType.A, PanelType.B };

        // Opcion del primer anillo cuyo anillo exterior esta abierto, o -1.
        private int _expanded = -1;

        /* Panel de inspeccion. Como el menu, congela su objetivo y se cierra solo si deja de
           alcanzarse. */
        private bool _inspecting;
        private IEntity _inspectTarget;

        private static readonly PanelType[] SIDE_PANELS = { PanelType.A, PanelType.B };

        public WorldInteractionPresenter(IWorldInteractionView view, WorldInteractionService service,
                                         IContainerPanels panels)
        {
            AC.CheckNotNull(view, nameof(view));
            AC.CheckNotNull(service, nameof(service));
            AC.CheckNotNull(panels, nameof(panels));

            _view = view;
            _service = service;
            _panels = panels;
            _view.Initialize();

            // De momento solo hay un anillo: lo que no sea el primero se ignora.
            _view.OnMenuHighlighted += (level, index) =>
            {
                _hiLevel = _menuOpen ? level : -1;
                _hiIndex = _menuOpen ? index : -1;
            };
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

            TickEvaluateInventoryDistances();
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

        private void TickEvaluateInventoryDistances()
        { 

            // Sin actor el mundo nunca se ha abierto: no hay de quien medir la distancia.
            if (_actor == null) return;

            foreach (PanelType panelType in SIDE_PANELS)
            {
                IEntity target = _panels.OccupantOf(panelType);
                if (target != null && WorldPresence.IsIn(target) && !_service.IsInRange(_actor, target))
                    _panels.ClosePanel(panelType);
            }
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
        /// Ejecuta una accion sobre la entidad objetivo. Segun la accion delega las operaciones de dominio 
        /// al servicio de interaccion con el mundo; y las operaciones relativas a la vista (mostrar/ocultar/actualizar elementos)
        /// recaen sobre metodos propios de esta clase.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="action"></param>
        /// <exception cref="ArgumentOutOfRangeException"> Si se intenta ejecutar una accion cuyo comportamiento no esta definido. </exception>
        private void Perform(IEntity target, WorldAction action, PanelType? panelType = null)
        {
            if (!_service.CanPerform(_actor, target, action)) return;

            switch (action)
            {
                case WorldAction.PickUp:
                {
                    _service.PickUp(_actor, target);
                    break;
                }
                case WorldAction.Inspect:
                { 
                    OpenInspect(target);
                    break;        
                }
                case WorldAction.Inventory:
                {
                    // Sin panel (toque de E u hoja del primer anillo) se abre en A aunque sustituya:
                    // decidir si hay que preguntar es cosa de quien construye el menu, no de aqui.
                    // Si ya se muestra en algun panel, no pasa nada.
                    if (!_panels.IsShowing(target))
                        _panels.OpenPanel(panelType ?? PanelType.A, target);
                    break;    
                } 

                default:
                    throw new ArgumentOutOfRangeException(nameof(action), "Action not supported: " + action.ToString());
            } 
        }

        /// <summary>Si el menu radial esta abierto. InputManager lo consulta cada fotograma
        /// para bloquear la vista mientras lo este.</summary>
        public bool IsMenuOpen => _menuOpen;

        private bool HasChildren(WorldAction action)
            => action == WorldAction.Inventory
            && (_panels.OccupantOf(PanelType.A) != null || _panels.OccupantOf(PanelType.B) != null);

        private List<string> RingLabels()
        {
            List<string> labels = new List<string>();
            foreach (PanelType slot in RING_SLOTS)
            {
                string name = slot == PanelType.A ? "Principal" : "Secundario";
                IEntity occupant = _panels.OccupantOf(slot);
                string who = occupant == null ? "vacio"
                        : occupant is ItemEntity item ? item.GetDisplayName() : occupant.GetName();
                labels.Add(name + " (" + who + ")");
            }
            return labels;
        }

        /// <summary>
        /// Elegir (level, index): abre el anillo exterior si la opcion tiene hijos, o ejecuta.
        /// Devuelve true si el menu debe cerrarse.
        /// </summary>
        private bool Choose(int level, int index)
        {
            if (level == 0)
            {
                if (index < 0 || index >= _menuActions.Count) return true;
                WorldAction action = _menuActions[index];

                if (_menuHasChildren[index])
                {
                    _view.CloseRingsAbove(0);
                    _view.AddRing(index, RingLabels());
                    _expanded = index;
                    return false;                 // el menu sigue abierto
                }

                Perform(_menuTarget, action);
                return true;
            }

            if (level == 1 && _expanded >= 0 && index >= 0 && index < RING_SLOTS.Length)
            {
                Perform(_menuTarget, _menuActions[_expanded], RING_SLOTS[index]);
                return true;
            }

            return true;
        }

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
            _hiLevel = -1; 
            _hiIndex = -1; 
            _expanded = -1;

            // La marca de la E sobraria con el menu encima.
            Forget();

            List<string> labels = new List<string>(_menuActions.Count); 
 
            foreach (WorldAction action in _menuActions) 
            { 
                labels.Add(action.GetDescription());
                
                _menuHasChildren.Add(HasChildren(action)); 
                
            }
            _view.OpenMenu(labels, _menuHasChildren);
        }

        /// <summary>
        /// Soltar la tecla tras mantener: ejecuta lo resaltado, o nada si el puntero esta en
        /// el centro o fuera. En los dos casos el menu se cierra.
        /// </summary>
        public void ReleaseMenu()
        {
            if (!_menuOpen) return;

            // Soltar sobre un padre con hijos no hace nada (E2); sobre una hoja, la ejecuta (E1).
            bool parent = _hiLevel == 0 && _hiIndex >= 0 && _hiIndex < _menuActions.Count
                        && _menuHasChildren[_hiIndex];
            if (_hiIndex >= 0 && !parent) Choose(_hiLevel, _hiIndex);

            CloseMenu();
        }

        private void OnMenuClicked(int level, int index)
        {
            if (!_menuOpen) return;
            if (Choose(level, index)) CloseMenu();
        }
 

        private void CloseMenu()
        {
            if (!_menuOpen) return;

            _menuOpen = false;
            _menuTarget = null;
            _menuActions = new List<WorldAction>();
            _hiLevel = -1; 
            _hiIndex = -1; 
            _expanded = -1;
            _view.CloseMenu();

            _menuHasChildren.Clear();
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
