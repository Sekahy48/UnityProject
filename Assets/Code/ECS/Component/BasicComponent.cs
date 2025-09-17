namespace ECS.Component
{
    public abstract class BasicComponent : IComponent
    {
        protected string name;
 

        public abstract IComponent Clone();
    }
}
