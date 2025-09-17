using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using Factories;
using Observer;
using UnityEngine;

namespace ECS.Systems
{
    public class EntityManager : IObserver
    {
        private readonly Dictionary<int, IEntity> entities = new();
        private readonly Dictionary<string, IEntity> prototypes = new();
        private readonly Dictionary<string, ItemEntity> itemsCatalog = new();

        public EntityManager()
        {
            prototypes["resourceNode"] = PrototypeFactory.CreateResourceNodePrototype();
            prototypes["aliveEntity"] = PrototypeFactory.CreateAliveEntityPrototype();
            prototypes["playerEntity"] = PrototypeFactory.CreatePlayerEntityPrototype();
        }

        public IEntity CreateEntity(string type)
        {
            if (!prototypes.TryGetValue(type, out IEntity prototype))
                throw new ArgumentException($"No prototype found for type: {type}");

            IEntity created = prototype.Clone();
            entities[created.GetIdAsInt()] = created;
            return created;
        }

        public void RemoveEntity(int id)
        {
            entities.Remove(id);
        }

        public IEntity GetEntity(int id)
        {
            return entities.TryGetValue(id, out var entity) ? entity : null;
        }

        public void AddComponentToEntity(int entityId, IComponent component)
        {
            if (!entities.TryGetValue(entityId, out var entity))
                throw new ArgumentException($"Entity with ID {entityId} not found.");

            entity.AddComponent(component);
        }

        public void RemoveComponentFromEntity(int entityId, Type target)
        {
            if (!entities.TryGetValue(entityId, out var entity))
                throw new ArgumentException($"Entity with ID {entityId} not found.");

            entity.RemoveComponent(target);
        }

        public List<IEntity> GetEntitiesWithComponent(Type target)
        {
            List<IEntity> result = new();
            foreach (var entity in entities.Values)
            {
                if (entity.HasComponent(target))
                {
                    result.Add(entity);
                }
            }
            return result;
        }

        public List<IEntity> GetPrototypes()
        {
            return new List<IEntity>(prototypes.Values);
        }

        public void Update()
        {
            throw new NotImplementedException("Unimplemented method 'Update'");
        }

        public IEntity GetPlayer()
        {
            var playerPrototype = prototypes["playerEntity"];
            var player = entities.GetValueOrDefault(playerPrototype.GetIdAsInt());

            if (player == null)
            {
                Debug.Log("EntityManager: Player entity not found, creating a new one.");
                player = playerPrototype.Clone();
                Debug.Log("EntityManager: Player entity retrieved: " + player.GetName());
                entities[IdGenerator.GenerateNewId()] = player;
            }

            return player;
        }
    }
}
