using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.Factories;
using Core.Observer;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Manages the game's entities. No UnityEngine dependencies.
    /// Prototypes are created with pure Core components.
    /// IEntityLinker (Unity) handles linking entities with GameObjects.
    /// </summary>
    public class EntityManager : IObserver
    {
        private int _playerId = -1;
        private PrototypeFactory _prototypeFactory;
        private readonly Dictionary<int, IEntity> _entities = new();
        private readonly Dictionary<string, IEntity> _prototypes = new(); 

        public EntityManager(PrototypeFactory prototypeFactory)
        { 
            _prototypeFactory = prototypeFactory;
            _prototypes["resourceNode"] = _prototypeFactory.CreateResourceNodePrototype();
            _prototypes["aliveEntity"] = _prototypeFactory.CreateAliveEntityPrototype();
            _prototypes["playerEntity"] = _prototypeFactory.CreatePlayerEntityPrototype();
        }

        public IEntity CreateEntity(string type)
        {
            if (!_prototypes.TryGetValue(type, out IEntity prototype))
                throw new ArgumentException($"No prototype found for type: {type}");

            IEntity created = prototype.Clone();
            _entities[created.GetIdAsInt()] = created;
            return created;
        }

        public void RemoveEntity(int id)
        {
            _entities.Remove(id);
        }

        public IEntity GetEntity(int id)
        {
            return _entities.TryGetValue(id, out var entity) ? entity : null;
        }

        public void AddComponentToEntity(int entityId, IComponent component)
        {
            if (!_entities.TryGetValue(entityId, out var entity))
                throw new ArgumentException($"Entity with ID {entityId} not found.");
            entity.AddComponent(component);
        }

        public void RemoveComponentFromEntity(int entityId, Type target)
        {
            if (!_entities.TryGetValue(entityId, out var entity))
                throw new ArgumentException($"Entity with ID {entityId} not found.");
            entity.RemoveComponent(target);
        }

        public List<IEntity> GetEntitiesWithComponent(Type target)
        {
            List<IEntity> result = new();
            foreach (var entity in _entities.Values)
            {
                if (entity.HasComponent(target))
                    result.Add(entity);
            }
            return result;
        }

        public List<IEntity> GetPrototypes()
        {
            return new List<IEntity>(_prototypes.Values);
        }

        public void Update()
        {
            throw new NotImplementedException("Unimplemented method 'Update'");
        }

        public IEntity GetPlayer()
        {
            if (_playerId >= 0 && _entities.TryGetValue(_playerId, out var existing))
                return existing;

            IEntity playerPrototype = _prototypes["playerEntity"];
            IEntity player = playerPrototype.Clone();
            _playerId = player.GetIdAsInt();
            _entities[_playerId] = player;
            return player;
        }
    }
}
