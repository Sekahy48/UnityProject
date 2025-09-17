using ECS.Entity;
using Observer;

namespace ECS.Component.Task
{
    public interface ITask : IObserver, ISubject
    {
        void Execute(IEntity entity);            // Ejecutar la tarea
        bool IsCompleted(IEntity entity);        // Verificar si la tarea está completada
        string GetDescription();                 // Obtener descripción de la tarea
        void Update(IEntity entity, float delta); // Actualizar la tarea si es necesario
        TaskState GetState();                    // Obtener estado de la tarea
        ITask Clone();                          // Clonar tarea para copia independiente
    }
}
