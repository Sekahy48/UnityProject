using System;
using System.Collections.Generic;
using ECS.Component;
using Handler;

namespace ECS.Entity
{
    /// <summary>
    /// Representa instancias concretas de entidades utilizables in-game.
    /// Se identifica por un id numérico y un nombre (tipo concreto).
    /// Contiene componentes organizados en un Dictionary.
    /// </summary>
    public class InGameEntity : IEntity
    {
        protected readonly EntityId id;
        protected readonly NameId type;
        protected readonly Dictionary<Type, IComponent> components = new();

        // Constructor
        public InGameEntity(int id, string type)
        {
            this.id = new EntityId(id);
            this.type = type == null ? new NameId("Generic Entity") : new NameId(type);
        }

        // ---- Getters ----
        public IHandler GetId()
        {
            return id;
        }

        public IHandler GetEntityType()
        {
            return type;
        }

        public string GetName()
        {
            return GetComponent<NameComponent>(typeof(NameComponent))?.GetDisplayName();
        }

            public IHandler GetCompoundIdentification()
        {
            return new NameId($"{this.GetComponent<NameComponent>(typeof(NameComponent))}-{id}");
        }

        public int GetIdAsInt()
        {
            return id.ToInt();
        }

        public T GetComponent<T>(Type target) where T : IComponent
        {
            if (HasComponent(target))
                return (T)components[target];
            else
                return default;
        }

        // ---- IComponent related ----
        public void AddComponent<T>(T component) where T : IComponent
        {
            components[component.GetType()] = component;
        }

        public bool HasComponent(Type target)
        {
            return components.ContainsKey(target);
        }

        public bool RemoveComponent(Type target)
        {
            return components.Remove(target);
        }

        public IEntity Clone()
        {
            var clone = new InGameEntity(IdGenerator.GenerateNewId(), type.ToString());
            foreach (var elem in components.Values)
            {
                clone.AddComponent(elem.Clone());
            }
            return clone;
        }
    }
 
}
