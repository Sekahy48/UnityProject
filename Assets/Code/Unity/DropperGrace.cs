using UnityEngine;

namespace Unity
{
    /// <summary>
    /// Mientras dura, lo soltado no choca con quien lo solto (decision 45): nace dentro de el
    /// y, sin esto, la fisica lo expulsaria de golpe o empujaria al jugador. Las colisiones se
    /// restauran pasado medio segundo Y cuando ya no se solapan; entonces el componente se
    /// quita solo.
    ///
    /// <para>Es un MonoBehaviour porque tiene que contar el tiempo; por eso vive en un archivo
    /// con su mismo nombre, como exige Unity.</para>
    /// </summary>
    public class DropperGrace : MonoBehaviour
    {
        private const float DURATION = 0.5f;

        private Collider[] _dropper;
        private float _endsAt;

        public void Begin(GameObject dropper)
        {
            _dropper = dropper.GetComponentsInChildren<Collider>();
            _endsAt = Time.time + DURATION;
            SetIgnored(GetComponentsInChildren<Collider>(), true);
        }

        /// <summary>Para los colliders que llegan despues: los del modelo, al cargar.</summary>
        public void IgnoreToo(Collider collider) => SetIgnored(new[] { collider }, true);

        private void Update()
        {
            if (Time.time < _endsAt) return;

            // Aun dentro de quien lo solto (cayo a sus pies contra una pared): reactivar ahora
            // haria que la fisica los separase de golpe y el jugador pegaria un bote, mayor
            // cuanto mas pese el objeto. Se espera a que se aparten; en cuanto el jugador se
            // mueve, ocurre. Mientras, si se le pisa, se atraviesa.
            Collider[] own = GetComponentsInChildren<Collider>();
            if (Overlaps(own)) return;

            SetIgnored(own, false);
            Destroy(this);
        }

        private bool Overlaps(Collider[] own)
        {
            if (_dropper == null) return false;
            foreach (Collider mine in own)
                foreach (Collider theirs in _dropper)
                    if (mine != null && theirs != null && mine.enabled && theirs.enabled &&
                        Physics.ComputePenetration(mine, mine.transform.position, mine.transform.rotation,
                                                   theirs, theirs.transform.position, theirs.transform.rotation,
                                                   out _, out _))
                        return true;
            return false;
        }

        private void SetIgnored(Collider[] own, bool ignored)
        {
            if (_dropper == null) return;
            foreach (Collider mine in own)
                foreach (Collider theirs in _dropper)
                    if (mine != null && theirs != null)
                        Physics.IgnoreCollision(mine, theirs, ignored);
        }
    }
}
