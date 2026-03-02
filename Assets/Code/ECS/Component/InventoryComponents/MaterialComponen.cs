using System;

namespace ECS.Component.InventoryComponents
{
    public enum MaterialType
    {
        WOOD,
        CLOTH,
        METAL,
        STONE,
        BONE
    }
    
    /// <summary>
    /// Obtiene la categoría/tipo general del material.
    ///
    public class MaterialComponent : IComponent
    {
        #region Atributes
        /// <summary>
        /// Representa la categoría/tipo general del material
        /// </summary>
        private MaterialType _materialType;

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        private String _materialName;

        /// <summary>
        /// Representa como de flexible es
        /// </summary>
        private float _flexibility;

        /// <summary>
        /// Representa como de duro es
        /// </summary>
        private float _hardness;

        /// <summary>
        /// Representa como de transpirable es
        /// </summary>
        private float _transpirability;

        /// <summary>
        /// Representa como de aislante es
        /// </summary>
        private float _thermalInsulation;

        #endregion

        public MaterialComponent(MaterialType type, String name, float flex, float hard, float transpir, float thermIns)
        {
            this._materialType = type;
            this._materialName = name;
            this._flexibility = flex;
            this._hardness = hard;
            this._transpirability = transpir;
            this._thermalInsulation = thermIns;
        }
        
        #region Setters & Getters 
        
         /// <summary>
        /// Obtiene el tipo general del material.
        /// </summary>
        /// <returns>El tipo de material.</returns>
        public MaterialType GetMaterialType()
        {
            return _materialType;
        }

        /// <summary>
        /// Establece el tipo general del material.
        /// </summary>
        /// <param name="materialType">El tipo de material a asignar.</param>
        public void SetMaterialType(MaterialType materialType)
        {
            _materialType = materialType;
        }

        /// <summary>
        /// Obtiene el nombre del material.
        /// </summary>
        /// <returns>El nombre del material.</returns>
        public string GetMaterialName()
        {
            return _materialName;
        }

        /// <summary>
        /// Establece el nombre del material.
        /// </summary>
        /// <param name="materialName">El nuevo nombre del material.</param>
        public void SetMaterialName(string materialName)
        {
            _materialName = materialName;
        }

        /// <summary>
        /// Obtiene el nivel de flexibilidad del material.
        /// </summary>
        /// <returns>El nivel de flexibilidad.</returns>
        public float GetFlexibility()
        {
            return _flexibility;
        }

        /// <summary>
        /// Establece el nivel de flexibilidad del material.
        /// </summary>
        /// <param name="flexibility">El nuevo nivel de flexibilidad.</param>
        public void SetFlexibility(float flexibility)
        {
            _flexibility = flexibility;
        }

        /// <summary>
        /// Obtiene el nivel de dureza del material.
        /// </summary>
        /// <returns>El nivel de dureza.</returns>
        public float GetHardness()
        {
            return _hardness;
        }

        /// <summary>
        /// Establece el nivel de dureza del material.
        /// </summary>
        /// <param name="hardness">El nuevo nivel de dureza.</param>
        public void SetHardness(float hardness)
        {
            _hardness = hardness;
        }

        /// <summary>
        /// Obtiene el nivel de transpirabilidad del material.
        /// </summary>
        /// <returns>El nivel de transpirabilidad.</returns>
        public float GetTranspirability()
        {
            return _transpirability;
        }

        /// <summary>
        /// Establece el nivel de transpirabilidad del material.
        /// </summary>
        /// <param name="transpirability">El nuevo nivel de transpirabilidad.</param>
        public void SetTranspirability(float transpirability)
        {
            _transpirability = transpirability;
        }

        /// <summary>
        /// Obtiene el nivel de aislamiento térmico del material.
        /// </summary>
        /// <returns>El nivel de aislamiento térmico.</returns>
        public float GetThermalInsulation()
        {
            return _thermalInsulation;
        }

        /// <summary>
        /// Establece el nivel de aislamiento térmico del material.
        /// </summary>
        /// <param name="thermalInsulation">El nuevo nivel de aislamiento térmico.</param>
        public void SetThermalInsulation(float thermalInsulation)
        {
            _thermalInsulation = thermalInsulation;
        }

        public IComponent Clone()
        {
            //TODO
            throw new NotImplementedException();
        }

        public bool Equivalent(IComponent other)
        {
            if (other is MaterialComponent otherMaterial)
            {
                return this._materialType == otherMaterial._materialType &&
                       this._materialName == otherMaterial._materialName &&
                       this._flexibility == otherMaterial._flexibility &&
                       this._hardness == otherMaterial._hardness &&
                       this._transpirability == otherMaterial._transpirability &&
                       this._thermalInsulation == otherMaterial._thermalInsulation;
            }

            return false;
        }
        #endregion
    }
}