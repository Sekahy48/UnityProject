using System;
using ECS.Component.Task; // Asumo que Priorities está aquí
using ECS.Entity; // Para la clase IEntity (o interfaz)

namespace ECS.Entity
{
    public class MemoryEntity
    {
        private IEntity memory;
        private Priorities basePrio;
        private readonly DateTime creationTime;

        // Constructor con IEntity y Prioridad
        public MemoryEntity(IEntity entity, Priorities prio)
        {
            memory = entity;
            basePrio = prio;
            creationTime = DateTime.Now;
        }

        // Constructor por defecto (sin parámetros)
        public MemoryEntity()
        {
            basePrio = Priorities.OMNIP;
            creationTime = DateTime.Now;
        }

        // Getters
        public IEntity GetMemory()
        {
            return memory;
        }

        public Priorities GetPrio()
        {
            return basePrio;
        }

        public DateTime GetCreationDate()
        {
            return creationTime;
        }

        // Setters que devuelven el valor anterior
        public IEntity SetMemory(IEntity newMemo)
        {
            var old = memory;
            memory = newMemo;
            return old;
        }

        public Priorities SetPrio(Priorities newPrio)
        {
            var old = basePrio;
            basePrio = newPrio;
            return old;
        }

        // Método que compara memorabilidad con otra MemoryEntity
        public bool IsMoreMemorable(MemoryEntity incoming)
        {
            if (this.GetPrio().Equals(incoming.GetPrio()))
            {
                return incoming.GetCreationDate() > this.GetCreationDate();
            }
            else
            {
                return incoming.GetPrio().GetValue() > this.GetPrio().GetValue();
            }
        }
    }
}
