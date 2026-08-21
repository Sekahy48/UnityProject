using System;
using ECS.Component;
using ECS.Entity;
using Events;

namespace ECS.Systems
{
    /// <summary>
    /// Calculates carry capacity from BodyComponent, EnergyComponent and NutritionComponent,
    /// and owns the load thresholds that turn a weight ratio into an encumbrance band.
    /// When the real system loop is implemented, this will become a system with its own component.
    /// </summary>
    public static class CarryCapacity
    {
        /// <summary>Ratio above which the entity is slowed down.</summary>
        public const float EXTRA_WEIGHT = 0.70f;
        /// <summary>Ratio above which the entity is heavily slowed and cannot run.</summary>
        public const float OVERWEIGHT = 0.85f;
        /// <summary>Ratio at which the entity cannot move at all.</summary>
        public const float IMMOBILE = 1f;

        /// <summary>
        /// Pure classification of a weight ratio into its encumbrance band.
        /// Returns the matching GameEventType because those values already are the
        /// vocabulary for these bands — a parallel enum would only need keeping in sync.
        ///
        /// Single source of truth: InventorySystem uses it to decide which event to post,
        /// and the UI uses it to decide which colour to paint, without duplicating thresholds.
        /// Being pure, it can be called on demand (e.g. when opening the inventory, where no
        /// event has fired) with no side effects.
        ///
        /// Checks run high-to-low so each band needs a single bound: reaching the second test
        /// already implies not being Immobile. Two-sided ranges are where off-by-one gaps hide.
        /// </summary>
        public static GameEventType ClassifyLoad(float weightRatio)
        {
            if (weightRatio >= IMMOBILE) return GameEventType.Immobile;
            if (weightRatio > OVERWEIGHT) return GameEventType.Overweight;
            if (weightRatio > EXTRA_WEIGHT) return GameEventType.ExtraWeight;
            return GameEventType.NormalWeight;
        }

        public static float GetMaxCarryWeight(IEntity entity)
        {
            BodyComponent body = entity.GetComponent<BodyComponent>();
            EnergyComponent energy = entity.GetComponent<EnergyComponent>();
            NutritionComponent nutrition = entity.GetComponent<NutritionComponent>();

            if (body == null) return 0f;

            float muscleMass = body.GetMuscleMass();
            float carryBase = muscleMass * 0.5f;
            float factorSex = (body.GetSex() == 0) ? 1.0f : 0.85f;

            float age = body.GetAge();
            float factorAge;
            if      (age < 18f)  factorAge = 0.6f  + (age - 10f) * 0.05f;
            else if (age <= 35f) factorAge = 1.0f;
            else if (age <= 60f) factorAge = 1.0f  - (age - 35f) * 0.015f;
            else                 factorAge = 0.625f - (age - 60f) * 0.01f;
            factorAge = Math.Max(factorAge, 0.1f);

            float factorHunger = 1.0f;
            float factorThirst = 1.0f;
            float factorFatigue = 1.0f;

            if (nutrition != null)
            {
                float maxHunger = nutrition.GetMaxHunger();
                float maxThirst = nutrition.GetMaxThirst();
                if (maxHunger > 0) factorHunger  = 1.0f - (nutrition.GetHunger() / maxHunger) * 0.30f;
                if (maxThirst > 0) factorThirst  = 1.0f - (nutrition.GetThirst() / maxThirst) * 0.40f;
            }

            if (energy != null)
            {
                float maxFatigue = energy.GetMaxFatigue();
                if (maxFatigue > 0) factorFatigue = 1.0f - (energy.GetFatigue() / maxFatigue) * 0.35f;
            }

            return carryBase * factorSex * factorAge * factorHunger * factorThirst * factorFatigue;
        } 
    }
}
