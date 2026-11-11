using System.ComponentModel;

namespace Core.ECS.Component.Equipment
{
    public enum EquipmentSlotType
    {
        [Description("Cabeza")]
        Head,

        [Description("Pecho")]
        Chest,

        [Description("Piernas")]
        Legs,

        [Description("Pies")]
        Feet,

        [Description("Mano derecha")]
        RightHand,

        [Description("Mano izquierda")]
        LeftHand,

        [Description("Espalda")]
        Back,

        [Description("Cadera")]
        Hip
    }
}