using System.Collections.Generic;
using ECS.Component.Task;

namespace ECS.Component
{
    public class TaskComponent : BasicComponent
    {
        private readonly LinkedList<ITask> tasks = new LinkedList<ITask>();
        private ITask currentTask = null;

        public TaskComponent()
        {
            this.name = "TaskComponent"; // Inicializa el nombre del componente
        }

        public void AddTask(ITask task)
        {
            tasks.AddLast(task); // Equivalente a Queue.add()
        }

        public void PushTask(ITask task)
        {
            tasks.AddFirst(task); // Añade al principio
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

            ITask first = tasks.First.Value;
            tasks.RemoveFirst();
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
    }
}
