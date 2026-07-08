using System;
using ECS.Component.Task; // Assuming Priorities is here
using ECS.Entity; // For the IEntity class (or interface)

namespace ECS.Entity
{
    public class MemoryEntity
    {
        private IEntity memory;
        private Priorities basePrio;
        private readonly DateTime creationTime;

        // Constructor with IEntity and Priority
        public MemoryEntity(IEntity entity, Priorities prio)
        {
            memory = entity;
            basePrio = prio;
            creationTime = DateTime.Now;
        }

        // Default constructor (no parameters)
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

        // Setters that return the previous value
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

        // Method that compares memorability with another MemoryEntity
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
