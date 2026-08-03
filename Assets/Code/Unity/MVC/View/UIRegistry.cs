using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MVC.View
{
    public class UIRegistry : MonoBehaviour
    {
        [SerializeField] private UIDocument _inventoryDocument;
        [SerializeField] private VisualTreeAsset _inventoryTabTemplate;
        [SerializeField] private VisualTreeAsset _inventoryItemTemplate;

        [SerializeField] private VisualTreeAsset _equipmentPanelTemplate;
        // en el futuro: crafting, equipment, hud...

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
                UITemplateType.InventoryTab  => _inventoryTabTemplate,
                UITemplateType.InventoryItem => _inventoryItemTemplate,
                UITemplateType.EquipmentPanel => _equipmentPanelTemplate,
                _ => throw new ArgumentException($"Unknown template type: {type}")
            };
        }
    }

    public enum UIDocumentType  { Inventory }
    public enum UITemplateType  { InventoryTab, InventoryItem, EquipmentPanel }
}