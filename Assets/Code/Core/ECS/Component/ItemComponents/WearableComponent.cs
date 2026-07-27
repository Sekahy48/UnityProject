using ECS.Component.Equipment;

namespace ECS.Component.ItemComponents
{
    public class WearableComponent : BasicComponent
    {
        private readonly EquipmentSlotType _targetSlot;
        private readonly bool _topLayer;
        private readonly GarmentCategory _garmentCategory;

        public WearableComponent(EquipmentSlotType targetSlot, bool topLayer, GarmentCategory garmentCategory)
        {
            _targetSlot = targetSlot;
            _topLayer = topLayer;
            _garmentCategory = garmentCategory;
        }

        public EquipmentSlotType GetTargetSlot() => _targetSlot;
        public bool IsTopLayer() => _topLayer;
        public GarmentCategory GetGarmentCategory() => _garmentCategory;

        public override IComponent Clone() => new WearableComponent(_targetSlot, _topLayer, _garmentCategory);

        public override bool Equivalent(IComponent other)
        {
            bool equivalent = false;
            if (other is WearableComponent otherWearable)
            {
                equivalent = _targetSlot.Equals(otherWearable._targetSlot) &&
                             _topLayer.Equals(otherWearable._topLayer) &&
                             _garmentCategory.Equals(otherWearable._garmentCategory);
            }
            return equivalent;
        }
    } 
}