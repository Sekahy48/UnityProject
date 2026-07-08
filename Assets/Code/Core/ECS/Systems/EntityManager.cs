using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using Factories;
using Observer;

namespace ECS.Systems
{
    /// <summary>
    /// Gestiona entidades del juego. Sin dependencias de UnityEngine.
    /// Los prototipos se crean con componentes Core puros.
    /// IEntityLinker (Unity) se encarga de vincular entidades con GameObjects.
    /// </summary>
    public class EntityManager : IObserver
    {
        private int _playerId = -1;
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
                    result.Add(entity);
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
            if (_playerId >= 0 && entities.TryGetValue(_playerId, out var existing))
                return existing;

            IEntity playerPrototype = prototypes["playerEntity"];
            IEntity player = playerPrototype.Clone();
            _playerId = player.GetIdAsInt();
            entities[_playerId] = player;
            return player;
        }
    }
}
