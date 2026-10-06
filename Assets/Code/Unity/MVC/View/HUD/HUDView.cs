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

            _root = _document.rootVisualElement.Q<VisualElement>("Container");
            _staminaFill = _root?.Q<VisualElement>("StaminaBar")?.Q<VisualElement>("Fill");
            _fatigueFill = _root?.Q<VisualElement>("FatigueBar")?.Q<VisualElement>("Fill");
            _statesBar = _root?.Q<VisualElement>("StatesBar");
            _statesTooltip = _root?.Q<VisualElement>("states-tooltip");

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
            _statesTooltip.Clear();

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
                    _statesTooltip.Add(new Label((string)newState.userData));
                    UIElementUtils.PlaceNextTo(_statesTooltip, newState, _root, Side.Left);
                });

                newState.RegisterCallback<PointerLeaveEvent>(_ =>
                    _statesTooltip.style.display = DisplayStyle.None);
                _statesBar.Add(newState);
                UIElementUtils.Shake(newState);
                
            }  

            _statesTooltip.Add(new Label (description)); 

            
        }

        
    }
}
