using System;
using System.Collections.Generic;

namespace Core.ECS.Component.InventoryComponents
{

    /// <summary>
    /// Guarda la capacidad de dano de un objeto.
    /// </summary>
    public class DamageComponent : IComponent, IJsonLoadable
    {

        #region Atributes
        /// <summary>
        /// Eficacia del dano PERFORANTE
        /// </summary>
        private DamageLevel _punctureDmg;

        /// <summary>
        /// Eficacia del dano CORTANTE
        /// </summary>
        private DamageLevel _slashingDmg;

        /// <summary>
        /// Eficacia del dano CONTUNDENTE
        /// </summary>
        private DamageLevel _impactDmg;

        #endregion

        public DamageComponent() {}

        public DamageComponent(DamageLevel punc, DamageLevel slash, DamageLevel impc)
        {
            _punctureDmg = punc;
            _slashingDmg = slash;
            _impactDmg = impc;
        }

        #region Getters & Setters
        /// <summary>
        /// Nivel de dano **perforante** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor de <see cref="DamageLevel"/> con la eficacia del dano perforante.
        /// </returns>
        public DamageLevel PunctureDmg => _punctureDmg;

        /// <summary>
        /// Fija el nivel de dano **perforante** del objeto.
        /// </summary>
        /// <param name="level">Nivel de dano, de tipo <see cref="DamageLevel"/>.</param>
        public void SetPunctureDmg(DamageLevel level)
        {
            _punctureDmg = level;
        }


        /// <summary>
        /// Nivel de dano **cortante** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor de <see cref="DamageLevel"/> con la eficacia del dano cortante.
        /// </returns>
        public DamageLevel SlashingDmg => _slashingDmg;

        /// <summary>
        /// Fija el nivel de dano **cortante** del objeto.
        /// </summary>
        /// <param name="level">Nivel de dano, de tipo <see cref="DamageLevel"/>.</param>
        public void SetSlashingDmg(DamageLevel level)
        {
            _slashingDmg = level;
        }


        /// <summary>
        /// Nivel de dano **contundente** del objeto.
        /// </summary>
        /// <returns>
        /// Un valor de <see cref="DamageLevel"/> con la eficacia del dano contundente.
        /// </returns>
        public DamageLevel ImpactDmg => _impactDmg;

        /// <summary>
        /// Fija el nivel de dano **contundente** del objeto.
        /// </summary>
        /// <param name="level">Nivel de dano, de tipo <see cref="DamageLevel"/>.</param>
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
            return new DamageComponent(_punctureDmg, _slashingDmg, _impactDmg);
        }

        public bool Equivalent(IComponent other)
        {
            if (other is DamageComponent otherDmg)
            {
                return _punctureDmg == otherDmg._punctureDmg &&
                       _slashingDmg == otherDmg._slashingDmg &&
                       _impactDmg == otherDmg._impactDmg;
            }
            return false;
        }
        #endregion
    }

}