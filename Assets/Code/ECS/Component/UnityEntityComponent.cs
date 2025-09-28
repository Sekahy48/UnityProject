using UnityEngine;

namespace ECS.Component
{
    public class UnityEntityComponent : IComponent
    {
        private GameObject GameObject { get; }

        public UnityEntityComponent(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public IComponent Clone()
        {
            // Posible problema: los GameObjects no se pueden clonar directamente.
            return new UnityEntityComponent(this.GameObject);
        }

        public GameObject GetGameObject()
        {
            return this.GameObject;
        }
    }

}