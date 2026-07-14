using System;
using System.Collections.Generic;
using ECS.Entity;
using ECS.Systems;

namespace MVC.Model
{
    /// <summary>
    /// Game model. No UnityEngine dependencies.
    /// </summary>
    public class Logic
    {
        private readonly EntityManager _entityManager;
        private MapManager MapManager;

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

        public void SetCurrentMap(string map)
        {
            MapManager = new MapManager();
            MapManager.LoadMap(map);
        }

        public Map GetCurrentMap()
        {
            return MapManager.GetCurrentMap();
        }

        public IEntity GetPlayer()
        {
            return _entityManager.GetPlayer();
        }
    }
}
