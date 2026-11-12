using System;
using System.Collections.Generic;
using Core.Observer;
using Core.Utils;

namespace Core.ECS.Component
{
    /// <summary>
    /// Componente basico de cualquier ItemEntity. Guarda el typeId que apunta al
    /// concepto de item del catalogo y el estado mutable propio de cada instancia
    /// (durabilidad, condicion). Los datos compartidos e inmutables (nombre, descripcion,
    /// peso, icono, dimensiones, maxStackSize) iran en el prototipo del catalogo
    /// cuando M1 este terminado.
    /// </summary>

    /// <summary>
    /// Metadatos para filtrar o mostrar en la interfaz.
    /// La logica de juego debe mirar si hay componentes (HasComponent), no esto.
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Tool,
        Food,
        Beverage,
        Consumable,
        Material,
        Placeable,
        Document,
        Generic,
        Part
    }

    public class BaseItemComponent : BasicComponent, IJsonLoadable, INumericFields
    {
        /// <summary>
        /// Concepto de item al que pertenece esta instancia (clave del catalogo).
        /// </summary>
        private int _typeId;
        
        /// <summary>
        /// Nombre generico del item o concepto. P. ej.: Espada (no "Tizona" ni "Excalibur").
        /// </summary>
        private string _genericName;

        /// <summary>
        /// Peso de la entidad.
        /// </summary>
        private float _weight;

        /// <summary>
        /// Pila maxima de la entidad en la rejilla del inventario.
        /// </summary>
        private int _maxStackSize;

        /// <summary>
        /// Durabilidad de la entidad.
        /// </summary>
        private float _durability;

        /// <summary>
        /// Durabilidad maxima de la entidad (puntos hasta que se rompe).
        /// </summary>
        private const float MAX_DURABILITY = 100;

        /// <summary>
        /// Condicion de la entidad (puntos hasta que funciona muy mal).
        /// </summary>
        private float _condition;

        /// <summary>
        /// Condicion maxima de la entidad.
        /// </summary>
        private const float MAX_CONDITION = 100;

        /// <summary>
        /// Descripcion del componente.
        /// </summary>
        private String _description;

        /// <summary>
        /// Ruta del icono del componente.
        /// </summary>
        private String _iconPath;

        /// <summary>
        /// Dimensiones del item en la rejilla del inventario (ancho x alto en celdas).
        /// Importan para el juego: deciden cuanto ocupa en la rejilla.
        /// </summary>
        private int _dimensionW;
        private int _dimensionH;

        public BaseItemComponent()
        {
            _dimensionW = 1;
            _dimensionH = 1;
        }

        public BaseItemComponent(int typeId, string genericName, float weight, int maxStackSize,
                                int dimensionW, int dimensionH,
                                float durability = MAX_DURABILITY,
                                float condition = MAX_CONDITION,
                                String description = "", String iconPath = "")
        {
            _typeId = typeId;
            _genericName = genericName;
            _weight = weight;
            _maxStackSize = maxStackSize;
            _durability = durability;
            _condition = condition;
            _dimensionW = dimensionW;
            _dimensionH = dimensionH;
            _description = description;
            _iconPath = iconPath;
        }
 

        /// <summary>
        /// Peso del componente.
        /// </summary>
        /// <returns>el peso</returns>
        public float Weight => _weight;

        /// <summary>
        /// Fija el peso del componente.
        /// </summary>
        public void SetWeight(float weight)
        {
            _weight = weight;
        }

        /// <summary>
        /// Pila maxima.
        /// </summary>
        /// <returns>la pila maxima</returns>
        public int MaxStackSize => _maxStackSize;

        /// <summary>
        /// Fija la pila maxima.
        /// </summary>
        public void SetMaxStackSize(int maxStackSize)
        {
            _maxStackSize = maxStackSize;
        }

        /// <summary>
        /// Durabilidad del componente.
        /// </summary>
        /// <returns>la durabilidad</returns>
        public float Durability => _durability;

        /// <summary>
        /// Fija la durabilidad del componente.
        /// </summary>
        public void SetDurability(int durability)
        {
            _durability = Math.Clamp(durability, 0, MAX_DURABILITY);
        }

        /// <summary>
        /// Condicion del componente.
        /// </summary>
        /// <returns>la condicion</returns>
        public float Condition => _condition;

        /// <summary>
        /// Fija la condicion del componente.
        /// </summary>
        public void SetCondition(float condition)
        {
            _condition = Math.Clamp(condition, 0, MAX_CONDITION);
        }

        /// <summary>
        /// Descripcion del componente.
        /// </summary>
        /// <returns>la descripcion</returns>
        public String Description => _description;

        /// <summary>
        /// Fija la descripcion del componente.
        /// </summary>
        public void SetDescription(String description)
        {
            _description = description;
        }

        /// <summary>
        /// Ruta del icono del componente.
        /// </summary>
        /// <returns>la ruta del icono</returns>
        public String IconPath => _iconPath;

        /// <summary>
        /// Fija la ruta del icono del componente.
        /// </summary>
        public void SetIconPath(String iconPath)
        {
            _iconPath = iconPath;
        }

        /// <summary>
        /// typeId del catalogo al que pertenece esta instancia.
        /// </summary>
        public int TypeId => _typeId;

        public void SetTypeId(int typeId)
        {
            _typeId = typeId;
        }

        public string GenericName => _genericName;

        public void SetGenericName(string genericName)
        {
            _genericName = genericName;
        }

        public int DimensionW => _dimensionW;
        public int DimensionH => _dimensionH;

        public void SetDimensions(int w, int h)
        {
            _dimensionW = w;
            _dimensionH = h;
        }

        /// <summary>
        /// Campos numericos, declarados una sola vez para las dos direcciones.
        ///
        /// Las dimensiones se ofrecen aunque sean casillas de la rejilla del inventario y no
        /// medidas del mundo. Son numeros del item y alguien podria querer compararlos; lo
        /// que no deben hacer es acabar usandose como tamano fisico.
        /// </summary>
        private static readonly NumericFields<BaseItemComponent> _fields =
            new NumericFields<BaseItemComponent>()
                .Add("weight", c => c._weight, (c, v) => c.SetWeight(v))
                .Add("maxStackSize", c => c._maxStackSize, (c, v) => c.SetMaxStackSize((int)v))
                .Add("durability", c => c._durability, (c, v) => c.SetDurability((int)v))
                .Add("condition", c => c._condition, (c, v) => c.SetCondition(v))
                .Add("dimensionW", c => c._dimensionW, (c, v) => c._dimensionW = (int)v)
                .Add("dimensionH", c => c._dimensionH, (c, v) => c._dimensionH = (int)v);

        public bool TryGetNumericValue(string field, out float value)
            => _fields.TryGet(this, field, out value);

        public void SetFromValues(Dictionary<string, object> values)
        {
            _fields.Apply(this, values);

            if (values.ContainsKey("description")) SetDescription(values["description"].ToString());
            if (values.ContainsKey("iconPath")) SetIconPath(values["iconPath"].ToString());
        }

        public override IComponent Clone()
        {
            return new BaseItemComponent(_typeId, _genericName, _weight, _maxStackSize,
                                        _dimensionW, _dimensionH,
                                        _durability,
                                        _condition,
                                        _description,
                                        _iconPath);
        }

        public override bool Equivalent(IComponent other)
        {
            return
                other is BaseItemComponent otherBase &&
                _typeId == otherBase._typeId &&
                _weight == otherBase._weight &&
                _maxStackSize == otherBase._maxStackSize &&
                _durability == otherBase._durability &&
                _condition == otherBase._condition &&
                _description == otherBase._description &&
                _iconPath == otherBase._iconPath;
        }
    }
}
