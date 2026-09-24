namespace Core.ECS.Component.Interaction
{
    /// <summary>
    /// Volumen esferico. Es el que se le da a una entidad que todavia no ha medido su
    /// modelo: dice "estoy aqui y ocupo mas o menos esto" sin necesitar ningun dato.
    /// </summary>
    public class SphereVolume : InteractionVolume
    {
        private readonly float _cx, _cy, _cz;
        private readonly float _radius;

        public SphereVolume(float radius) : this(0f, radius, 0f, radius) {}

        /// <param name="cx">Centro en coordenadas locales de la entidad</param>
        public SphereVolume(float cx, float cy, float cz, float radius)
        {
            _cx = cx; _cy = cy; _cz = cz;
            _radius = radius;
        }

        public float Radius => _radius;

        public override float DistanceFrom(float x, float y, float z)
        {
            float distance = Length(x - _cx, y - _cy, z - _cz) - _radius;
            return distance > 0f ? distance : 0f;
        }

        public override InteractionVolume Clone() => this;

        public override bool Equivalent(InteractionVolume other)
        {
            return other is SphereVolume o
                && o._cx == _cx && o._cy == _cy && o._cz == _cz && o._radius == _radius;
        }
    }
}
