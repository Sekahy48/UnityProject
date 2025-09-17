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
            throw new System.NotImplementedException();
        }

        public GameObject GetGameObject()
        {
            return this.GameObject;
        }
    }

}