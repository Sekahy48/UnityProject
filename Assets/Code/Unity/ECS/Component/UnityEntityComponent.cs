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
            // Possible issue: GameObjects can't be cloned directly.
            return new UnityEntityComponent(this.GameObject);
        }

        public GameObject GetGameObject()
        {
            return this.GameObject;
        }

        public bool Equivalent(IComponent other)
        {
            return 
                other is UnityEntityComponent otherUnity &&
                this.GameObject == otherUnity.GameObject;
        }
    }

}