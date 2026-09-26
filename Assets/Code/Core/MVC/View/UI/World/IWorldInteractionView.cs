using System;
using System.Collections.Generic;
using Core.MVC.View;

namespace Core.MVC.View.UI.World
{
    /// <summary>
    /// Lo que el presentador de interaccion con el mundo necesita de su vista.
    ///
    /// <para>Interfaz en Core y no la clase de Unity directamente: el presentador vive en
    /// Core y no puede nombrar nada que dependa del motor. La vista concreta sabe de
    /// UI Toolkit y de camaras; el presentador solo sabe que hay algo que pinta una marca en
    /// un punto del mundo.</para>
    /// </summary>
    public interface IWorldInteractionView : IView
    {
        /// <summary>Muestra la marca con este contenido. Se llama solo cuando cambia.</summary>
        void ShowPrompt(WorldPromptData data);

        /// <summary>
        /// Coloca la marca sobre este punto del mundo. Se llama cada fotograma mientras se
        /// muestra, porque la camara se mueve aunque el objetivo no. Proyectar a pantalla es
        /// cosa de la vista: es la que conoce la camara.
        /// </summary>
        void PlacePrompt(float worldX, float worldY, float worldZ);

        void HidePrompt();

        /// <summary>
        /// Si se esta atendiendo al mundo: con camara de avatar y el inventario cerrado. La
        /// vista muestra u oculta lo que solo tiene sentido entonces, como el punto de mira.
        /// </summary>
        void SetAttending(bool attending);

        #region Menu radial

        /// <summary>
        /// Abre el menu radial con estas opciones, la primera arriba y el resto en sentido
        /// horario. Ninguna resaltada al abrir: el puntero empieza en el centro.
        /// </summary>
        void OpenMenu(IReadOnlyList<string> labels);

        void CloseMenu();

        /// <summary>El puntero entro en la porcion de la opcion i, o en ninguna (-1).</summary>
        event Action<int> OnMenuHighlighted;

        /// <summary>Clic sobre la porcion de la opcion i.</summary>
        event Action<int> OnMenuClicked;

        /// <summary>Clic fuera de toda opcion (centro o lejos): cerrar sin hacer nada.</summary>
        event Action OnMenuDismissed;

        #endregion

        #region Panel de inspeccion

        void ShowInspect(InspectPanelData data);

        void HideInspect();

        /// <summary>El jugador pidio cerrar el panel desde el propio panel (la X).</summary>
        event Action OnInspectCloseRequested;

        #endregion
    }
}
