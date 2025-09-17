using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;
using System;
namespace ECS.Systems
{
    public class HealthManager : IObserver
    {
        /// <summary>
        /// Procesa un evento de daño o curación entre entidades.
        /// </summary>
        public void Process(IEntity source, IEntity target)
        {
            try
            {
                if (source == null || target == null) return;

                if (source.HasComponent(typeof(DamageComponent)))
                {
                    ApplyDamage(source, target);
                }

                if (source.HasComponent(typeof(HealComponent)))
                {
                    ApplyHealing(source, target);
                }
            }
            catch (EntityComponentException e)
            {
                Debug.LogError($"[HealthSystem] Error processing entities: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// Aplica daño desde una entidad a otra.
        /// </summary>
        public void ApplyDamage(IEntity source, IEntity target)
        {
            var damage = source.GetComponent<DamageComponent>(typeof(DamageComponent));
            var health = target.GetComponent<HealthComponent>(typeof(HealthComponent));

            if (health == null)
            {
                Debug.Log("[HealthSystem] Target entity does not have a HealthComponent.");
                return;
            }

            int damageDealt = damage.CalculateDamage();
            health.ReceiveDamage(damageDealt);

            Debug.Log($"[HealthSystem] Entity [{target.GetCompoundIdentification()}] received {damageDealt} damage from [{source.GetCompoundIdentification()}]");
        }

        /// <summary>
        /// Aplica curación desde una entidad a otra.
        /// </summary>
        public void ApplyHealing(IEntity source, IEntity target)
        {
            var heal = source.GetComponent<HealComponent>(typeof(HealComponent));
            var health = target.GetComponent<HealthComponent>(typeof(HealthComponent));

            if (health == null)
            {
                Debug.Log("[HealthSystem] Target entity does not have a HealthComponent.");
                return;
            }

            int healingDone = heal.CalculateHealing();
            health.HealHealth(healingDone);

            Debug.Log($"[HealthSystem] Entity [{target.GetCompoundIdentification()}] received {healingDone} healing from [{source.GetCompoundIdentification()}]");
        }

        /// <summary>
        /// Método requerido por la interfaz IObserver (vacío por ahora).
        /// </summary>
        public void Update()
        {
            throw new System.NotImplementedException("HealthManager.Update is not yet implemented.");
        }
    }
}
