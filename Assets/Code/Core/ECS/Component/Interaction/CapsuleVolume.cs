namespace Core.ECS.Component.Interaction
{
    /// <summary>
    /// Segmento con grosor. Es la forma de un tronco, una lanza o una espada tirada en el
    /// suelo: cosas mucho mas largas que anchas, donde una caja dejaria esquinas vacias a
    /// las que el jugador podria "alcanzar" sin estar cerca de nada.
    ///
    /// <para>Se define con sus dos extremos en coordenadas locales, asi que vale para
    /// cualquier orientacion sin necesitar un eje aparte.</para>
    ///
    /// <para>No hace falta para empezar: la caja cubre casi todo. Existe para el dia en que
    /// se note que un objeto largo se comporta raro.</para>
    /// </summary>
    public class CapsuleVolume : InteractionVolume
    {
        private readonly float _ax, _ay, _az;
        private readonly float _bx, _by, _bz;
        private readonly float _radius;

        public CapsuleVolume(float ax, float ay, float az, float bx, float by, float bz, float radius)
        {
            _ax = ax; _ay = ay; _az = az;
            _bx = bx; _by = by; _bz = bz;
            _radius = radius;
        }

        /// <summary>
        /// Distancia punto-segmento menos el radio. El punto se proyecta sobre la recta que
        /// pasa por los extremos, el parametro resultante se recorta al tramo [0,1] —de ahi
        /// salen las tapas redondeadas— y se mide contra ese punto.
        /// </summary>
        public override float DistanceFrom(float x, float y, float z)
        {
            float abx = _bx - _ax, aby = _by - _ay, abz = _bz - _az;
            float apx = x - _ax, apy = y - _ay, apz = z - _az;

            float lengthSquared = abx * abx + aby * aby + abz * abz;

            // Los dos extremos coinciden: la capsula degenera en una esfera.
            float t = lengthSquared <= 0f ? 0f : (apx * abx + apy * aby + apz * abz) / lengthSquared;

            if (t < 0f) t = 0f;
            else if (t > 1f) t = 1f;

            float distance = Length(apx - abx * t, apy - aby * t, apz - abz * t) - _radius;
            return distance > 0f ? distance : 0f;
        }

        public override InteractionVolume Clone() => this;

        public override bool Equivalent(InteractionVolume other)
        {
            return other is CapsuleVolume o
                && o._ax == _ax && o._ay == _ay && o._az == _az
                && o._bx == _bx && o._by == _by && o._bz == _bz
                && o._radius == _radius;
        }
    }
}
