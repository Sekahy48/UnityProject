using System;

namespace ECS.Component
{
    /// <summary>
    /// Represents a specific attribute referring to an entity's health points.
    /// </summary>
    public class HealthComponent : BasicComponent
    {
        private int currentHealth; // Current health
        private int maxHealth;     // Max health
        public const int UNLIMITED_HEALTH = -1;

        /// <summary>
        /// Creates a health component with current health and a max value.
        /// </summary>
        public HealthComponent(int current, int max)
        {
            this.currentHealth = current;
            this.maxHealth = max == UNLIMITED_HEALTH ? UNLIMITED_HEALTH : Math.Max(0, max);
            this._name = "HealthComponent";
        }

        /// <summary>
        /// Creates a health component with current health and an unlimited max.
        /// </summary>
        public HealthComponent(int current)
        {
            this.currentHealth = current;
            this.maxHealth = UNLIMITED_HEALTH;
            this._name = "HealthComponent";
        }

        /// <summary>
        /// Decreases current health (down to zero at most).
        /// </summary>
        public void ReceiveDamage(int damage)
        {
            if (damage < 0) return;
            this.currentHealth = Math.Max(0, this.currentHealth - damage);
        }

        /// <summary>
        /// Increases current health (up to the max at most).
        /// </summary>
        public void HealHealth(int heal)
        {
            this.currentHealth += heal;
            ClampHealth();
        }

        /// <summary>
        /// Decreases current health by a percentage.
        /// </summary>
        public void ReceiveDamagePercentage(double percentage)
        {
            int damage = (int)(maxHealth * percentage);
            ReceiveDamage(damage);
        }

        /// <summary>
        /// Increases current health by a percentage (up to the max).
        /// </summary>
        public void HealPercentage(double percentage)
        {
            int heal = (int)(maxHealth * percentage);
            HealHealth(heal);
        }

        private bool IsAlive()
        {
            return currentHealth > 0;
        }

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void ClampHealth()
        {
            if (maxHealth != UNLIMITED_HEALTH && currentHealth > maxHealth)
                currentHealth = maxHealth;
            if (currentHealth < 0)
                currentHealth = 0;
        }

        public bool IsDead()
        {
            return currentHealth <= 0;
        }

        public override IComponent Clone()
        {
            return new HealthComponent(this.currentHealth, this.maxHealth);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is HealthComponent otherHealth &&
                this.currentHealth == otherHealth.currentHealth &&
                this.maxHealth == otherHealth.maxHealth;
        }
    }
}
