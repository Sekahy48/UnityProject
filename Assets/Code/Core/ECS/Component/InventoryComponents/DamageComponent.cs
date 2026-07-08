namespace ECS.Component.InventoryComponents
{

    /// <summary>
    /// Clase encargada de almacenar las capacidades de daño de un objeto
    /// </summary>
    public class DamageComponent : IComponent
    {

        #region Atributes
        /// <summary>
        /// Nivel de efectividad de daño PENETRANTE 
        /// </summary>
        private DamageLevel _punctureDmg;

        /// <summary>
        /// Nivel de efectividad de daño CORTANTE 
        /// </summary>
        private DamageLevel _slashingDmg;

        /// <summary>
        /// Nivel de efectividad de daño CONTUNDENTE 
        /// </summary>
        private DamageLevel _impactDmg;

        #endregion

        public DamageComponent(DamageLevel punc, DamageLevel slash, DamageLevel impc)
        {
            this._punctureDmg = punc;
            this._slashingDmg = slash;
            this._impactDmg = impc;
        }

        #region Getters & Setters
        /// <summary>
        /// Obtiene el nivel de daño **penetrante** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor del enumerado <see cref="DamageLevel"/> que representa la efectividad del daño penetrante.
        /// </returns>
        public DamageLevel GetPunctureDmg()
        {
            return _punctureDmg;
        }

        /// <summary>
        /// Establece el nivel de daño **penetrante** del objeto.
        /// </summary>
        /// <param name="level">Nivel de daño de tipo <see cref="DamageLevel"/>.</param>
        public void SetPunctureDmg(DamageLevel level)
        {
            _punctureDmg = level;
        }


        /// <summary>
        /// Obtiene el nivel de daño **cortante** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor del enumerado <see cref="DamageLevel"/> que representa la efectividad del daño cortante.
        /// </returns>
        public DamageLevel GetSlashingDmg()
        {
            return _slashingDmg;
        }

        /// <summary>
        /// Establece el nivel de daño **cortante** del objeto.
        /// </summary>
        /// <param name="level">Nivel de daño de tipo <see cref="DamageLevel"/>.</param>
        public void SetSlashingDmg(DamageLevel level)
        {
            _slashingDmg = level;
        }


        /// <summary>
        /// Obtiene el nivel de daño **contundente** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor del enumerado <see cref="DamageLevel"/> que representa la efectividad del daño contundente.
        /// </returns>
        public DamageLevel GetImpactDmg()
        {
            return _impactDmg;
        }

        /// <summary>
        /// Establece el nivel de daño **contundente** del objeto.
        /// </summary>
        /// <param name="level">Nivel de daño de tipo <see cref="DamageLevel"/>.</param>
        public void SetImpactDmg(DamageLevel level)
        {
            _impactDmg = level;
        }
        #endregion
 
        #region Component
        public IComponent Clone()
        {
            return new DamageComponent(this._punctureDmg, this._slashingDmg, this._impactDmg);
        }

        public bool Equivalent(IComponent other)
        {
            if (other is DamageComponent otherDmg)
            {
                return this._punctureDmg == otherDmg._punctureDmg &&
                       this._slashingDmg == otherDmg._slashingDmg &&
                       this._impactDmg == otherDmg._impactDmg;
            }
            return false;
        }
        #endregion
    }

}