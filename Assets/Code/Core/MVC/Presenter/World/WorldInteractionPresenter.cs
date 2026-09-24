using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.ECS.Systems;
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

        public WorldInteractionPresenter(IWorldInteractionView view, WorldInteractionService service)
        {
            AC.CheckNotNull(view, nameof(view));
            AC.CheckNotNull(service, nameof(service));

            _view = view;
            _service = service;
            _view.Initialize();
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
            if (!_open || _target == null || _actions.Count == 0) return;

            _service.Execute(_actor, _target, _actions[0]);

            // El monton puede haber desaparecido o cambiado: que la marca lo refleje ya y
            // no un fotograma despues.
            Tick();
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
