namespace ECS.Component
{
    /// <summary>
    /// Componente que representa la capacidad de curación de una entidad.
    /// </summary>
    public class HealComponent : BasicComponent
    {
        private int healingAmount;       // Cantidad de curación base
        private float bonusMultiplier;   // Multiplicador de bonificación

        public HealComponent(int healingAmount, float bonusMultiplier)
        {
            this.healingAmount = healingAmount;
            this.bonusMultiplier = bonusMultiplier;
            this.name = "HealComponent"; // Inicializa el nombre del componente
        }

        public override IComponent Clone()
        {
            return new HealComponent(this.healingAmount, this.bonusMultiplier); // Clona el componente
        }

        // Getters y setters en estilo C#
        public int HealingAmount
        {
            get => healingAmount;
            set => healingAmount = value;
        }

        public float BonusMultiplier
        {
            get => bonusMultiplier;
            set => bonusMultiplier = value;
        }

        public int CalculateHealing()
        {
            return (int)(healingAmount * bonusMultiplier);
        }

        public override string ToString()
        {
            return $"HealComponent{{healingAmount={healingAmount}, bonusMultiplier={bonusMultiplier}}}";
        }
    }
}
