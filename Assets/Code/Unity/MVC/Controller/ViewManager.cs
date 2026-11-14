using System.Collections.Generic;
using Core.MVC.Presenter;
using Core.MVC.View;
using MVC.View;
using MVC.View.HUD;
using MVC.View.Inventory;
using MVC.View.World;

namespace MVC.Controller
{
    public class ViewManager
    {
        private Dictionary<PresenterType, IView> _views;
    
        public ViewManager()
        {
            _views = new Dictionary<PresenterType, IView>();
        }

        public void InitializeViews(UIRegistry uiRegistry, Unity.IActiveCameraSource cameras)
        {
             // Inventory View
             _views[PresenterType.INV] = new InventoryView(uiRegistry.GetDocument(UIDocumentType.Inventory),
                                                          uiRegistry.GetTemplate(UITemplateType.InventoryPanel));

             _views[PresenterType.WORLD] = new WorldInteractionView(uiRegistry.GetDocument(UIDocumentType.WorldInteraction),
                                                                   cameras,
                                                                   uiRegistry.GetTemplate(UITemplateType.InspectPanel));

             _views[PresenterType.HUD] = new HUDView(uiRegistry.GetDocument(UIDocumentType.HUD));
        }

        public T GetView<T>(PresenterType type) where T : IView
        {
            if (_views.TryGetValue(type, out IView view))
                return (T)view;
            return default;
        }
    }

}