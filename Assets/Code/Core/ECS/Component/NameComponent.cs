namespace ECS.Component
{

    /// <summary>
    /// Component that stores an entity's display name.
    /// </summary>
    public class NameComponent : BasicComponent
    {
        private readonly string displayName;

        public NameComponent(string displayName)
        {
            this.displayName = displayName;
            this._name = "NameComponent"; // Initializes the component name
        }

        public string DisplayName => displayName;

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