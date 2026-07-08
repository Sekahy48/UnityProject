using System;
using System.Collections.Generic;
using ECS.Entity;
using ECS.Systems;

namespace MVC.Model
{
    /// <summary>
    /// Modelo del juego. Sin dependencias de UnityEngine.
    /// </summary>
    public class Logic
    {
        private readonly EntityManager entityManager;
        private MapManager MapManager;

        public Logic()
        {
            entityManager = new EntityManager();
        }

        public EntityManager GetEntityManager()
        {
            return entityManager;
        }

        public List<IEntity> GetEntitiesWithComponent(Type componentName)
        {
            return entityManager.GetEntitiesWithComponent(componentName);
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
            return entityManager.GetPlayer();
        }
    }
}
