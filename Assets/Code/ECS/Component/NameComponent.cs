namespace ECS.Component
{

    /// <summary>
    /// Componente que almacena el nombre para mostrar de una entidad.
    /// </summary>
    public class NameComponent : BasicComponent
    {
        private readonly string displayName;

        public NameComponent(string displayName)
        {
            this.displayName = displayName;
            this._name = "NameComponent"; // Inicializa el nombre del componente
        }

        public string GetDisplayName()
        {
            return displayName;
        }

        public override IComponent Clone()
        {
            return new NameComponent(displayName);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is NameComponent otherName &&
                this.displayName == otherName.displayName;
        }
    }
}