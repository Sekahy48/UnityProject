using System;
using Core.ECS.Component.Task; // Se supone que Priorities esta aqui
using Core.ECS.Entity; // Para IEntity

namespace Core.ECS.Entity
{
    public class MemoryEntity
    {
        private IEntity _memory;
        private Priorities _basePrio;
        private readonly DateTime _creationTime;

        // Constructor con IEntity y prioridad
        public MemoryEntity(IEntity entity, Priorities prio)
        {
            _memory = entity;
            _basePrio = prio;
            _creationTime = DateTime.Now;
        }

        // Constructor por defecto (sin parametros)
        public MemoryEntity()
        {
            _basePrio = Priorities.Omnip;
            _creationTime = DateTime.Now;
        }

        // Lecturas
        public IEntity GetMemory()
        {
            return _memory;
        }

        public Priorities GetPrio()
        {
            return _basePrio;
        }

        public DateTime GetCreationDate()
        {
            return _creationTime;
        }

        // Escrituras que devuelven el valor anterior
        public IEntity SetMemory(IEntity newMemo)
        {
            var old = _memory;
            _memory = newMemo;
            return old;
        }

        public Priorities SetPrio(Priorities newPrio)
        {
            var old = _basePrio;
            _basePrio = newPrio;
            return old;
        }

        // Compara lo memorable que es con otro MemoryEntity
        public bool IsMoreMemorable(MemoryEntity incoming)
        {
            if (GetPrio().Equals(incoming.GetPrio()))
            {
                return incoming.GetCreationDate() > GetCreationDate();
            }
            else
            {
                return incoming.GetPrio().GetValue() > GetPrio().GetValue();
            }
        }
    }
}
