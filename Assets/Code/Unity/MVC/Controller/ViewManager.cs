using System.Collections.Generic;
using MVC.Presenter;
using MVC.View;

namespace MVC.Controller
{
    public class ViewManager
    {
        private Dictionary<PresenterType, IView> views;
    
        public ViewManager()
        {
            views = new Dictionary<PresenterType, IView>();
        }

        public void InitializeViews(UIRegistry uiRegistry)
        {
             // Inventory View
             views[PresenterType.INV] = new InventoryView(  uiRegistry.GetDocument(UIDocumentType.Inventory),
                                                            uiRegistry.GetTemplate(UITemplateType.InventoryItem));
        }

        public T GetView<T>(PresenterType type) where T : IView
        {
            if (views.TryGetValue(type, out IView view))
                return (T)view;
            return default;
        }
    }

}