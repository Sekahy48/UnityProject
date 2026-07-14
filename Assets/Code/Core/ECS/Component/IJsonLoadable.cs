using System.Collections.Generic;

namespace ECS.Component
{
    /// <summary>
    /// Implemented by components that can be loaded from JSON key-value pairs.
    /// Used by the catalog loader to populate components from Stack&Go exports.
    /// </summary>
    public interface IJsonLoadable
    {
        void SetFromValues(Dictionary<string, object> values);
    }
}
