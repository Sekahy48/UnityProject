using System.Collections.Generic;
using Core.ECS;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Unity.ECS.Component;
using UnityEngine;
using AC = Core.Utils.ArgumentChecker;

namespace Unity.ECS.Systems
{
    /// <summary>
    /// Devuelve al mundo lo que se sale de el (Fase 1.5, M4; decisiones 13, 29 y 46): cualquier
    /// entidad con Rigidbody, el jugador incluido.
    ///
    /// <para>Sistema del motor y no del Core: salirse del mundo es un fallo de la fisica
    /// (atravesar el suelo, caer por un borde), no una regla de juego, y buscar suelo es
    /// geometria. Mueve el Rigidbody y el Core se entera por TransformSyncSystem, que corre
    /// despues.</para>
    ///
    /// <para>Recuerda el ultimo sitio seguro de cada cuerpo: un objeto, cuando Unity lo duerme;
    /// un cuerpo que anda (con BodyComponent: el jugador, los NPC), cuando pisa suelo. Al
    /// aparecer, el primer sitio que se ve vale como seguro.</para>
    ///
    /// <para>Al salirse, dos casos. Si hay suelo en su misma X y Z, se le pone encima: el rayo
    /// sale 2 m por encima de donde estuvo quieto y no desde lo alto del mapa, porque desde
    /// arriba, dentro de una casa, daria con el tejado. Solo vale suelo estatico (sin
    /// Rigidbody). Si no hay suelo (cayo por un borde), vuelve al ultimo sitio seguro.</para>
    /// </summary>
    public class OutOfWorldRecoverySystem : IPeriodicSystem
    {
        private const float SEARCH_ABOVE = 2f;        // m por encima del ultimo sitio seguro
        private const float LIFT = 0.05f;             // m sobre el suelo al recolocar
        private const float GROUND_PROBE = 0.15f;     // m bajo los pies para dar por hecho que pisa
        private const int MAX_HITS = 16;

        private readonly IWorldLimit _limit;
        private readonly Dictionary<Rigidbody, Vector3> _lastSafe = new();
        private readonly RaycastHit[] _hits = new RaycastHit[MAX_HITS];
        private readonly List<Rigidbody> _gone = new();

        public OutOfWorldRecoverySystem(IWorldLimit limit)
        {
            AC.CheckNotNull(limit, nameof(limit));
            _limit = limit;
        }

        public void Process(float deltaTime, EntityManager entityManager)
        {
            AC.CheckNotNull(entityManager, nameof(entityManager));

            foreach (IEntity entity in entityManager.GetEntitiesWithComponent(typeof(UnityEntityComponent)))
            {
                GameObject go = entity.GetComponent<UnityEntityComponent>().GetGameObject();
                if (go == null || !go.TryGetComponent(out Rigidbody body)) continue;

                bool walker = entity.HasComponent(typeof(BodyComponent));
                if (_limit.IsOutside(body.position)) Recover(body);
                else if (IsSafe(body, walker) || !_lastSafe.ContainsKey(body)) _lastSafe[body] = body.position;
            }

            ForgetDestroyed();
        }

        /// <summary>
        /// Si la posicion actual sirve para volver a ella. Un cuerpo que anda (jugador, NPC) vale
        /// en cuanto pisa suelo: con "quieto", tras una carrera larga sin pararse volveria
        /// lejisimos. Un objeto, solo cuando Unity lo duerme: con "pisando suelo", uno que rueda
        /// hacia un borde volveria al mismo borde y caeria otra vez.
        /// </summary>
        private bool IsSafe(Rigidbody body, bool walker)
        {
            if (!walker) return body.IsSleeping();

            Bounds bounds = BoundsOf(body);
            return FindStaticGround(bounds.center, bounds.extents.y + GROUND_PROBE, out _);
        }

        private void Recover(Rigidbody body)
        {
            Vector3 safe = _lastSafe.TryGetValue(body, out Vector3 known) ? known : Vector3.zero;
            Vector3 from = new Vector3(body.position.x, safe.y + SEARCH_ABOVE, body.position.z);
            float reach = from.y - body.position.y;

            Vector3 target;
            if (FindStaticGround(from, reach, out float groundY))
            {
                // Que la base del cuerpo (no su origen) quede sobre el suelo
                float baseOffset = body.position.y - BoundsOf(body).min.y;
                target = new Vector3(from.x, groundY + baseOffset + LIFT, from.z);
            }
            else
            {
                target = safe + Vector3.up * LIFT;
            }

            Debug.LogWarning($"OutOfWorldRecoverySystem: '{body.name}' se salio del mundo en {body.position}; vuelve a {target}.");

            body.position = target;
            body.transform.position = target;   // que se vea en este mismo fotograma
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        /// <summary>Primer suelo estatico bajo un punto. Lo que tiene Rigidbody no es suelo.</summary>
        private bool FindStaticGround(Vector3 from, float distance, out float groundY)
        {
            groundY = 0f;
            int count = Physics.RaycastNonAlloc(from, Vector3.down, _hits, distance,
                                                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            float best = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (_hits[i].rigidbody != null) continue;   // el propio cuerpo u otro objeto suelto
                if (_hits[i].distance < best) { best = _hits[i].distance; groundY = _hits[i].point.y; }
            }
            return best < float.MaxValue;
        }

        private static Bounds BoundsOf(Rigidbody body)
        {
            Collider[] colliders = body.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0) return new Bounds(body.position, Vector3.zero);
            Bounds bounds = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) bounds.Encapsulate(colliders[i].bounds);
            return bounds;
        }

        /// <summary>Lo recogido o destruido deja de ocupar sitio en la memoria del sistema.</summary>
        private void ForgetDestroyed()
        {
            _gone.Clear();
            foreach (Rigidbody body in _lastSafe.Keys)
                if (body == null) _gone.Add(body);
            foreach (Rigidbody body in _gone) _lastSafe.Remove(body);
        }
    }
}
