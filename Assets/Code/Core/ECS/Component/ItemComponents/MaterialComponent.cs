using System;
using System.Collections.Generic;

namespace ECS.Component.InventoryComponents
{
    public enum MaterialType
    {
        Wood,
        Cloth,
        Metal,
        Stone,
        Bone
    }
    
    /// <summary>
    /// Gets the general category/type of the material.
    ///
    public class MaterialComponent : IComponent, IJsonLoadable
    {
        #region Atributes
        /// <summary>
        /// Represents the general category/type of the material
        /// </summary>
        private MaterialType _materialType;

        /// <summary>
        /// Name of the instance
        /// </summary>
        private String _materialName;

        /// <summary>
        /// Represents how flexible it is
        /// </summary>
        private float _flexibility;

        /// <summary>
        /// Represents how hard it is
        /// </summary>
        private float _hardness;

        /// <summary>
        /// Represents how breathable it is
        /// </summary>
        private float _transpirability;

        /// <summary>
        /// Represents how insulating it is
        /// </summary>
        private float _thermalInsulation;

        #endregion

        public MaterialComponent() {}

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
        /// Gets the general type of the material.
        /// </summary>
        /// <returns>The material type.</returns>
        public MaterialType GetMaterialType()
        {
            return _materialType;
        }

        /// <summary>
        /// Sets the general type of the material.
        /// </summary>
        /// <param name="materialType">The material type to assign.</param>
        public void SetMaterialType(MaterialType materialType)
        {
            _materialType = materialType;
        }

        /// <summary>
        /// Gets the name of the material.
        /// </summary>
        /// <returns>The material name.</returns>
        public string GetMaterialName()
        {
            return _materialName;
        }

        /// <summary>
        /// Sets the name of the material.
        /// </summary>
        /// <param name="materialName">The new material name.</param>
        public void SetMaterialName(string materialName)
        {
            _materialName = materialName;
        }

        /// <summary>
        /// Gets the flexibility level of the material.
        /// </summary>
        /// <returns>The flexibility level.</returns>
        public float GetFlexibility()
        {
            return _flexibility;
        }

        /// <summary>
        /// Sets the flexibility level of the material.
        /// </summary>
        /// <param name="flexibility">The new flexibility level.</param>
        public void SetFlexibility(float flexibility)
        {
            _flexibility = flexibility;
        }

        /// <summary>
        /// Gets the hardness level of the material.
        /// </summary>
        /// <returns>The hardness level.</returns>
        public float GetHardness()
        {
            return _hardness;
        }

        /// <summary>
        /// Sets the hardness level of the material.
        /// </summary>
        /// <param name="hardness">The new hardness level.</param>
        public void SetHardness(float hardness)
        {
            _hardness = hardness;
        }

        /// <summary>
        /// Gets the breathability level of the material.
        /// </summary>
        /// <returns>The breathability level.</returns>
        public float GetTranspirability()
        {
            return _transpirability;
        }

        /// <summary>
        /// Sets the breathability level of the material.
        /// </summary>
        /// <param name="transpirability">The new breathability level.</param>
        public void SetTranspirability(float transpirability)
        {
            _transpirability = transpirability;
        }

        /// <summary>
        /// Gets the thermal insulation level of the material.
        /// </summary>
        /// <returns>The thermal insulation level.</returns>
        public float GetThermalInsulation()
        {
            return _thermalInsulation;
        }

        /// <summary>
        /// Sets the thermal insulation level of the material.
        /// </summary>
        /// <param name="thermalInsulation">The new thermal insulation level.</param>
        public void SetThermalInsulation(float thermalInsulation)
        {
            _thermalInsulation = thermalInsulation;
        }

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("materialType")) SetMaterialType(Enum.Parse<MaterialType>(values["materialType"].ToString(), true));
            if (values.ContainsKey("materialName")) SetMaterialName(values["materialName"].ToString());
            if (values.ContainsKey("flexibility")) SetFlexibility(Convert.ToSingle(values["flexibility"]));
            if (values.ContainsKey("hardness")) SetHardness(Convert.ToSingle(values["hardness"]));
            if (values.ContainsKey("transpirability")) SetTranspirability(Convert.ToSingle(values["transpirability"]));
            if (values.ContainsKey("thermalInsulation")) SetThermalInsulation(Convert.ToSingle(values["thermalInsulation"]));
        }

        public IComponent Clone()
        {
            return new MaterialComponent(_materialType, _materialName, _flexibility, _hardness, _transpirability, _thermalInsulation);
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