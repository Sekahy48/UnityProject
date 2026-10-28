using System.Collections.Generic;
using Core.MVC.View.UI.HUD;
using Core.MVC.View.UI.HUD.States;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static MVC.View.UIElementUtils;

namespace MVC.View.HUD
{
    /// <summary>
    /// Vista del HUD sobre PlayerHUD.uxml. Solo traduce proporciones a anchos de relleno.
    /// </summary>
    public class HUDView : IHUDView
    {
        private readonly UIDocument _document;
        private VisualElement _root;
        private VisualElement _staminaFill;
        private VisualElement _fatigueFill;
        private VisualElement _statesBar;
        private VisualElement _statesTooltip;
        private readonly string DEFAULT_STATUS_CLASS = "status";

        private Dictionary<StatusCategory, string> _stateClasses= new Dictionary<StatusCategory, string> {
            { StatusCategory.Weight, "status--weight"}
        };

        public HUDView(UIDocument document)
        {
            _document = document;
        }

        public void Initialize()
        {
            if (_document == null)
            {
                Debug.LogError("HUDView: falta el UIDocument del HUD en UIRegistry.");
                return;
            }

            _root = _document.rootVisualElement.Q<VisualElement>("hud-root");
            _staminaFill = _root?.Q<VisualElement>("stamina-bar")?.Q<VisualElement>("fill");
            _fatigueFill = _root?.Q<VisualElement>("fatigue-bar")?.Q<VisualElement>("fill");
            _statesBar = _root?.Q<VisualElement>("states-bar");
            _statesTooltip = _root?.Q<VisualElement>("states-tooltip");

            // El HUD va por encima del inventario (orden 4 en su PanelSettings) para que el
            // tooltip de estados se pueda leer con el inventario abierto. Ni la raiz que crea
            // Unity ni la capa capturan el raton: solo los iconos, asi que el resto de la
            // pantalla sigue llegando a los paneles de debajo.
            _document.rootVisualElement.pickingMode = PickingMode.Ignore;

            Hide();   // lo abre InputManager segun la camara
        }

        public void Show()
        {
            if (_root != null) _root.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            if (_root != null) _root.style.display = DisplayStyle.None;
        }

        public void SetStamina(float ratio) => Fill(_staminaFill, ratio);
        public void SetFatigue(float ratio) => Fill(_fatigueFill, ratio);

        private static void Fill(VisualElement fill, float ratio)
        {
            if (fill == null) return;
            fill.style.width = Length.Percent(Mathf.Clamp01(ratio) * 100f);
        }

        public void AddState(PlayerStatus state, string description)
        {
            string category = _stateClasses[state.Category];
            string stateClass = $"{category}-{state.Level}";

            VisualElement previousState = _statesBar.Q<VisualElement>(category);
            VisualElement newState = new VisualElement();

            if (state.Level == 0)
            {
                previousState?.parent.Remove(previousState);
                _statesTooltip.style.display = DisplayStyle.None;
                return;
            }

            if (previousState != null)
            {
                bool shake = !previousState.ClassListContains(stateClass);
                previousState.ClearClassList();
                previousState.AddToClassList(stateClass); 
                previousState.AddToClassList(DEFAULT_STATUS_CLASS);

                previousState.name = category; 
                previousState.userData = description;   // el texto del nivel nuevo

                // Si el tooltip esta abierto sobre este icono, que cambie con el nivel.
                if (_statesTooltip.resolvedStyle.display == DisplayStyle.Flex)
                {
                    _statesTooltip.Clear();
                    _statesTooltip.Add(TooltipText(description));
                }
                if (shake)
                    UIElementUtils.Shake(previousState);
            }
            else
            {  
                newState.AddToClassList(stateClass); 
                newState.AddToClassList(DEFAULT_STATUS_CLASS);

                newState.name = category;
                newState.userData = description;   // el texto viaja con su icono

                newState.RegisterCallback<PointerEnterEvent>(_ =>
                {
                    _statesTooltip.Clear();
                    _statesTooltip.Add(TooltipText((string)newState.userData));
                    UIElementUtils.PlaceNextTo(_statesTooltip, newState, _root, Side.Left);
                });

                newState.RegisterCallback<PointerLeaveEvent>(_ =>
                    _statesTooltip.style.display = DisplayStyle.None);
                _statesBar.Add(newState);
                UIElementUtils.Shake(newState);
                
            }  


            
        }

        /// <summary>
        /// Texto del tooltip con su clase BEM: el tema lo estiliza por clase, no por tipo.
        /// </summary>
        private static Label TooltipText(string text)
        {
            Label label = new Label(text);
            label.AddToClassList("tooltip__text");
            return label;
        }

        
    }
}
