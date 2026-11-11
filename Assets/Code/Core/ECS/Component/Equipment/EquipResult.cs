namespace Core.ECS.Component.Equipment
{
    public enum EquipResult
    { 
        SuccessEquip, 
        SuccessUnequip,
        SlotDisabled, 
        MaxLayersReached, 
        WrongSlot, 
        NoSlotFits,
        DuplicateCategory, 
        NotWearable, 
        TopLayerBlocked,
        UnableToUnequip,
    }

    public static class EquipResultExtensions
    {
        public static string GetMessage(this EquipResult result) => result switch
        {
            EquipResult.SuccessEquip => "Item equipped successfully",
            EquipResult.SuccessUnequip => "Item unequipped successfully",
            EquipResult.SlotDisabled => "This slot is disabled",
            EquipResult.MaxLayersReached => "No more layers available",
            EquipResult.WrongSlot => "Item doesn't fit this slot",
            EquipResult.NoSlotFits => "No compatible slot found",
            EquipResult.DuplicateCategory => "Already wearing that garment type",
            EquipResult.NotWearable => "This item cannot be equipped",
            EquipResult.TopLayerBlocked => "Top layer is blocking",
            _ => "Unknown result"
        };
    }
}