using System;
using System.Collections.Generic;
using System.Numerics;
using Observer;
using Utils;

namespace ECS.Component
{
    /// <summary>
    /// Basic component that implements IComponent and extends GenericSubject.
    /// As part of its functionality it contains the basic things every ItemEntity
    /// should have, as well as a name referring to the component type (inherited
    /// by whoever implements this class). Can be composed inside other entity
    /// types but this isn't expected to be leveraged.
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
        /// Type of the entity.
        /// </summary>
        private List<ItemType> _itemType;

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
        /// Item dimensions while in inventory, how much space it takes up visually,
        /// not in terms of calculating the remaining carry capacity of the inventory
        /// that holds it.
        /// </summary>
        private Vector2 _dimmensions;
        
        public BaseItemComponent(float weight, float volume, 
                                Vector2 dimmensions,
                                List<ItemType> itemType,
                                float durability = _maxDurability, 
                                float condition = maxCondition, 
                                String description = "", String iconPath = "")
        {   
            ArgumentChecker.CheckNotNull(dimmensions, "Dimmensions cannot be null");
            ArgumentChecker.CheckNotNull(itemType, "ItemType cannot be null");
            _weight = weight;
            _volume = volume;
            _durability = durability;
            _condition = condition;
            _dimmensions = dimmensions;
            _description = description;
            _iconPath = iconPath;
            _itemType = new List<ItemType>();

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
        /// Gets the type of the component.
        /// </summary>
        public List<ItemType> GetItemType()
        {
            return _itemType;
        }

        /// <summary>
        /// Sets the type of the component.
        /// </summary>
        public void SetItemType(List<ItemType> itemType)
        {
            _itemType = itemType;
        }
        
        public override IComponent Clone()
        {
            return new BaseItemComponent(_weight, _volume,
                                        _dimmensions,
                                        _itemType,
                                        _durability,
                                        _condition,
                                        _description,
                                        _iconPath);
        }

        public override bool Equivalent(IComponent other)
        {
            return 
                other is BaseItemComponent otherBase &&
                this._weight == otherBase._weight &&
                this._volume == otherBase._volume &&
                this._durability == otherBase._durability &&
                this._condition == otherBase._condition &&
                this._description == otherBase._description &&
                this._iconPath == otherBase._iconPath &&
                this.SameItemTypes(otherBase._itemType);
        }

        private bool SameItemTypes(List<ItemType> other)
        {
            if (this._itemType == null && other == null) return true;
            if (this._itemType == null || other == null) return false;
            if (this._itemType.Count != other.Count) return false;
            
            for (int i = 0; i < this._itemType.Count; i++)
            {
                if (this._itemType[i] != other[i]) return false;
            }
            return true;
        }
    }
}
