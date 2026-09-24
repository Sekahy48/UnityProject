using UnityEngine;

namespace Unity
{
    /// <summary>
    /// La camara con la que se esta viendo el juego ahora mismo.
    ///
    /// <para>Existe porque proyectar un punto del mundo a la pantalla depende del punto de
    /// vista: la misma manzana cae en un sitio distinto de la pantalla en primera y en
    /// tercera persona. Y no hay <c>Camera.main</c> que valga: <c>CameraRegister</c> apaga
    /// la camara de la escena y cada estrategia crea la suya sin etiqueta.</para>
    ///
    /// <para>Interfaz con nombre y no un <c>Func&lt;Camera&gt;</c>, como <c>IReachFilter</c>.
    /// Solo la implementa <c>CameraRegister</c>.</para>
    /// </summary>
    public interface IActiveCameraSource
    {
        /// <summary>La camara activa, o null si no hay ninguna.</summary>
        Camera Current { get; }
    }
}
