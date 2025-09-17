using System;

namespace ECS.Component
{
    /// <summary>
    /// Representa un atributo concreto referido a los puntos de daño y poder crítico de una entidad.
    /// </summary>
    public class DamageComponent : BasicComponent
    {
        private int baseDamage;              // Daño base
        private float criticalChance;        // Probabilidad de crítico (0.0 - 1.0)
        private float criticalMultiplier;    // Multiplicador de daño crítico

        private static readonly Random rng = new Random(); // Generador de números aleatorios

        public DamageComponent(int baseDamage, float criticalChance, float criticalMultiplier)
        {
            this.baseDamage = baseDamage;
            this.criticalChance = criticalChance;
            this.criticalMultiplier = criticalMultiplier;
            this.name = "DamageComponent"; // Inicializa el nombre del componente
        }

        public override IComponent Clone()
        {
            return new DamageComponent(this.baseDamage, this.criticalChance, this.criticalMultiplier);
        }

        public int BaseDamage
        {
            get => baseDamage;
            set => baseDamage = value;
        }

        public float CriticalChance
        {
            get => criticalChance;
            set => criticalChance = value;
        }

        public float CriticalMultiplier
        {
            get => criticalMultiplier;
            set => criticalMultiplier = value;
        }

        public int CalculateDamage()
        {
            // Calcular si es crítico
            if (rng.NextDouble() < criticalChance)
            {
                return (int)(baseDamage * criticalMultiplier);
            }
            return baseDamage;
        }

        public override string ToString()
        {
            return $"DamageComponent{{ baseDamage={baseDamage}, criticalChance={criticalChance}, criticalMultiplier={criticalMultiplier} }}";
        }
    }
}
