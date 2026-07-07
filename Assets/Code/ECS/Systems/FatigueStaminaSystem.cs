using System;
using ECS.Component;
using ECS.Entity;
using Observer;
using UnityEngine;

namespace ECS.Systems
{
    public class FatigueStaminaSystem : GenericSubject
    {
        private const float STAMINA_REGEN_RATE = 5f;
        private const float FATIGUE_REGEN_RATE = 0.5f;

        private const float STAMINA_DRAIN_PER_SECOND = 10f;
        private const float FATIGUE_DRAIN_PER_STAMINA = 0.01f;
        private const float FATIGUE_BURST_DRAIN = 5f;
        public void ProcessEntity(float DeltaTime, IEntity entity, Boolean drain)
        {
            Boolean changed = false;
            if (entity.HasComponent(typeof(FisiologicComponent)))
            {
                FisiologicComponent fisiologic = entity.GetComponent<FisiologicComponent>();

                if (!drain)
                {
                    if (!fisiologic.IsFatigueFull())
                    {
                        RestoreFatigue(DeltaTime, fisiologic); 
                        changed = true;
                    }
                    if (!fisiologic.IsStaminaFull())
                    {
                        RestoreStamina(DeltaTime, fisiologic);
                        changed = true;
                    }
                    if (fisiologic.IsStaminaFull())
                    {
                        entity.GetComponent<MovementComponent>().SetCanRun(true);
                    }
                }
                else
                {
                    if (!fisiologic.IsStaminaEmpty())
                    {
                        DrainStamina(DeltaTime, fisiologic);
                        changed = true;
                        if (fisiologic.IsStaminaEmpty())
                        {
                            //UnityEngine.Debug.Log("Stamina has reached zero!");
                            entity.GetComponent<MovementComponent>().SetCanRun(false);
                        }
                    }
                }

                if (changed)
                {
                    this.NotifyObservers();
                }
            }
            else
            {
                UnityEngine.Debug.LogError($"Entity {entity.GetCompoundIdentification()} does not have a FisiologicComponent.");
            }
            
        }
        
        public void RestoreFatigue(float DeltaTime, FisiologicComponent component)
        {
            component.SetFatigue(component.GetFatigue() + FATIGUE_REGEN_RATE * DeltaTime);

        }

        public void RestoreStamina(float DeltaTime, FisiologicComponent component)
        {
            component.SetStamina(component.GetStamina() + STAMINA_REGEN_RATE * DeltaTime);
        }

        public void DrainStamina(float DeltaTime, FisiologicComponent component)
        {
            component.SetStamina(component.GetStamina() - STAMINA_DRAIN_PER_SECOND * DeltaTime);
            Boolean burst = component.GetStamina() <= 0;
            DrainFatigue(DeltaTime, component, burst);
        }
        
        public void DrainFatigue(float DeltaTime, FisiologicComponent component, Boolean burst)
        {
            float fatigueDrain = FATIGUE_DRAIN_PER_STAMINA * STAMINA_DRAIN_PER_SECOND * DeltaTime;
            if (burst)
            {
                fatigueDrain += FATIGUE_BURST_DRAIN;
            }   
            component.SetFatigue(component.GetFatigue() - fatigueDrain);
        }

    }
}