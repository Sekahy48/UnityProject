using System;
using System.Threading;

namespace ECS.Component
{
    /// <summary>
    /// Representa un atributo concreto referido a los puntos de salud de una entidad.
    /// </summary>
    public class HealthComponent : BasicComponent
    {
        private int currentHealth; // Vida actual
        private int maxHealth;     // Vida máxima
        public const int UNLIMITED_HEALTH = -1;

        /// <summary>
        /// Crea un componente de salud con vida current y máximo max.
        /// </summary>
        public HealthComponent(int current, int max)
        {
            this.currentHealth = current;
            this.maxHealth = max == UNLIMITED_HEALTH ? UNLIMITED_HEALTH : Math.Max(0, max);
            this.name = "HealthComponent";
        }

        /// <summary>
        /// Crea un componente de salud con vida current y máximo indefinido.
        /// </summary>
        public HealthComponent(int current)
        {
            this.currentHealth = current;
            this.maxHealth = UNLIMITED_HEALTH;
            this.name = "HealthComponent";
        }

        public override IComponent Clone()
        {
            return new HealthComponent(this.currentHealth, this.maxHealth);
        }

        /// <summary>
        /// Disminuye la vida actual (hasta cero como mucho).
        /// </summary>
        public void ReceiveDamage(int damage)
        {
            if (damage < 0) return;
            this.currentHealth = Math.Max(0, this.currentHealth - damage);
        }

        /// <summary>
        /// Incrementa la vida actual (hasta el máximo como mucho).
        /// </summary>
        public void HealHealth(int heal)
        {
            this.currentHealth += heal;
            ClampHealth();
        }

        /// <summary>
        /// Decrementa la vida actual en un porcentaje.
        /// </summary>
        public void ReceiveDamagePercentage(double percentage)
        {
            int damage = (int)(maxHealth * percentage);
            ReceiveDamage(damage);
        }

        /// <summary>
        /// Incrementa la vida actual en un porcentaje (hasta el máximo).
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

        /// <summary>
        /// Recibe daño a lo largo del tiempo.
        /// </summary>
        public void ReceiveDamageOverTime(int totalDamage, int timeInMilliseconds)
        {
            int damagePerTick = totalDamage / (timeInMilliseconds / 1000);
            int elapsedTime = 0;

            while (elapsedTime < timeInMilliseconds)
            {
                ReceiveDamage(damagePerTick);
                elapsedTime += 1000;
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <summary>
        /// Cura a lo largo del tiempo.
        /// </summary>
        public void HealOverTime(int totalHeal, int timeInMilliseconds)
        {
            int healPerTick = totalHeal / (timeInMilliseconds / 1000);
            int elapsedTime = 0;

            while (elapsedTime < timeInMilliseconds)
            {
                HealHealth(healPerTick);
                elapsedTime += 1000;
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

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

        /// <summary>
        /// Recibe daño a lo largo del tiempo en porcentaje.
        /// </summary>
        public void ReceiveDamagePercentageOverTime(double percentage, int timeInMilliseconds)
        {
            int damagePerTick = (int)(maxHealth * percentage);
            int elapsedTime = 0;

            while (elapsedTime < timeInMilliseconds)
            {
                ReceiveDamage(damagePerTick);
                elapsedTime += 1000;
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }

        /// <summary>
        /// Cura a lo largo del tiempo en porcentaje.
        /// </summary>
        public void HealPercentageOverTime(double percentage, int timeInMilliseconds)
        {
            int healPerTick = (int)(maxHealth * percentage);
            int elapsedTime = 0;

            while (elapsedTime < timeInMilliseconds)
            {
                HealHealth(healPerTick);
                elapsedTime += 1000;
                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }
        }
    }
}
