using System;
using System.Collections.Generic;
using System.Numerics;
using Observer;
using Utils;

namespace ECS.Component
{
    /// <summary>
    /// Componente básico que implementa IComponent y extiende GenericSubject.
    /// Contiene como parte de su funcionalidad las cosas basicas que todo ItemEntity 
    /// debe tener, así como un nombre referente al tipo de componente (se hereda´
    /// para quien implemente esta clase). Puede componerse dentro de otros tipos
    /// de entidad pero no se espera que esto se aproveche.
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
        /// Tipo de la entidad.
        /// </summary>
        private List<ItemType> _itemType;
        
        /// <summary>
        /// Peso de la entidad.
        /// </summary>
        private float _weight;

        /// <summary>
        /// Volumen de la entidad.
        /// </summary>
        private float _volume;

        /// <summary>
        /// Durabilidad de la entidad.
        /// </summary>
        private float _durability;

        /// <summary>
        /// Durabilidad máxima de la entidad. (puntos hasta romperse)
        /// </summary>
        private const float _maxDurability = 100;

        /// <summary>
        /// Condición de la entidad. (puntos hasta funcionar muy deficientemente)
        /// </summary>
        private float _condition;

        /// <summary>
        /// Condición maxima de la entidad.
        /// </summary>
        private const float maxCondition = 100;

        /// <summary>
        /// Descripción del componente.
        /// </summary>
        private String _description;

        /// <summary>
        /// Ruta del icono del componente.
        /// </summary>
        private String _iconPath;

        /// <summary>
        /// Dimensiones del item cuando esta en inventario, cuanto ocupa visualmente,
        /// no en terminos de calculo de capacidad de carga restante del inventario 
        /// que lo alverga.
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
        /// Obtiene el peso del componente.
        /// </summary>
        /// <returns>el peso</returns>
        public float GetWeight()
        {
            return _weight;
        }

        /// <summary>
        /// Establece el peso del componente.
        /// </summary>
        public void SetWeight(float weight)
        {
            _weight = weight;
        }

        /// <summary>
        /// Obtiene el volumen del componente.
        /// </summary>
        /// <returns>el volumen</returns>
        public float GetVolume()
        {
            return _volume;
        }

        /// <summary>
        /// Establece el volumen del componente.
        /// </summary>
        public void SetVolume(float volume)
        {
            _volume = volume;
        }

        /// <summary>
        /// Obtiene la durabilidad del componente.
        /// </summary>
        /// <returns>la durabilidad</returns>
        public float GetDurability()
        {
            return _durability;
        }

        /// <summary>
        /// Establece la durabilidad del componente.
        /// </summary>
        public void SetDurability(int durability)
        {
            _durability = Math.Clamp(durability, 0, _maxDurability);
        }

        /// <summary>
        /// Obtiene la condición del componente.
        /// </summary>
        /// <returns>la condición</returns>
        public float GetCondition()
        {
            return _condition;
        }

        /// <summary>
        /// Establece la condición del componente.
        /// </summary>
        public void SetCondition(float condition)
        {
            this._condition = Math.Clamp(condition, 0, maxCondition);
        }

        /// <summary>
        /// Obtiene la descripción del componente.
        /// </summary>
        /// <returns>la descripción</returns>
        public String GetDescription()
        {
            return _description;
        }

        /// <summary>
        /// Establece la descripción del componente.
        /// </summary>
        public void SetDescription(String description)
        {
            _description = description;
        }

        /// <summary>
        /// Obtiene la ruta del icono del componente.
        /// </summary>
        /// <returns>la ruta del icono</returns>
        public String GetIconPath()
        {
            return _iconPath;
        }

        /// <summary>
        /// Establece la ruta del icono del componente.
        /// </summary>
        public void SetIconPath(String iconPath)
        {
            _iconPath = iconPath;
        }

        /// <summary>
        /// Obtiene el tipo del componente.
        /// </summary>
        public List<ItemType> GetItemType()
        {
            return _itemType;
        }

        /// <summary>
        /// Establece el tipo del componente.
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
