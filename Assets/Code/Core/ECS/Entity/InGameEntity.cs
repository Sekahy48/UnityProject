    using System;
    using System.Collections.Generic;
    using Core.ECS.Component;
    using Core.Handler; 

    namespace Core.ECS.Entity
    {
        /// <summary>
        /// Represents concrete instances of entities usable in-game.
        /// Identified by a numeric id and a name (concrete type).
        /// Contains components organized in a Dictionary.
        /// </summary>
        public class InGameEntity : IEntity
        {
            protected EntityId id;
            protected readonly Dictionary<Type, IComponent> components = new();

            /// <summary>
            /// La entidad no guarda su arquetipo.
            ///
            /// Lo guardaba como texto, y ese campo mezclaba dos preguntas distintas: los
            /// arquetipos del mundo —jugador, monton, nodo de recurso— que son claves del
            /// registro de prototipos, y el "ItemEntity" que pasaba
            /// <see cref="ItemEntity"/>, que no es un arquetipo sino su propia clase de C#.
            /// Quien necesita el arquetipo lo tiene en la mano al crear la entidad
            /// (<see cref="EntityType"/>); quien necesita saber si algo es un item lo
            /// pregunta con <c>is ItemEntity</c>, que no puede desincronizarse de la verdad.
            /// </summary>
            public InGameEntity(int id)
            {
                this.id = new EntityId(id);
            }

            // ---- Getters ----
            public IHandler GetId()
            {
                return id;
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
            /// Gets a component by dynamic Type. Use only when the type isn't known at compile time.
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

            public IEnumerable<IComponent> GetComponents()
            {
                return components.Values;
            }

            protected virtual InGameEntity CreateCloneInstance(int id)
            {
                return new InGameEntity(id);
            }

            public IEntity Clone()
            {
                InGameEntity clone = CreateCloneInstance(IdGenerator.GenerateNewId());
                foreach (IComponent elem in components.Values)
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
            /// <summary>
            /// Dos entidades son equivalentes si llevan exactamente los mismos componentes
            /// con los mismos valores.
            ///
            /// <para>Antes empezaba comparando el arquetipo, que era una cadena. Al quitar
            /// ese campo hacia falta otra guarda, y la buena resulto ser la que faltaba:
            /// <b>comparar cuantos componentes tiene cada una</b>. Sin eso el recorrido es
            /// de un solo sentido —comprueba que los mios estan en el otro, no al reves—,
            /// asi que una entidad con componentes de mas pasaba por equivalente y la
            /// respuesta cambiaba segun cual de las dos preguntara. Eso era un defecto
            /// aparte del arquetipo, y comparar clases en su lugar no lo habria tapado: el
            /// jugador y un monton son los dos <c>InGameEntity</c>.</para>
            /// </summary>
            public bool Equivalent(IEntity other)
            {
                if (other == null || components.Count != CountComponentsOf(other))
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

            private static int CountComponentsOf(IEntity entity)
            {
                int count = 0;
                foreach (IComponent unused in entity.GetComponents()) count++;
                return count;
            }
        }
    
    }
