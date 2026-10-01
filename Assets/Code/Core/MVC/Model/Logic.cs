using System;
using System.Collections.Generic;
using Core.ECS.Entity;
using Core.ECS.Systems;

namespace Core.MVC.Model
{
    /// <summary>
    /// Game model. No UnityEngine dependencies.
    /// </summary>
    public class Logic
    {
        private readonly EntityManager _entityManager;

        public Logic(EntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        public EntityManager GetEntityManager()
        {
            return _entityManager;
        }

        public List<IEntity> GetEntitiesWithComponent(Type componentName)
        {
            return _entityManager.GetEntitiesWithComponent(componentName);
        }

        public IEntity GetPlayer()
        {
            return _entityManager.GetPlayer();
        }
    }
}
