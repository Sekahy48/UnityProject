using System;
using System.Collections.Generic;
using Core.MVC.View.UI.Inventory;
using Core.MVC.View.UI.World;
using MVC.View.Controls;
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
        private RadialMenu _radial;

        /* Panel de inspeccion (Inspect/InspectPanel.uxml, clonado dentro de este documento). */
        private readonly VisualTreeAsset _inspectTemplate;
        private VisualElement _inspectRoot;
        /* Capa a pantalla completa que aloja el panel. Modal mientras se inspecciona (ver ShowInspect). */
        private VisualElement _inspectLayer;
        private VisualElement _inspectIcon;
        private Label _inspectTitle, _inspectName, _inspectType, _inspectDescription;
        private Label _inspectAmount, _inspectWeightTotal, _inspectWeightUnit, _inspectDurability, _inspectSize;
        private VisualElement _inspectLotsSection;
        private ScrollView _inspectLots;

        public event Action OnInspectCloseRequested;

        public event Action<int> OnMenuHighlighted;
        public event Action<int> OnMenuClicked;
        public event Action OnMenuDismissed;

        /* Si el presentador quiere la marca visible. Distinto de que quepa en pantalla: un
           objetivo detras de la camara sigue siendo el objetivo, solo que no se pinta. */
        private bool _wanted;

        public WorldInteractionView(UIDocument document, IActiveCameraSource cameras,
                                    VisualTreeAsset inspectTemplate)
        {
            _document = document;
            _cameras = cameras;
            _inspectTemplate = inspectTemplate;
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
            // Tambien la raiz que crea Unity para el documento: este documento va por ENCIMA
            // del inventario (sort order), y si ella capturara tapara A y B sin que se vea nada.
            _document.rootVisualElement.pickingMode = PickingMode.Ignore;
            _root.pickingMode = PickingMode.Ignore;

            _prompt = _root.Q<VisualElement>("prompt");
            _actionLabel = _root.Q<Label>("prompt-action");
            _targetLabel = _root.Q<Label>("prompt-target");
            _moreLabel = _root.Q<Label>("prompt-more");
            _reticle = _root.Q<VisualElement>("reticle");

            BuildInspectPanel();

            // El radial se crea por codigo: su estructura depende de cuantas opciones haya.
            // Va el ultimo para quedar por encima de la marca y del punto de mira.
            _radial = new RadialMenu();
            _radial.OnHighlighted += index => OnMenuHighlighted?.Invoke(index);
            _radial.OnClicked += index => OnMenuClicked?.Invoke(index);
            _radial.OnDismissed += () => OnMenuDismissed?.Invoke();
            _root.Add(_radial);

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

        #region Panel de inspeccion

        /// <summary>
        /// Clona la plantilla del panel dentro de este documento. Va en el mismo documento
        /// que la marca y el radial porque pertenece al mismo momento de juego (interactuar
        /// con algo del mundo); un documento aparte solo anadiria otro objeto que enganchar
        /// en la escena.
        /// </summary>
        private void BuildInspectPanel()
        {
            if (_inspectTemplate == null)
            {
                Debug.LogError("WorldInteractionView: falta asignar la plantilla del panel de inspeccion en UIRegistry.");
                return;
            }

            TemplateContainer container = _inspectTemplate.Instantiate();

            // El contenedor de la plantilla ocupa la pantalla para que el panel se centre en
            // ella, pero no captura el raton: solo lo hace el propio panel.
            container.style.position = Position.Absolute;
            container.style.left = 0;
            container.style.top = 0;
            container.style.right = 0;
            container.style.bottom = 0;
            container.pickingMode = PickingMode.Ignore;
            _root.Add(container);
            _inspectLayer = container;

            _inspectRoot = container.Q<VisualElement>("inspect-root");
            _inspectIcon = container.Q<VisualElement>("ip-icon");
            _inspectTitle = container.Q<Label>("ip-title");
            _inspectName = container.Q<Label>("ip-name");
            _inspectType = container.Q<Label>("ip-type");
            _inspectDescription = container.Q<Label>("ip-description");
            _inspectAmount = container.Q<Label>("ip-amount");
            _inspectWeightTotal = container.Q<Label>("ip-weight-total");
            _inspectWeightUnit = container.Q<Label>("ip-weight-unit");
            _inspectDurability = container.Q<Label>("ip-durability");
            _inspectSize = container.Q<Label>("ip-size");
            _inspectLotsSection = container.Q<VisualElement>("ip-lots-section");
            _inspectLots = container.Q<ScrollView>("ip-lots");

            Button close = container.Q<Button>("ip-close");
            if (close != null) close.clicked += () => OnInspectCloseRequested?.Invoke();

            HideInspect();
        }

        public void ShowInspect(InspectPanelData data)
        {
            if (_inspectRoot == null || data?.Item == null) return;

            ItemDisplayData item = data.Item;

            _inspectTitle.text = "Inspeccionar";
            _inspectName.text = item.Name;
            _inspectType.text = item.TypeName;
            _inspectDescription.text = item.Description;
            UIElementUtils.SetBackgroundTexture(_inspectIcon, item.IconPath);

            _inspectAmount.text = item.Amount.ToString();
            _inspectWeightTotal.text = $"{item.TotalWeight:F1} kg";
            _inspectWeightUnit.text = $"{item.UnitWeight:F1} kg";
            _inspectDurability.text = $"{item.Durability:0} %";
            _inspectSize.text = $"{item.DimensionW} x {item.DimensionH}";

            // El desglose solo tiene sentido con mas de una variante.
            _inspectLots.Clear();
            bool hasLots = data.Lots != null && data.Lots.Count > 1;
            _inspectLotsSection.style.display = hasLots ? DisplayStyle.Flex : DisplayStyle.None;
            if (hasLots)
                foreach (ItemDisplayData lot in data.Lots) _inspectLots.Add(BuildLotRow(lot));

            _inspectRoot.style.display = DisplayStyle.Flex;

            // Modal: mientras se inspecciona, la capa captura toda la pantalla y nada de
            // debajo (paneles A/B) recibe el raton. Un clic fuera no hace nada; se cierra
            // con su X o con Esc. Igual que el radial mientras esta abierto.
            _inspectLayer.pickingMode = PickingMode.Position;
        }

        public void HideInspect()
        {
            if (_inspectRoot == null) return;
            _inspectRoot.style.display = DisplayStyle.None;
            _inspectLayer.pickingMode = PickingMode.Ignore;
        }

        /// <summary>Una fila del desglose, con las mismas clases que la fila de ejemplo del UXML.</summary>
        private static VisualElement BuildLotRow(ItemDisplayData lot)
        {
            VisualElement row = new VisualElement();
            row.AddToClassList("ip-lot-row");

            VisualElement icon = new VisualElement();
            icon.AddToClassList("ip-lot-icon");
            UIElementUtils.SetBackgroundTexture(icon, lot.IconPath);
            row.Add(icon);

            // La variante se distingue por su estado: nombre + durabilidad.
            Label name = new Label($"{lot.Name} ({lot.Durability:0} %)");
            name.AddToClassList("ip-lot-name");
            row.Add(name);

            Label amount = new Label($"x{lot.Amount}");
            amount.AddToClassList("ip-lot-amount");
            row.Add(amount);

            Label weight = new Label($"{lot.TotalWeight:F1} kg");
            weight.AddToClassList("ip-lot-weight");
            row.Add(weight);

            return row;
        }

        #endregion

        public void OpenMenu(IReadOnlyList<string> labels) => _radial?.Open(labels);

        public void CloseMenu() => _radial?.Close();
    }
}
