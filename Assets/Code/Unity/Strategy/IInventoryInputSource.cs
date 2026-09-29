using System;
using Core.MVC.View.UI.Inventory;
using MVC.View.Inventory;

public interface IInventoryInputSource
{
    event Action OnInventoryToggleRequested;
    event Action OnInventoryCancelRequested;
    event Action<PanelType> OnInventoryPanelToggleRequested;
}