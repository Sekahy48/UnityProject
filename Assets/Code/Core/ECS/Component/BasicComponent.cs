using System;
using Observer;

namespace ECS.Component
{
    public abstract class BasicComponent : GenericSubject, IComponent
    {
        protected String _name;
 

        public abstract IComponent Clone();
        public abstract bool Equivalent(IComponent other);
    }
}
