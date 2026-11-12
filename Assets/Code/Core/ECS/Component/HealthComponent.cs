using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Puntos de vida de una entidad.
    /// </summary>
    public class HealthComponent : BasicComponent
    {
        private int _currentHealth; // Vida actual
        private int _maxHealth;     // Vida maxima
        public const int UNLIMITED_HEALTH = -1;

        /// <summary>
        /// Crea un componente de vida con la vida actual y un maximo.
        /// </summary>
        public HealthComponent(int current, int max)
        {
            _currentHealth = current;
            _maxHealth = max == UNLIMITED_HEALTH ? UNLIMITED_HEALTH : Math.Max(0, max);
            Name = "HealthComponent";
        }

        /// <summary>
        /// Crea un componente de vida con la vida actual y sin maximo.
        /// </summary>
        public HealthComponent(int current)
        {
            _currentHealth = current;
            _maxHealth = UNLIMITED_HEALTH;
            Name = "HealthComponent";
        }

        /// <summary>
        /// Resta vida (como mucho, hasta cero).
        /// </summary>
        public void ReceiveDamage(int damage)
        {
            if (damage < 0) return;
            _currentHealth = Math.Max(0, _currentHealth - damage);
        }

        /// <summary>
        /// Suma vida (como mucho, hasta el maximo).
        /// </summary>
        public void HealHealth(int heal)
        {
            _currentHealth += heal;
            ClampHealth();
        }

        /// <summary>
        /// Resta un porcentaje de la vida.
        /// </summary>
        public void ReceiveDamagePercentage(double percentage)
        {
            int damage = (int)(_maxHealth * percentage);
            ReceiveDamage(damage);
        }

        /// <summary>
        /// Suma un porcentaje de la vida (como mucho, hasta el maximo).
        /// </summary>
        public void HealPercentage(double percentage)
        {
            int heal = (int)(_maxHealth * percentage);
            HealHealth(heal);
        }

        private bool IsAlive()
        {
            return _currentHealth > 0;
        }

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        private void ClampHealth()
        {
            if (_maxHealth != UNLIMITED_HEALTH && _currentHealth > _maxHealth)
                _currentHealth = _maxHealth;
            if (_currentHealth < 0)
                _currentHealth = 0;
        }

        public bool IsDead()
        {
            return _currentHealth <= 0;
        }

        public override IComponent Clone()
        {
            return new HealthComponent(_currentHealth, _maxHealth);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is HealthComponent otherHealth &&
                _currentHealth == otherHealth._currentHealth &&
                _maxHealth == otherHealth._maxHealth;
        }
    }
}
