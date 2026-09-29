using Core.ECS.Entity;
using Core.MVC.View.UI.Inventory;

namespace Core.MVC.Presenter
{
    public interface IContainerPanels
    {
        /// <summary>
        /// Devuelve la entidad que ocupa el panel con el tipo proporcionado al metodo.
        /// </summary>
        /// <param name="slot"> Tipo de panel a comprobar. </param>
        /// <returns> Entidad que ocupa el panel o null. </returns>
        IEntity OccupantOf(PanelType slot);

        /// <summary>
        /// Responde a si algun panel ya esta mostrando el inventario de la entidad proporcionada por parametros.
        /// </summary>
        /// <param name="container"> Entidad a comprobar. </param>
        /// <returns> True si algun panel ya lo muestra, false si no se muestra en ninguno. </returns>
        bool IsShowing(IEntity container);

        /// <summary>
        /// Abre el inventario de la entidad proporcionada en el panel cuyo tipo es el proporcionado.
        /// </summary>
        /// <param name="slot"> Tipo del panel sobre el que mostrar el inventario de la entidad.</param>
        /// <param name="container"> Entidad cuyo inventario se desea mostrar. </param>
        void OpenPanel(PanelType slot, IEntity container);

        /// <summary>
        /// Oculta el panel cuyo tipo es el proporcionado.
        /// </summary>
        /// <param name="slot"> Tipo del panel a ocultar.  </param>
        void ClosePanel(PanelType slot);
    }
}