namespace Core.ECS.Component.Interaction
{
    /// <summary>
    /// Caja alineada a los ejes locales de la entidad. Como la entidad lleva su propia
    /// rotacion, la caja gira con ella: el punto se lleva al espacio local antes de medir.
    ///
    /// <para>Es la forma que sirve para casi todo —arcon, caja, tabla, botella— y la que se
    /// obtiene gratis midiendo las envolventes de un modelo cargado. Aproximar un objeto
    /// irregular por su caja sobra para decidir si se alcanza: el hueco del respaldo de una
    /// silla no cambia la respuesta.</para>
    /// </summary>
    public class BoxVolume : InteractionVolume
    {
        private readonly float _cx, _cy, _cz;
        private readonly float _hx, _hy, _hz;

        /// <param name="cx">Centro en coordenadas locales de la entidad</param>
        /// <param name="hx">Semieje: la mitad del ancho total</param>
        public BoxVolume(float cx, float cy, float cz, float hx, float hy, float hz)
        {
            _cx = cx; _cy = cy; _cz = cz;
            _hx = hx; _hy = hy; _hz = hz;
        }

        /// <summary>
        /// Distancia punto-caja: se mide cuanto se sale el punto por cada eje, se descartan
        /// los ejes por los que no se sale, y se toma la longitud de lo que queda. Si no se
        /// sale por ninguno, el punto esta dentro y la distancia es cero.
        /// </summary>
        public override float DistanceFrom(float x, float y, float z)
        {
            float dx = Overshoot(x - _cx, _hx);
            float dy = Overshoot(y - _cy, _hy);
            float dz = Overshoot(z - _cz, _hz);

            return dx == 0f && dy == 0f && dz == 0f ? 0f : Length(dx, dy, dz);
        }

        /// <summary>Cuanto se pasa una coordenada del semieje, o cero si cae dentro.</summary>
        private static float Overshoot(float offset, float halfExtent)
        {
            float distance = (offset < 0f ? -offset : offset) - halfExtent;
            return distance > 0f ? distance : 0f;
        }

        public override InteractionVolume Clone() => this;

        public override bool Equivalent(InteractionVolume other)
        {
            return other is BoxVolume o
                && o._cx == _cx && o._cy == _cy && o._cz == _cz
                && o._hx == _hx && o._hy == _hy && o._hz == _hz;
        }
    }
}
