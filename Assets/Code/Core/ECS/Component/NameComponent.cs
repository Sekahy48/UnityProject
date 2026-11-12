using System;
using System.Collections.Generic;

namespace Core.ECS.Component
{

    /// <summary>
    /// Component that stores an entity's display name.
    /// </summary>
    public class NameComponent : BasicComponent, IJsonLoadable
    {
        private string _displayName;

        public NameComponent() {}

        public NameComponent(string displayName)
        {
            _displayName = displayName;
            Name = "NameComponent";
        }

        public string DisplayName => _displayName;
        public void SetDisplayName(string value) { _displayName = value; }

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("displayName")) SetDisplayName(values["displayName"].ToString());
        }

        public override IComponent Clone()
        {
            return new NameComponent(_displayName);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is NameComponent otherName &&
                _displayName == otherName._displayName;
        }
    }
}