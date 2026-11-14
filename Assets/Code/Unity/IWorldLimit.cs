using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Donde acaba el mundo valido (Fase 1.5, M4; decision 46). Interfaz para poder cambiar como
    /// se calcula sin tocar quien la usa: hoy una altura fija; con varios mapas, calculada de la
    /// escena.
    /// </summary>
    public interface IWorldLimit
    {
        bool IsOutside(Vector3 position);
    }
}
