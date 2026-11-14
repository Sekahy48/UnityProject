using System;
using Core.Observer;

namespace Core.ECS.Component
{
    public abstract class BasicComponent : GenericSubject, IComponent
    {
        /* Nombre del tipo de componente, para depurar. */
        protected string Name { get; set; }
 

        public abstract IComponent Clone();
        public abstract bool Equivalent(IComponent other);
    }
}
