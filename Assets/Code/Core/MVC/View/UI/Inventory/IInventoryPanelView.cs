using System;
using System.Collections.Generic;
using Core.Events;
using Core.Inventory;
using Core.MVC.Presenter.Inventory;

namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Lo que un presentador de panel necesita de la rejilla que pinta. Ver
    /// <see cref="IInventoryView"/>: mismo motivo, el presentador vive en Core.
    /// </summary>
    public interface IInventoryPanelView
    {
        #region Rejilla

        void GenerateGrid(int rows, int cols);
        void RenderGridItems(List<GridItemDisplayData> items);
        CellSize GetCellSize();

        /// <summary>Esquina superior derecha de un item en coordenadas del panel: ancla de menus.</summary>
        PanelPoint ItemTopRightCorner(GridPos origin, int w, int h);

        #endregion

        #region Peso y avisos

        void UpdateWeightStats(float currentWeight, float maxWeight, GameEventType eventType);
        void SetWeightNote(bool carrierBlocks);

        #endregion

        #region Puntero

        /// <summary>Vuelve a publicar lo que hay bajo el puntero (tras cambiar la mano).</summary>
        void RepublishHover();
        void RepublishHover(GrabPortion portion);

        event Action<GridPos> OnCellLeftPressed;
        event Action<GridPos, GrabPortion> OnCellPortionPressed;
        event Action<GridPos, bool> OnCellReleased;
        event Action<GridPos, PanelType> OnCellRightPressed;
        event Action<GridPos, CellSize, GrabPortion> OnPointerMovedOverCell;

        #endregion

        /// <summary>La X del panel. Quien lo cierra es el presentador.</summary>
        event Action OnCloseRequested;
    }
}
