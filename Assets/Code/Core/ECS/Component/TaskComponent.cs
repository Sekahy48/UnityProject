using System.Collections.Generic;
using System.Linq;
using ECS.Component.Task;

namespace ECS.Component
{
    public class TaskComponent : BasicComponent
    {
        private readonly List<ITask> tasks = new List<ITask>();
        private ITask currentTask = null;

        public TaskComponent()
        {
            this._name = "TaskComponent"; // Inicializa el nombre del componente
        }

        public void AddTask(ITask task)
        {
            tasks.Add(task); 
        }

        public void PushTask(ITask task)
        {
            tasks.Insert(0, task); // Añade al principio
        }

        public void RemoveTask(ITask task)
        {
            tasks.Remove(task);
        }

        public void ClearTasks()
        {
            currentTask = null;
            tasks.Clear();
        }

        public bool HasTasks()
        {
            return tasks.Count > 0 || currentTask != null;
        }

        public ITask GetCurrentTask()
        {
            return currentTask;
        }

        public void SetCurrentTask(ITask task)
        {
            currentTask = task;
        }

        public List<ITask> GetAllTasks()
        {
            return new List<ITask>(tasks);
        }

        public ITask PollNextTask()
        {
            if (tasks.Count == 0)
                return null;

            ITask first = tasks.First();
            tasks.RemoveAt(0);
            return first;
        }

        public override IComponent Clone()
        {
            TaskComponent copy = new TaskComponent();

            if (currentTask != null)
                copy.SetCurrentTask(currentTask.Clone());

            foreach (ITask task in tasks)
                copy.AddTask(task.Clone());

            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is not TaskComponent otherTaskComponent) return false;

            if (this.currentTask == null != (otherTaskComponent.currentTask == null)) return false;
            if (this.currentTask != null && !this.currentTask.Equivalent(otherTaskComponent.currentTask)) return false;
            if (this.tasks.Count != otherTaskComponent.tasks.Count) return false;

            for (int i = 0; i < this.tasks.Count; i++)
            {
                if (!this.tasks.ElementAt(i).Equivalent(otherTaskComponent.tasks.ElementAt(i)))
                    return false;
            }

            return true;
        }
    }
}
