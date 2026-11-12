using System.Collections.Generic;
using System.Linq;
using Core.ECS.Component.Task;

namespace Core.ECS.Component
{
    public class TaskComponent : BasicComponent
    {
        private readonly List<ITask> _tasks = new List<ITask>();
        private ITask _currentTask = null;

        public TaskComponent()
        {
            Name = "TaskComponent"; // Initializes the component name
        }

        public void AddTask(ITask task)
        {
            _tasks.Add(task); 
        }

        public void PushTask(ITask task)
        {
            _tasks.Insert(0, task); // Adds to the front
        }

        public void RemoveTask(ITask task)
        {
            _tasks.Remove(task);
        }

        public void ClearTasks()
        {
            _currentTask = null;
            _tasks.Clear();
        }

        public bool HasTasks()
        {
            return _tasks.Count > 0 || _currentTask != null;
        }

        public ITask CurrentTask => _currentTask;

        public void SetCurrentTask(ITask task)
        {
            _currentTask = task;
        }

        public List<ITask> GetAllTasks()
        {
            return new List<ITask>(_tasks);
        }

        public ITask PollNextTask()
        {
            if (_tasks.Count == 0)
                return null;

            ITask first = _tasks.First();
            _tasks.RemoveAt(0);
            return first;
        }

        public override IComponent Clone()
        {
            TaskComponent copy = new TaskComponent();

            if (_currentTask != null)
                copy.SetCurrentTask(_currentTask.Clone());

            foreach (ITask task in _tasks)
                copy.AddTask(task.Clone());

            return copy;
        }

        public override bool Equivalent(IComponent other)
        {
            if (other is not TaskComponent otherTaskComponent) return false;

            if (_currentTask == null != (otherTaskComponent._currentTask == null)) return false;
            if (_currentTask != null && !_currentTask.Equivalent(otherTaskComponent._currentTask)) return false;
            if (_tasks.Count != otherTaskComponent._tasks.Count) return false;

            for (int i = 0; i < _tasks.Count; i++)
            {
                if (!_tasks.ElementAt(i).Equivalent(otherTaskComponent._tasks.ElementAt(i)))
                    return false;
            }

            return true;
        }
    }
}
