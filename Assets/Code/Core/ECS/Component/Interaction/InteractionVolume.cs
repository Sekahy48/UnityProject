namespace Core.ECS.Component.Interaction
{
    /// <summary>
    /// El espacio que ocupa una entidad a efectos de alcanzarla.
    ///
    /// <para>No es su forma real ni pretende serlo. Para decidir si el jugador llega a algo
    /// basta con una aproximacion: los huecos y los detalles de la malla no cambian la
    /// respuesta, y en cambio pagarlos costaria tener geometria de colision por objeto.</para>
    ///
    /// <para>Existe como jerarquia y no como un enum con un <c>switch</c> porque la pregunta
    /// que se hace es siempre la misma —"a que distancia estoy de ti?"— y cada forma sabe
    /// contestarla a su manera. Quien busca objetivos no tiene por que saber si esta mirando
    /// una caja o una capsula.</para>
    ///
    /// <para>Las coordenadas son <b>locales a la entidad</b>. Como el origen de nuestros
    /// modelos esta en la base, un volumen que represente al objeto entero tendra su centro
    /// por encima de cero.</para>
    /// </summary>
    public abstract class InteractionVolume
    {
        /// <summary>
        /// Distancia desde un punto hasta la superficie del volumen, en coordenadas locales
        /// de la entidad. Cero si el punto esta dentro.
        /// </summary>
        public abstract float DistanceFrom(float x, float y, float z);

        /// <summary>
        /// Punto mas alto del volumen sobre su eje, en coordenadas locales de la entidad.
        ///
        /// <para>Es donde se ancla lo que se pinta encima de la entidad —la E de interactuar—.
        /// Se pide al volumen y no se mide aparte del modelo porque el volumen ya ES la medida
        /// del modelo (el linker lo ajusta a sus envolventes); una segunda altura podria
        /// discrepar de la primera.</para>
        /// </summary>
        public abstract (float x, float y, float z) TopPoint();

        /// <summary>
        /// Centro del volumen, en coordenadas locales. Es a donde se apunta: el origen de la
        /// entidad esta en su base, y apuntar al centro de un arcon es apuntar al arcon.
        /// </summary>
        public abstract (float x, float y, float z) CenterPoint();

        /// <summary>
        /// Copia el volumen. Los componentes se clonan por entidad y un volumen es inmutable
        /// una vez creado, asi que las implementaciones pueden devolverse a si mismas.
        /// </summary>
        public abstract InteractionVolume Clone();

        public abstract bool Equivalent(InteractionVolume other);

        /// <summary>Raiz cuadrada sin arrastrar System.Math a cada implementacion.</summary>
        protected static float Length(float x, float y, float z)
        {
            return (float)System.Math.Sqrt(x * x + y * y + z * z);
        }
    }
}
