namespace Core.ECS.Component.Interaction
{
    /// <summary>
    /// Marca a una entidad como alcanzable y dice que espacio ocupa a esos efectos.
    ///
    /// <para>Llevarlo es lo que hace que una entidad entre en la busqueda de objetivos: sin
    /// este componente, existe en el mundo pero no se le puede apuntar. Eso deja la
    /// distincion entre decorado e interactuable en un solo sitio y sin banderas.</para>
    ///
    /// <para><b>De donde salen los numeros.</b> La entidad nace con un volumen por defecto
    /// de su arquetipo —una esfera pequena, suficiente para que se le pueda apuntar desde el
    /// primer fotograma— y quien enlaza el modelo lo sustituye por la caja medida de sus
    /// mallas cuando termina de cargar. No se declaran a mano en el catalogo: el volumen ya
    /// esta en la geometria del modelo, y guardarlo aparte seria tener el mismo hecho dos
    /// veces, con la garantia de que un dia el modelo cambie y el numero se quede
    /// mintiendo.</para>
    /// </summary>
    public class InteractionVolumeComponent : BasicComponent
    {
        private InteractionVolume _volume;

        public InteractionVolumeComponent() : this(new SphereVolume(0.25f)) {}

        public InteractionVolumeComponent(InteractionVolume volume)
        {
            _volume = volume ?? new SphereVolume(0.25f);
            _name = "InteractionVolumeComponent";
        }

        public InteractionVolume Volume => _volume;

        /// <summary>
        /// Sustituye el volumen. Lo llama quien mide el modelo una vez cargado.
        /// </summary>
        public void SetVolume(InteractionVolume volume)
        {
            if (volume != null) _volume = volume;
        }

        /// <summary>
        /// Distancia desde un punto del mundo hasta el volumen de esta entidad.
        ///
        /// <para>El punto se lleva al espacio local de la entidad antes de medir, asi que el
        /// volumen gira y se traslada con ella sin que nadie tenga que rehacerlo.</para>
        /// </summary>
        /// <param name="owner">Posicion de la entidad que lleva este volumen</param>
        /// <returns>Distancia a la superficie, cero si el punto esta dentro. Si la entidad
        /// no tiene posicion no hay con que comparar y devuelve infinito, que la deja fuera
        /// de cualquier alcance sin necesitar un caso aparte.</returns>
        public float DistanceFrom(PositionComponent owner, float x, float y, float z)
        {
            if (owner == null) return float.MaxValue;

            (float lx, float ly, float lz) = owner.WorldToLocal(x, y, z);
            return _volume.DistanceFrom(lx, ly, lz);
        }

        public override IComponent Clone()
        {
            return new InteractionVolumeComponent(_volume.Clone());
        }

        public override bool Equivalent(IComponent other)
        {
            return other is InteractionVolumeComponent o && _volume.Equivalent(o._volume);
        }
    }
}
