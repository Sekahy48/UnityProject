using Observer;

namespace ECS.Component
{
    public abstract class BasicComponent : GenericSubject, IComponent
    {
        protected string name;
 

        public abstract IComponent Clone();
    }
}
