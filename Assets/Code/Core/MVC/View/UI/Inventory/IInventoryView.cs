using System;
using System.Collections.Generic;
using Core.ECS.Component.Equipment;
using Core.Services;

namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Lo que el presentador del inventario necesita de su vista.
    ///
    /// <para>Interfaz en Core, igual que <c>IWorldInteractionView</c>: el presentador vive en
    /// Core y no puede nombrar la clase de Unity. La vista concreta sabe de UI Toolkit; el
    /// presentador solo de datos (DTOs, enums, tamanos) y de eventos.</para>
    /// </summary>
    public interface IInventoryView : IView
    {
        #region Ciclo de vida y visibilidad

        /// <summary>La vista termino de montarse y ya se le puede pedir cosas.</summary>
        event Action OnReady;
        bool IsReady();

        /// <summary>Muestra el inventario principal (los huecos laterales van aparte).</summary>
        void Show();
        void Hide();

        /// <summary>Si el documento captura el raton a pantalla completa. Lo decide el presentador.</summary>
        void SetScreenCapture(bool capture);

        /// <summary>Cierra menu contextual, popup de capas y popup de sub-lotes.</summary>
        void DismissOverlays();

        /// <summary>Que muestra un hueco lateral. El estado lo guarda el presentador.</summary>
        void ShowSideContent(PanelType slot, SidePanelContent content);

        /// <summary>La rejilla de un panel.</summary>
        IInventoryPanelView GetPanel(PanelType type);

        event Action OnCloseClicked;
        event Action OnCancelRequested;
        event Action OnCatalogToggleRequested;

        #endregion

        #region Mano

        void RenderHandBuffer(ItemDisplayData itemData, CellSize itemSize, CellSize anchorBasis);
        void UpdateHandDisplay(PlacementVerdict verdict, CellSize itemSize, CellSize anchorBasis);
        void ClearHandBuffer();

        /// <summary>Se solto fuera de cualquier rejilla o slot.</summary>
        event Action OnReleasedOutsideGrid;

        #endregion

        #region Equipo y capas

        void UpdateEquipmentSlots(EquipmentComponent equipment);
        CellSize GetEquipmentCellSize();

        /// <summary>
        /// Tipo del slot de equipo que origino el gesto en curso. Antes el presentador pedia
        /// el VisualElement y lo devolvia a la vista para traducirlo; ahora la vista lo
        /// traduce sola y a Core solo llega el enum.
        /// </summary>
        EquipmentSlotType ActiveEquipmentSlotType(bool fromLayersPopup);

        bool IsLayersPopupOpen { get; }
        EquipmentSlotType OpenLayersSlotType { get; }
        void RenderLayers(List<ItemDisplayData> layers);
        void CloseLayersPopup();

        event Action<EquipmentSlotType> OnSlotLayersRequested;
        event Action<int, bool> OnEquipmentSlotRightClicked;
        event Action<int, bool> OnLayerLeftPressed;
        event Action<int, bool, bool> OnLayerLeftReleased;
        event Action<int, bool, CellSize> OnPointerMovedOverSlot;
        event Action OnPointerLeftSlot;

        #endregion

        #region Pestanas de inventario

        void ClearInveotryTabs();
        void AddTabToInventoryTabs(string name, Action clickEvent, List<MenuOption> options);
        void RenderInventoryTabs(Action onHide);

        #endregion

        #region Menus y popups

        void RenderContextualMenu(List<MenuOption> options);
        void CloseContextualMenu();
        event Action OnContextualMenuClosed;

        void RenderSublotsPopup(IReadOnlyList<ItemDisplayData> sublots, PanelPoint anchor,
                                Action<int> onLeftClicked, Action<int> onRightClicked);

        #endregion

        #region Franja de inspeccion

        void UpdateInspectionStrip(ItemDisplayData item);
        event Action<ItemDisplayData> OnInspectionHovered;

        #endregion

        #region Catalogo de desarrollo

        void FillItemCatalog(List<ItemDisplayData> items);
        event Action<int, int> OnCatalogItemGrabbed;

        #endregion
    }
}
