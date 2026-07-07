using System;
using System.Collections.Generic;
using ECS.Entity;
using ECS.Systems;
using Observer;
using UnityEngine;

namespace MVC.Model
{
    public class Logic : IObserver
    {
        private readonly EntityManager entityManager;
        private readonly FatigueStaminaSystem fatigueStaminaSystem = new();
        private readonly ClockSystem clockInstance = ClockSystem.GetInstance();
        private Boolean changesRemaining = false;  
        private MapManager MapManager;

        public Logic(GameObject playerObject)
        {
            entityManager = new EntityManager(playerObject);
        }

        public EntityManager GetEntityManager()
        {
            return entityManager;
        }

        public FatigueStaminaSystem GetFatigueStaminaSystem()
        {
            return fatigueStaminaSystem;
        }
        
        public List<IEntity> GetEntitiesWithComponent(Type componentName)
        {
            return entityManager.GetEntitiesWithComponent(componentName);
        }

        public void UpdateThis()
        {
            //float deltaTime = Time.deltaTime;
            //clockInstance.Update(deltaTime);
            //Debug.Log("Updating Logic" + entityManager.GetPlayer().GetComponent<ECS.Component.MovementComponent>().IsRunning());

            if (changesRemaining)
            {
                changesRemaining = false;
                this.fatigueStaminaSystem.ProcessEntity(Time.deltaTime, entityManager.GetPlayer(), entityManager.GetPlayer().GetComponent<ECS.Component.MovementComponent>().IsRunning());
            }
        }

        public void Update()
        {
            changesRemaining = true;
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
