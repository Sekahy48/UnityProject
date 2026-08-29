using System;
using ECS.Component;
using ECS.Entity;
using Events;
using Inventory;

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

        /// <summary>
        /// Weight limit of whatever holds an inventory. A body carries what its muscles allow;
        /// anything else (a chest, a cart) is limited by its own StorageComponent. Single rule,
        /// so the UI bar and the transfer check can never disagree about the ceiling.
        /// </summary>
        public static float GetMaxLoad(IEntity entity)
        {
            if (entity.HasComponent(typeof(BodyComponent)))
                return GetMaxCarryWeight(entity);

            StorageComponent storage = entity.GetComponent<StorageComponent>();
            return storage != null ? storage.MaxWeight : float.MaxValue;
        }

        /// <summary>
        /// Cuantas de esas <paramref name="amount"/> unidades caben todavia por peso. Pura: no
        /// toca nada, asi que sirve igual para ejecutar el movimiento y para pintar el veredicto
        /// antes de soltar. Vive aqui, junto al techo que consulta, para que ambos usos no puedan
        /// discrepar sobre cuanto queda libre.
        /// </summary>
        public static int FitByWeight(IEntity entity, InventoryObject inventory, ItemEntity item, int amount)
        {
            if (inventory == null) return 0;

            float itemWeight = item.GetComponent<BaseItemComponent>().Weight;
            if (itemWeight <= 0) return amount;   // sin peso no hay limite que aplicar

            float free = GetMaxLoad(entity) - inventory.GetTotalWeight();
            int fit = (int)(free / itemWeight);

            // Sobrepasado el techo el hueco libre es negativo: no cabe nada, no "cabe menos que nada".
            return Math.Min(amount, Math.Max(fit, 0));
        }

        public static float GetMaxCarryWeight(IEntity entity)
        {
            BodyComponent body = entity.GetComponent<BodyComponent>();
            EnergyComponent energy = entity.GetComponent<EnergyComponent>();
            NutritionComponent nutrition = entity.GetComponent<NutritionComponent>();

            if (body == null) return 0f;

            float muscleMass = body.GetMuscleMass();
            float carryBase = muscleMass * 0.5f;
            float factorSex = (body.Sex == 0) ? 1.0f : 0.85f;

            float age = body.Age;
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
                float maxHunger = nutrition.MaxHunger;
                float maxThirst = nutrition.MaxThirst;
                if (maxHunger > 0) factorHunger  = 1.0f - (nutrition.Hunger / maxHunger) * 0.30f;
                if (maxThirst > 0) factorThirst  = 1.0f - (nutrition.Thirst / maxThirst) * 0.40f;
            }

            if (energy != null)
            {
                float maxFatigue = energy.MaxFatigue;
                if (maxFatigue > 0) factorFatigue = 1.0f - (energy.Fatigue / maxFatigue) * 0.35f;
            }

            return carryBase * factorSex * factorAge * factorHunger * factorThirst * factorFatigue;
        } 
    }
}
