using System;
using System.Collections.Generic;
using ECS.Component.Equipment;

namespace ECS.Component.ItemComponents
{
    public class WearableComponent : BasicComponent, IJsonLoadable
    {
        private EquipmentSlotType _targetSlot;
        private bool _topLayer;
        private GarmentCategory _garmentCategory;

        public WearableComponent() { }

        public WearableComponent(EquipmentSlotType targetSlot, bool topLayer, GarmentCategory garmentCategory)
        {
            _targetSlot = targetSlot;
            _topLayer = topLayer;
            _garmentCategory = garmentCategory;
        }

        public EquipmentSlotType GetTargetSlot() => _targetSlot;
        public bool IsTopLayer() => _topLayer;
        public GarmentCategory GetGarmentCategory() => _garmentCategory;

        public void SetTargetSlot(EquipmentSlotType targetSlot) => _targetSlot = targetSlot;
        public void SetTopLayer(bool topLayer) => _topLayer = topLayer;
        public void SetGarmentCategory(GarmentCategory garmentCategory) => _garmentCategory = garmentCategory;

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("targetSlot"))
                SetTargetSlot(Enum.Parse<EquipmentSlotType>(values["targetSlot"].ToString(), true));
            if (values.ContainsKey("garmentCategory"))
                SetGarmentCategory(Enum.Parse<GarmentCategory>(values["garmentCategory"].ToString(), true));
            if (values.ContainsKey("topLayer"))
                SetTopLayer(Convert.ToBoolean(values["topLayer"]));
        }

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