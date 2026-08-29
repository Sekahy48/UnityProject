using System;
using System.Collections.Generic;

namespace ECS.Component.InventoryComponents
{

    /// <summary>
    /// Class responsible for storing an object's damage capabilities
    /// </summary>
    public class DamageComponent : IComponent, IJsonLoadable
    {

        #region Atributes
        /// <summary>
        /// Effectiveness level of PIERCING damage
        /// </summary>
        private DamageLevel _punctureDmg;

        /// <summary>
        /// Effectiveness level of SLASHING damage
        /// </summary>
        private DamageLevel _slashingDmg;

        /// <summary>
        /// Effectiveness level of BLUNT damage
        /// </summary>
        private DamageLevel _impactDmg;

        #endregion

        public DamageComponent() {}

        public DamageComponent(DamageLevel punc, DamageLevel slash, DamageLevel impc)
        {
            this._punctureDmg = punc;
            this._slashingDmg = slash;
            this._impactDmg = impc;
        }

        #region Getters & Setters
        /// <summary>
        /// Gets the object's **piercing** damage level.
        /// </summary>
        /// <returns>
        /// A <see cref="DamageLevel"/> enum value representing the piercing damage effectiveness.
        /// </returns>
        public DamageLevel PunctureDmg => _punctureDmg;

        /// <summary>
        /// Sets the object's **piercing** damage level.
        /// </summary>
        /// <param name="level">Damage level of type <see cref="DamageLevel"/>.</param>
        public void SetPunctureDmg(DamageLevel level)
        {
            _punctureDmg = level;
        }


        /// <summary>
        /// Gets the object's **slashing** damage level.
        /// </summary>
        /// <returns>
        /// A <see cref="DamageLevel"/> enum value representing the slashing damage effectiveness.
        /// </returns>
        public DamageLevel SlashingDmg => _slashingDmg;

        /// <summary>
        /// Sets the object's **slashing** damage level.
        /// </summary>
        /// <param name="level">Damage level of type <see cref="DamageLevel"/>.</param>
        public void SetSlashingDmg(DamageLevel level)
        {
            _slashingDmg = level;
        }


        /// <summary>
        /// Gets the object's **blunt** damage level.
        /// </summary>
        /// <returns>
        /// A <see cref="DamageLevel"/> enum value representing the blunt damage effectiveness.
        /// </returns>
        public DamageLevel ImpactDmg => _impactDmg;

        /// <summary>
        /// Sets the object's **blunt** damage level.
        /// </summary>
        /// <param name="level">Damage level of type <see cref="DamageLevel"/>.</param>
        public void SetImpactDmg(DamageLevel level)
        {
            _impactDmg = level;
        }
        #endregion
 
        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("punctureDmg")) SetPunctureDmg(Enum.Parse<DamageLevel>(values["punctureDmg"].ToString(), true));
            if (values.ContainsKey("slashingDmg")) SetSlashingDmg(Enum.Parse<DamageLevel>(values["slashingDmg"].ToString(), true));
            if (values.ContainsKey("impactDmg")) SetImpactDmg(Enum.Parse<DamageLevel>(values["impactDmg"].ToString(), true));
        }

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