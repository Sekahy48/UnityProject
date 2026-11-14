using System;
using System.Collections.Generic;

namespace Core.ECS.Component.InventoryComponents
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
    /// Material de un item: tipo, nombre y propiedades fisicas.
    /// </summary>
    public class MaterialComponent : IComponent, IJsonLoadable, INumericFields
    {
        #region Atributes
        /// <summary>
        /// Categoria o tipo general del material
        /// </summary>
        private MaterialType _materialType;

        /// <summary>
        /// Nombre de la instancia
        /// </summary>
        private String _materialName;

        /// <summary>
        /// Lo flexible que es
        /// </summary>
        private float _flexibility;

        /// <summary>
        /// Lo duro que es
        /// </summary>
        private float _hardness;

        /// <summary>
        /// Lo transpirable que es
        /// </summary>
        private float _transpirability;

        /// <summary>
        /// Lo aislante que es
        /// </summary>
        private float _thermalInsulation;

        #endregion

        public MaterialComponent() {}

        public MaterialComponent(MaterialType type, String name, float flex, float hard, float transpir, float thermIns)
        {
            _materialType = type;
            _materialName = name;
            _flexibility = flex;
            _hardness = hard;
            _transpirability = transpir;
            _thermalInsulation = thermIns;
        }
        
        #region Setters & Getters 
        
         /// <summary>
        /// Tipo general del material.
        /// </summary>
        /// <returns>El tipo de material.</returns>
        public MaterialType MaterialType => _materialType;

        /// <summary>
        /// Fija el tipo general del material.
        /// </summary>
        /// <param name="materialType">El tipo de material.</param>
        public void SetMaterialType(MaterialType materialType)
        {
            _materialType = materialType;
        }

        /// <summary>
        /// Nombre del material.
        /// </summary>
        /// <returns>El nombre del material.</returns>
        public string MaterialName => _materialName;

        /// <summary>
        /// Fija el nombre del material.
        /// </summary>
        /// <param name="materialName">El nombre nuevo.</param>
        public void SetMaterialName(string materialName)
        {
            _materialName = materialName;
        }

        /// <summary>
        /// Nivel de flexibilidad del material.
        /// </summary>
        /// <returns>El nivel de flexibilidad.</returns>
        public float Flexibility => _flexibility;

        /// <summary>
        /// Fija el nivel de flexibilidad del material.
        /// </summary>
        /// <param name="flexibility">El nivel nuevo.</param>
        public void SetFlexibility(float flexibility)
        {
            _flexibility = flexibility;
        }

        /// <summary>
        /// Nivel de dureza del material.
        /// </summary>
        /// <returns>El nivel de dureza.</returns>
        public float Hardness => _hardness;

        /// <summary>
        /// Fija el nivel de dureza del material.
        /// </summary>
        /// <param name="hardness">El nivel nuevo.</param>
        public void SetHardness(float hardness)
        {
            _hardness = hardness;
        }

        /// <summary>
        /// Nivel de transpirabilidad del material.
        /// </summary>
        /// <returns>El nivel de transpirabilidad.</returns>
        public float Transpirability => _transpirability;

        /// <summary>
        /// Fija el nivel de transpirabilidad del material.
        /// </summary>
        /// <param name="transpirability">El nivel nuevo.</param>
        public void SetTranspirability(float transpirability)
        {
            _transpirability = transpirability;
        }

        /// <summary>
        /// Nivel de aislamiento termico del material.
        /// </summary>
        /// <returns>El nivel de aislamiento termico.</returns>
        public float ThermalInsulation => _thermalInsulation;

        /// <summary>
        /// Fija el nivel de aislamiento termico del material.
        /// </summary>
        /// <param name="thermalInsulation">El nivel nuevo.</param>
        public void SetThermalInsulation(float thermalInsulation)
        {
            _thermalInsulation = thermalInsulation;
        }

        /// <summary>
        /// Campos numericos. El tipo y el nombre del material quedan fuera por no serlo.
        /// </summary>
        private static readonly NumericFields<MaterialComponent> _fields =
            new NumericFields<MaterialComponent>()
                .Add("flexibility", c => c._flexibility, (c, v) => c.SetFlexibility(v))
                .Add("hardness", c => c._hardness, (c, v) => c.SetHardness(v))
                .Add("transpirability", c => c._transpirability, (c, v) => c.SetTranspirability(v))
                .Add("thermalInsulation", c => c._thermalInsulation, (c, v) => c.SetThermalInsulation(v));

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);

            if (values.ContainsKey("materialType")) SetMaterialType(Enum.Parse<MaterialType>(values["materialType"].ToString(), true));
            if (values.ContainsKey("materialName")) SetMaterialName(values["materialName"].ToString());
        }

        public IComponent Clone()
        {
            return new MaterialComponent(_materialType, _materialName, _flexibility, _hardness, _transpirability, _thermalInsulation);
        }

        public bool Equivalent(IComponent other)
        {
            if (other is MaterialComponent otherMaterial)
            {
                return _materialType == otherMaterial._materialType &&
                       _materialName == otherMaterial._materialName &&
                       _flexibility == otherMaterial._flexibility &&
                       _hardness == otherMaterial._hardness &&
                       _transpirability == otherMaterial._transpirability &&
                       _thermalInsulation == otherMaterial._thermalInsulation;
            }

            return false;
        }
        #endregion
    }
}