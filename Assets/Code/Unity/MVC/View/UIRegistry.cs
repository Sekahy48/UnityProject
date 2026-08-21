using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public class UIRegistry : MonoBehaviour
    {
        [SerializeField] private UIDocument _inventoryDocument; 
        // en el futuro: crafting, equipment, hud...

        public UIDocument GetDocument(UIDocumentType type)
        {
            return type switch
            {
                UIDocumentType.Inventory => _inventoryDocument,
                _ => throw new ArgumentException($"Unknown document type: {type}")
            };
        } 
    }

    public enum UIDocumentType  { Inventory } 
}