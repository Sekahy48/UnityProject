using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.Events;
using Core.MVC.Model;
using Core.Observer;
using AC = Core.Utils.ArgumentChecker;

namespace Core.ECS.Systems
{
    public class TaskSystem : IEventObserver
    {
        private readonly Logic _logic;

        public TaskSystem(Logic logic)
        {
            AC.CheckNotNull(logic, nameof(logic));
            _logic = logic;
        }

        public void UpdateOnEvent(GameEvent gameEvent)
        {
            AC.CheckNotNull(gameEvent, nameof(gameEvent));

            List<IEntity> entitiesWithTasks = _logic.GetEntitiesWithComponent(typeof(TaskComponent));

            foreach (var entity in entitiesWithTasks)
            {
                var taskComponent = entity.GetComponent<TaskComponent>();
                if (taskComponent != null)
                    ExecuteTaskComponent(entity, taskComponent);
            }
        }

        private void ExecuteTaskComponent(IEntity entity, TaskComponent taskComponent)
        {
            if (taskComponent == null)
                throw new ArgumentNullException(nameof(taskComponent),
                    "Entity does not have TaskComponent. Unexpected behavior.");

            if (taskComponent.CurrentTask == null)
                taskComponent.SetCurrentTask(taskComponent.PollNextTask());

            if (taskComponent.CurrentTask != null)
            {
                taskComponent.CurrentTask.Execute(entity);

                if (taskComponent.CurrentTask.IsCompleted(entity))
                {
                    taskComponent.RemoveTask(taskComponent.CurrentTask);
                    taskComponent.SetCurrentTask(null);
                }
            }
        }
    }
}
