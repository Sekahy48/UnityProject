using Core.ECS.Entity;
using Core.Observer;

namespace Core.ECS.Component.Task
{
    public interface ITask : IObserver, ISubject
    {
        public void Execute(IEntity entity);         
        public bool IsCompleted(IEntity entity);  
        public string GetDescription();                 
        public void Update(IEntity entity, float delta);  
        public TaskState GetState();                   
        public ITask Clone(); 
        
        public bool Equivalent(ITask other); 
    }
}
