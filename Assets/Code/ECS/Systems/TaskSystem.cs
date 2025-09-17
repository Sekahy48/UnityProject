using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using MVC.Model;
using Observer;

namespace ECS.Systems
{
    public class TaskSystem : IObserver
    {
        private readonly Logic logic;

        public TaskSystem(Logic logic)
        {
            this.logic = logic;
        }

        public void Update()
        {
            List<IEntity> entitiesWithTasks = logic.GetEntitiesWithComponent(typeof(TaskComponent));

            foreach (var entity in entitiesWithTasks)
            {
                var taskComponent = entity.GetComponent<TaskComponent>(typeof(TaskComponent));
                if (taskComponent != null)
                    ExecuteTaskComponent(entity, taskComponent);
            }
        }

        private void ExecuteTaskComponent(IEntity entity, TaskComponent taskComponent)
        {
            if (taskComponent == null)
                throw new NullReferenceException("Entity does not have TaskComponent. Unexpected behavior.");

            if (taskComponent.GetCurrentTask() == null)
                taskComponent.SetCurrentTask(taskComponent.PollNextTask());

            if (taskComponent.GetCurrentTask() != null)
            {
                taskComponent.GetCurrentTask().Execute(entity);

                if (taskComponent.GetCurrentTask().IsCompleted(entity))
                {
                    taskComponent.RemoveTask(taskComponent.GetCurrentTask());
                    taskComponent.SetCurrentTask(null);
                }
            }
        }
    }
}
