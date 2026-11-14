using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Fuera del mundo = por debajo de una altura fija. Basta con un solo mapa de pruebas.
    /// Caer desde una torre acaba en el suelo, por encima del limite; solo lo cruza lo que
    /// atraviesa el suelo o cae por un borde.
    /// </summary>
    public class FixedHeightWorldLimit : IWorldLimit
    {
        private readonly float _minY;

        public FixedHeightWorldLimit(float minY) => _minY = minY;

        public bool IsOutside(Vector3 position) => position.y < _minY;
    }
}
