using System;
using Observer;

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
        GENERIC,
        WEAPON,
        ARMOR,
        CONSUMABLE,
        MATERIAL,
        TOOL,
        QUEST,
        MISC
    }
    
    public class BaseItemComponent : BasicComponent
    {

        /// <summary>
        /// Tipo de la entidad.
        /// </summary>
        private ItemType _itemType;
        
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
        private int _durability;

        /// <summary>
        /// Durabilidad máxima de la entidad. (puntos hasta romperse)
        /// </summary>
        private const int _maxDurability = 100;

        /// <summary>
        /// Condición de la entidad. (puntos hasta funcionar muy deficientemente)
        /// </summary>
        private int condition;

        /// <summary>
        /// Condición maxima de la entidad.
        /// </summary>
        private const int maxCondition = 100;

        /// <summary>
        /// Descripción del componente.
        /// </summary>
        private String _description;

        /// <summary>
        /// Ruta del icono del componente.
        /// </summary>
        private String _iconPath;

        public BaseItemComponent(float weight, float volume)
        {
            _weight = weight;
            _volume = volume;
        } 
        
        public override IComponent Clone()
        {
            return new BaseItemComponent(_weight, _volume);
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
        public int GetDurability()
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
        public int GetCondition()
        {
            return condition;
        }

        /// <summary>
        /// Establece la condición del componente.
        /// </summary>
        public void SetCondition(int condition)
        {
            this.condition = Math.Clamp(condition, 0, maxCondition);
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
        public ItemType GetItemType()
        {
            return _itemType;
        }

        /// <summary>
        /// Establece el tipo del componente.
        /// </summary>
        public void SetItemType(ItemType itemType)
        {
            _itemType = itemType;
        }
        
    }
}
