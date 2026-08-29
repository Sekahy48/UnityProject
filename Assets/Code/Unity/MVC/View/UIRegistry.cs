using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public class UIRegistry : MonoBehaviour
    {
        [SerializeField] private UIDocument _inventoryDocument; 
        [SerializeField] private VisualTreeAsset _inventoryPanelTemplate; 

        public UIDocument GetDocument(UIDocumentType type)
        {
            return type switch
            {
                UIDocumentType.Inventory => _inventoryDocument,
                _ => throw new ArgumentException($"Unknown document type: {type}")
            };
        } 

        public VisualTreeAsset GetTemplate(UITemplateType type)
        {
            return type switch
            {
                UITemplateType.InventoryPanel => _inventoryPanelTemplate,
                _ => throw new ArgumentException($"Unknown template type: {type}")
            };
        }
    }

    public enum UIDocumentType  { Inventory } 

    public enum UITemplateType { InventoryPanel }

}