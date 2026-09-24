using Core.MVC.View.UI.World;
using Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View.World
{
    /// <summary>
    /// Pinta la marca de interaccion ("E  Recoger") sobre el objetivo del mundo.
    ///
    /// <para>Solo pinta y proyecta. Que objetivo, que accion y cuando mostrarse lo decide
    /// <c>WorldInteractionPresenter</c>; esta clase recibe texto ya resuelto y un punto del
    /// mundo, y lo lleva a la pantalla con la camara activa.</para>
    ///
    /// <para>Vive en su propio <c>UIDocument</c> y no dentro del HUD de barras: sigue el
    /// patron del inventario (documento registrado en <c>UIRegistry</c>) en vez del de
    /// <c>HUDUtils</c>, que busca su documento por etiqueta.</para>
    /// </summary>
    public class WorldInteractionView : IWorldInteractionView
    {
        private readonly UIDocument _document;
        private readonly IActiveCameraSource _cameras;

        private VisualElement _root;
        private VisualElement _prompt;
        private Label _actionLabel;
        private Label _targetLabel;
        private Label _moreLabel;
        private VisualElement _reticle;

        /* Si el presentador quiere la marca visible. Distinto de que quepa en pantalla: un
           objetivo detras de la camara sigue siendo el objetivo, solo que no se pinta. */
        private bool _wanted;

        public WorldInteractionView(UIDocument document, IActiveCameraSource cameras)
        {
            _document = document;
            _cameras = cameras;
        }

        public void Initialize()
        {
            // Sin documento asignado en UIRegistry la vista no pinta nada, pero el juego
            // sigue: recoger con la E funciona igual, solo sin marca.
            if (_document == null)
            {
                Debug.LogError("WorldInteractionView: falta asignar el UIDocument de interaccion en UIRegistry.");
                return;
            }

            _root = _document.rootVisualElement.Q<VisualElement>("world-interaction-root");
            if (_root == null)
            {
                Debug.LogError("WorldInteractionView: no se encuentra 'world-interaction-root' en el documento.");
                return;
            }

            // La capa ocupa la pantalla entera: sin esto se tragaria los clics del resto de UI.
            _root.pickingMode = PickingMode.Ignore;

            _prompt = _root.Q<VisualElement>("prompt");
            _actionLabel = _root.Q<Label>("prompt-action");
            _targetLabel = _root.Q<Label>("prompt-target");
            _moreLabel = _root.Q<Label>("prompt-more");
            _reticle = _root.Q<VisualElement>("reticle");

            HidePrompt();
            SetAttending(false);
        }

        public void ShowPrompt(WorldPromptData data)
        {
            if (_prompt == null || data == null) return;

            _actionLabel.text = data.ActionLabel;
            _targetLabel.text = data.TargetLabel;
            _targetLabel.style.display = string.IsNullOrEmpty(data.TargetLabel) ? DisplayStyle.None : DisplayStyle.Flex;
            _moreLabel.style.display = data.HasMoreActions ? DisplayStyle.Flex : DisplayStyle.None;

            _wanted = true;
            _prompt.style.display = DisplayStyle.Flex;
            _reticle?.AddToClassList("wi-reticle--active");
        }

        /// <summary>
        /// Lleva el punto del mundo a coordenadas del panel. El centrado horizontal y que la
        /// marca quede ENCIMA del punto (no con su esquina en el) lo hace el USS con
        /// <c>translate: -50% -100%</c>, que no depende del tamano del texto.
        /// </summary>
        public void PlacePrompt(float worldX, float worldY, float worldZ)
        {
            if (_prompt == null || !_wanted || _root.panel == null) return;

            Camera camera = _cameras?.Current;
            if (camera == null)
            {
                _prompt.style.visibility = Visibility.Hidden;
                return;
            }

            Vector3 world = new Vector3(worldX, worldY, worldZ);

            // Detras de la camara la proyeccion devuelve un punto reflejado que caeria en
            // pantalla como si estuviera delante: se esconde en vez de pintar mentira.
            if (camera.WorldToViewportPoint(world).z <= 0f)
            {
                _prompt.style.visibility = Visibility.Hidden;
                return;
            }

            Vector2 panelPos = RuntimePanelUtils.CameraTransformWorldToPanel(_root.panel, world, camera);

            _prompt.style.left = panelPos.x;
            _prompt.style.top = panelPos.y;
            _prompt.style.visibility = Visibility.Visible;
        }

        public void HidePrompt()
        {
            _wanted = false;
            _reticle?.RemoveFromClassList("wi-reticle--active");
            if (_prompt == null) return;
            _prompt.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// El punto de mira solo aparece cuando se apunta con la camara: ni en RTS ni con el
        /// inventario abierto, donde el raton es un cursor y no una mira.
        /// </summary>
        public void SetAttending(bool attending)
        {
            _reticle?.EnableInClassList("wi-reticle--hidden", !attending);
        }
    }
}
