using System;
using System.Numerics;
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
        WEAPON,
        TOOL,
        FOOD,
        BEVERAGE,
        CONSUMABLE,
        MATERIAL,
        PLACEABLE,
        DOCUMENT,
        GENERIC,
        PART
    }

    public class BaseItemComponent : BasicComponent
    {
        /// <summary>
        /// References the item concept this instance belongs to (catalog key).
        /// </summary>
        private int _typeId;

        /// <summary>
        /// Weight of the entity.
        /// </summary>
        private float _weight;

        /// <summary>
        /// Volume of the entity.
        /// </summary>
        private float _volume;

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
        private const float maxCondition = 100;

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
        private Vector2 _dimensions;

        public BaseItemComponent(int typeId, float weight, float volume,
                                Vector2 dimensions,
                                float durability = _maxDurability,
                                float condition = maxCondition,
                                String description = "", String iconPath = "")
        {
            ArgumentChecker.CheckNotNull(dimensions, "Dimensions cannot be null");
            _typeId = typeId;
            _weight = weight;
            _volume = volume;
            _durability = durability;
            _condition = condition;
            _dimensions = dimensions;
            _description = description;
            _iconPath = iconPath;
        }
 

        /// <summary>
        /// Gets the weight of the component.
        /// </summary>
        /// <returns>the weight</returns>
        public float GetWeight()
        {
            return _weight;
        }

        /// <summary>
        /// Sets the weight of the component.
        /// </summary>
        public void SetWeight(float weight)
        {
            _weight = weight;
        }

        /// <summary>
        /// Gets the volume of the component.
        /// </summary>
        /// <returns>the volume</returns>
        public float GetVolume()
        {
            return _volume;
        }

        /// <summary>
        /// Sets the volume of the component.
        /// </summary>
        public void SetVolume(float volume)
        {
            _volume = volume;
        }

        /// <summary>
        /// Gets the durability of the component.
        /// </summary>
        /// <returns>the durability</returns>
        public float GetDurability()
        {
            return _durability;
        }

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
        public float GetCondition()
        {
            return _condition;
        }

        /// <summary>
        /// Sets the condition of the component.
        /// </summary>
        public void SetCondition(float condition)
        {
            this._condition = Math.Clamp(condition, 0, maxCondition);
        }

        /// <summary>
        /// Gets the description of the component.
        /// </summary>
        /// <returns>the description</returns>
        public String GetDescription()
        {
            return _description;
        }

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
        public String GetIconPath()
        {
            return _iconPath;
        }

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
        public int GetTypeId()
        {
            return _typeId;
        }

        public override IComponent Clone()
        {
            return new BaseItemComponent(_typeId, _weight, _volume,
                                        _dimensions,
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
                this._volume == otherBase._volume &&
                this._durability == otherBase._durability &&
                this._condition == otherBase._condition &&
                this._description == otherBase._description &&
                this._iconPath == otherBase._iconPath;
        }
    }
}
