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
            protected EntityId id;
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
                return GetComponent<NameComponent>()?.DisplayName;
            }

                public IHandler GetCompoundIdentification()
            {
                return new NameId($"{this.GetComponent<NameComponent>()}-{id}");
            }

            public int GetIdAsInt()
            {
                return id.ToInt();
            }

            public int GenerateEntityId()
            {
                this.id = new EntityId(IdGenerator.GenerateNewId());
                return this.id.ToInt();
            }
            public T GetComponent<T>() where T : IComponent
            {
                if (HasComponent(typeof(T)))
                    return (T)components[typeof(T)];
                else
                    return default;
            }

            /// <summary>
            /// Obtiene un componente por Type dinámico. Usar solo cuando el tipo no se conoce en compilación.
            /// </summary>
            public IComponent GetComponentByType(Type target)
            {
                return components.TryGetValue(target, out var c) ? c : null;
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

            /// <summary>
            /// Checks if this entity is equivalent to another (same type and same components with same values)
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equivalent(IEntity other)
            {
                if (other == null || this.GetEntityType().ToString() != other.GetEntityType().ToString())
                    return false;
                foreach (IComponent component in components.Values)
                {
                    //TODO 
                    if (!other.HasComponent(component.GetType()))
                        return false;
                    if (!component.Equivalent(other.GetComponentByType(component.GetType())))
                        return false;
                }
                return true;
            }

            public override bool Equals(object obj)
            {
                return 
                    obj is InGameEntity other &&
                    this.id.Equals(other.id) &&
                    this.type.Equals(other.type) &&
                    this.Equivalent(other);
            }
        }
    
    }
