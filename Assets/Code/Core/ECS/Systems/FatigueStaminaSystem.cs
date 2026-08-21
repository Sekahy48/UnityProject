using System;
using System.Collections.Generic;
using ECS.Component;
using ECS.Entity;
using Observer;

namespace ECS.Systems
{
    public class FatigueStaminaSystem : GenericSubject, IPeriodicSystem
    {
        private const float STAMINA_REGEN_RATE = 5f;
        private const float FATIGUE_REGEN_RATE = 0.5f;

        private const float STAMINA_DRAIN_PER_SECOND = 10f;
        private const float FATIGUE_DRAIN_PER_STAMINA = 0.01f;
        private const float FATIGUE_BURST_DRAIN = 5f;

        private bool _staminaRestrictionActive = false;

        /// <summary>
        /// IGameSystem: processes all entities with EnergyComponent + MovementComponent.
        /// </summary>
        public void Process(float deltaTime, EntityManager entityManager)
        {
            List<IEntity> entities = entityManager.GetEntitiesWithComponent(typeof(EnergyComponent));
            foreach (var entity in entities)
            {
                ProcessEntity(deltaTime, entity);
            }
        }

        public void ProcessEntity(float deltaTime, IEntity entity)
        {
            EnergyComponent energy = entity.GetComponent<EnergyComponent>();
            MovementComponent movement = entity.GetComponent<MovementComponent>();

            if (energy == null) return;

            bool drain = movement != null && movement.IsRunning();
            bool changed = false;

            if (!drain)
            {
                if (!energy.IsFatigueFull())
                {
                    RestoreFatigue(deltaTime, energy);
                    changed = true;
                }
                if (!energy.IsStaminaFull())
                {
                    RestoreStamina(deltaTime, energy);
                    changed = true;
                }
                if (energy.IsStaminaFull() && movement != null && _staminaRestrictionActive)
                {
                    movement.RemoveRunRestriction();
                    _staminaRestrictionActive = false;
                }
            }
            else
            {
                if (!energy.IsStaminaEmpty())
                {
                    DrainStamina(deltaTime, energy);
                    changed = true;
                    if (energy.IsStaminaEmpty() && !_staminaRestrictionActive)
                    {
                        movement.AddRunRestriction();
                        _staminaRestrictionActive = true;
                    }
                }
            }

            if (changed)
            {
                this.NotifyObservers();
            }
        }

        public void RestoreFatigue(float deltaTime, EnergyComponent component)
        {
            component.SetFatigue(component.GetFatigue() + FATIGUE_REGEN_RATE * deltaTime);
        }

        public void RestoreStamina(float deltaTime, EnergyComponent component)
        {
            component.SetStamina(component.GetStamina() + STAMINA_REGEN_RATE * deltaTime);
        }

        public void DrainStamina(float deltaTime, EnergyComponent component)
        {
            component.SetStamina(component.GetStamina() - STAMINA_DRAIN_PER_SECOND * deltaTime);
            bool burst = component.GetStamina() <= 0;
            DrainFatigue(deltaTime, component, burst);
        }

        public void DrainFatigue(float deltaTime, EnergyComponent component, bool burst)
        {
            float fatigueDrain = FATIGUE_DRAIN_PER_STAMINA * STAMINA_DRAIN_PER_SECOND * deltaTime;
            if (burst)
            {
                fatigueDrain += FATIGUE_BURST_DRAIN;
            }
            component.SetFatigue(component.GetFatigue() - fatigueDrain);
        }
    }
}
