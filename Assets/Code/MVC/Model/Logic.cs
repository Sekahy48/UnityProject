using System;
using System.Collections.Generic;
using ECS.Entity;
using ECS.Systems; 
using UnityEngine;

namespace MVC.Model
{
    public class Logic
    {
        private static readonly EntityManager entityManager = new EntityManager();
        private readonly ClockSystem clockInstance = ClockSystem.GetInstance();
        private MapManager MapManager;
         
        public EntityManager GetEntityManager()
        {
            return entityManager;
        }

        public List<IEntity> GetEntitiesWithComponent(Type componentName)
        {
            return entityManager.GetEntitiesWithComponent(componentName);
        }

        public void ExecuteFrame()
        {
            float deltaTime = Time.deltaTime;
            clockInstance.Update(deltaTime);
        }

        public void SetCurrentMap(String map)
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
