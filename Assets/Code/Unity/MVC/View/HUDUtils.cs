using System;
using Strategy;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public class HUDUtils
    {
        private static HUDUtils _instance;
        private readonly UIDocument _uiDocument;
        private VisualElement _root;

        private HUDUtils()
        {
            Debug.Log("Initializing HUDUtils instance.");
            GameObject hud = GameObject.FindGameObjectWithTag("PlayerHUD");
            if (hud == null)
            {
                Debug.LogError("HUD GameObject with tag 'PlayerHUD' not found.");
                return;
            }
            
            _uiDocument = hud.GetComponent<UIDocument>();
            if (_uiDocument == null)
            {
                Debug.LogError("UIDocument component not found on HUD GameObject.");
                return;
            }
            _root = _uiDocument.rootVisualElement;
            if (_root == null)
            {
                Debug.LogError("Root VisualElement not found in UIDocument.");
                return;
            }
        }

        public static HUDUtils GetInstance()
        {
            if (_instance == null)
            {
                _instance = new HUDUtils();
            }
            return _instance;
        }

        public void ModifyFillable(String id, float percentage)
        {
            //Debug.Log($"Modifying fillable '{id}' to {percentage * 100}%");
            VisualElement fillable = _root.Q<VisualElement>(id).Q<VisualElement>("Fill");
            if (fillable == null)
            {
                Debug.LogError($"Fillable element with id '{id}' not found.");
                return;
            }
            fillable.style.width = new Length(percentage * 100, LengthUnit.Percent);

        }
        
        //public void SetCamera(Camera camera)
        //{
       //     this._uiDocument.panelSettings. = camera;
       // }
    }
}