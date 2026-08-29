using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using Events;
using MVC.Model;
using Observer;

namespace ECS.Systems
{
    public class TaskSystem : IEventObserver
    {
        private readonly Logic logic;

        public TaskSystem(Logic logic)
        {
            this.logic = logic;
        }

        public void UpdateOnEvent(GameEvent gameEvent)
        {
            List<IEntity> entitiesWithTasks = logic.GetEntitiesWithComponent(typeof(TaskComponent));

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
