using Core;
using Core.ECS.Entity;
using Unity.ECS.Component;
using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Filtro de pared: una linea entre dos puntos esta libre si ningun collider solido la
    /// corta, sin contar el cuerpo del actor ni el propio objetivo.
    ///
    /// <para><b>Tapa cualquier collider</b> que no sea trigger (decision provisional: sin
    /// capas dedicadas todavia). Si algun dia tapa lo que no debe —hierba alta, volumenes
    /// invisibles—, lo siguiente es una capa <c>BlocksInteraction</c> y pasar su mascara.
    /// Los montones del suelo no llevan collider, asi que uno nunca tapa a otro.</para>
    ///
    /// <para>Se excluye por jerarquia y no por capa porque asi no depende de que nadie haya
    /// puesto al jugador en la capa correcta en el inspector: lo que cuelga del GameObject
    /// del actor o del objetivo no cuenta, este en la capa que este.</para>
    /// </summary>
    public class LineOfSightFilter : IReachFilter
    {
        /// <summary>
        /// Impactos que se miran por rayo. Si hay mas colliders que esto en la linea, los
        /// sobrantes no se ven; con distancias de 2 m es mas que suficiente.
        /// </summary>
        private const int MAX_HITS = 8;

        /// <summary>
        /// Margen al final del rayo: se detiene un poco antes del punto para no tropezar con
        /// la superficie sobre la que descansa el objetivo cuando el punto roza el suelo.
        /// </summary>
        private const float END_MARGIN = 0.05f;

        private readonly RaycastHit[] _hits = new RaycastHit[MAX_HITS];

        public bool IsClear(IEntity actor, IEntity target,
                            float fromX, float fromY, float fromZ,
                            float toX, float toY, float toZ)
        {
            Vector3 from = new Vector3(fromX, fromY, fromZ);
            Vector3 to = new Vector3(toX, toY, toZ);

            Vector3 delta = to - from;
            float length = delta.magnitude - END_MARGIN;
            if (length <= 0f) return true;

            int count = Physics.RaycastNonAlloc(from, delta.normalized, _hits, length,
                                                Physics.DefaultRaycastLayers,
                                                QueryTriggerInteraction.Ignore);

            Transform actorRoot = RootOf(actor);
            Transform targetRoot = RootOf(target);

            for (int i = 0; i < count; i++)
            {
                Transform hit = _hits[i].collider.transform;
                if (actorRoot != null && hit.IsChildOf(actorRoot)) continue;
                if (targetRoot != null && hit.IsChildOf(targetRoot)) continue;
                return false;
            }

            return true;
        }

        private static Transform RootOf(IEntity entity)
        {
            GameObject go = entity?.GetComponent<UnityEntityComponent>()?.GetGameObject();
            return go != null ? go.transform : null;
        }
    }
}
