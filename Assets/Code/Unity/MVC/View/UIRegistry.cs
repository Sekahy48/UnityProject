using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public class UIRegistry : MonoBehaviour
    {
        [SerializeField] private UIDocument _inventoryDocument; 
        [SerializeField] private VisualTreeAsset _inventoryPanelTemplate; 
        [SerializeField] private UIDocument _worldInteractionDocument;
        [SerializeField] private VisualTreeAsset _inspectPanelTemplate;

        public UIDocument GetDocument(UIDocumentType type)
        {
            return type switch
            {
                UIDocumentType.Inventory => _inventoryDocument,
                UIDocumentType.WorldInteraction => _worldInteractionDocument,
                _ => throw new ArgumentException($"Unknown document type: {type}")
            };
        } 

        public VisualTreeAsset GetTemplate(UITemplateType type)
        {
            return type switch
            {
                UITemplateType.InventoryPanel => _inventoryPanelTemplate,
                UITemplateType.InspectPanel => _inspectPanelTemplate,
                _ => throw new ArgumentException($"Unknown template type: {type}")
            };
        }
    }

    public enum UIDocumentType  { Inventory, WorldInteraction } 

    public enum UITemplateType { InventoryPanel, InspectPanel }

}