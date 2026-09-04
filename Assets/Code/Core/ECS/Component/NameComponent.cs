using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{

    /// <summary>
    /// Component that stores an entity's display name.
    /// </summary>
    public class NameComponent : BasicComponent, IJsonLoadable
    {
        private string displayName;

        public NameComponent() {}

        public NameComponent(string displayName)
        {
            this.displayName = displayName;
            this._name = "NameComponent";
        }

        public string DisplayName => displayName;
        public void SetDisplayName(string value) { displayName = value; }

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("displayName")) SetDisplayName(values["displayName"].ToString());
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