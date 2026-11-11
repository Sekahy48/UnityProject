using UnityEngine;

using Core.ECS.Component;

namespace Unity.ECS.Component
{
    /// <summary>
    /// Puente entre una entidad de Core y su GameObject. Lo pone y lo quita solo el
    /// linker, y no se clona (ver <see cref="IEngineBridge"/>).
    /// </summary>
    public class UnityEntityComponent : IEngineBridge
    {
        private GameObject GameObject { get; }

        public UnityEntityComponent(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public IComponent Clone()
        {
            // No deberia llamarse: InGameEntity.Clone salta los IEngineBridge. Si alguien lo
            // hiciera, devolveria un segundo puente al MISMO GameObject, que es justo el
            // error que eso evita.
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