using System;
using System.Collections.Generic;
using Observer;
using Utils;

namespace ECS.Component
{
    /// <summary>
    /// Core component for any ItemEntity. Holds the typeId referencing the
    /// item concept in the catalog, plus instance-specific mutable state
    /// (durability, condition). Shared/immutable data (name, description,
    /// weight, icon, dimensions, maxStackSize) will live in the catalog
    /// prototype once M1 is complete.
    /// </summary>

    /// <summary>
    /// UI metadata enum for filtering/display purposes.
    /// Mechanical logic should use component presence (HasComponent) instead.
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

    public class BaseItemComponent : BasicComponent, IJsonLoadable
    {
        /// <summary>
        /// References the item concept this instance belongs to (catalog key).
        /// </summary>
        private int _typeId;
        
        /// <summary>
        /// Generic name of the item/concept name. Ex: Sword (not "Tizona" or "Excalibur").
        /// </summary>
        private string _genericName;

        /// <summary>
        /// Weight of the entity.
        /// </summary>
        private float _weight;

        /// <summary>
        /// Max stack of the entity in the grid-like inventory.
        /// </summary>
        private int _maxStackSize;

        /// <summary>
        /// Durability of the entity.
        /// </summary>
        private float _durability;

        /// <summary>
        /// Max durability of the entity. (points until it breaks)
        /// </summary>
        private const float _maxDurability = 100;

        /// <summary>
        /// Condition of the entity. (points until it works very poorly)
        /// </summary>
        private float _condition;

        /// <summary>
        /// Max condition of the entity.
        /// </summary>
        private const float _maxCondition = 100;

        /// <summary>
        /// Description of the component.
        /// </summary>
        private String _description;

        /// <summary>
        /// Icon path of the component.
        /// </summary>
        private String _iconPath;

        /// <summary>
        /// Item dimensions in inventory grid (w×h cells). Mechanically
        /// relevant — determines grid footprint.
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
                                float durability = _maxDurability,
                                float condition = _maxCondition,
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
        /// Gets the weight of the component.
        /// </summary>
        /// <returns>the weight</returns>
        public float Weight => _weight;

        /// <summary>
        /// Sets the weight of the component.
        /// </summary>
        public void SetWeight(float weight)
        {
            _weight = weight;
        }

        /// <summary>
        /// Gets the maximun stack size.
        /// </summary>
        /// <returns>the maximun stack size</returns>
        public int MaxStackSize => _maxStackSize;

        /// <summary>
        /// Sets the maximun stack size.
        /// </summary>
        public void SetMaxStackSize(int maxStackSize)
        {
            _maxStackSize = maxStackSize;
        }

        /// <summary>
        /// Gets the durability of the component.
        /// </summary>
        /// <returns>the durability</returns>
        public float Durability => _durability;

        /// <summary>
        /// Sets the durability of the component.
        /// </summary>
        public void SetDurability(int durability)
        {
            _durability = Math.Clamp(durability, 0, _maxDurability);
        }

        /// <summary>
        /// Gets the condition of the component.
        /// </summary>
        /// <returns>the condition</returns>
        public float Condition => _condition;

        /// <summary>
        /// Sets the condition of the component.
        /// </summary>
        public void SetCondition(float condition)
        {
            this._condition = Math.Clamp(condition, 0, _maxCondition);
        }

        /// <summary>
        /// Gets the description of the component.
        /// </summary>
        /// <returns>the description</returns>
        public String Description => _description;

        /// <summary>
        /// Sets the description of the component.
        /// </summary>
        public void SetDescription(String description)
        {
            _description = description;
        }

        /// <summary>
        /// Gets the icon path of the component.
        /// </summary>
        /// <returns>the icon path</returns>
        public String IconPath => _iconPath;

        /// <summary>
        /// Sets the icon path of the component.
        /// </summary>
        public void SetIconPath(String iconPath)
        {
            _iconPath = iconPath;
        }

        /// <summary>
        /// Gets the catalog typeId this item instance belongs to.
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

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("weight")) SetWeight(Convert.ToSingle(values["weight"]));
            if (values.ContainsKey("maxStackSize")) SetMaxStackSize(Convert.ToInt32(values["maxStackSize"]));
            if (values.ContainsKey("durability")) SetDurability(Convert.ToInt32(values["durability"]));
            if (values.ContainsKey("condition")) SetCondition(Convert.ToSingle(values["condition"]));
            if (values.ContainsKey("description")) SetDescription(values["description"].ToString());
            if (values.ContainsKey("iconPath")) SetIconPath(values["iconPath"].ToString());
            if (values.ContainsKey("dimensionW")) _dimensionW = Convert.ToInt32(values["dimensionW"]);
            if (values.ContainsKey("dimensionH")) _dimensionH = Convert.ToInt32(values["dimensionH"]);
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
                this._typeId == otherBase._typeId &&
                this._weight == otherBase._weight &&
                this._maxStackSize == otherBase._maxStackSize &&
                this._durability == otherBase._durability &&
                this._condition == otherBase._condition &&
                this._description == otherBase._description &&
                this._iconPath == otherBase._iconPath;
        }
    }
}
