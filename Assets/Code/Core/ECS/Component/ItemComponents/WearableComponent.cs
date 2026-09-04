using System;
using System.Collections.Generic;
using System.Linq;
using Core.ECS.Component.Equipment;

namespace Core.ECS.Component.ItemComponents
{
    public class WearableComponent : BasicComponent, IJsonLoadable
    {
        private IReadOnlyList<EquipmentSlotType> _targetSlots;
        private bool _topLayer;
        public bool FullOcupancy {get; private set;}
        private GarmentCategory _garmentCategory;
 
        public WearableComponent() { }

        public WearableComponent(IReadOnlyList<EquipmentSlotType> targetSlots, bool topLayer, GarmentCategory garmentCategory, bool fullOcupancy)
        {
            _targetSlots = targetSlots;
            _topLayer = topLayer;
            _garmentCategory = garmentCategory; 
            FullOcupancy = fullOcupancy;
        }

        public IReadOnlyList<EquipmentSlotType> TargetSlots => _targetSlots;
        public bool IsTopLayer => _topLayer;
        public GarmentCategory GarmentCategory => _garmentCategory;

        public void SetTargetSlots(IReadOnlyList<EquipmentSlotType> targetSlots) => _targetSlots = targetSlots;
        public void SetTopLayer(bool topLayer) => _topLayer = topLayer;
        public void SetGarmentCategory(GarmentCategory garmentCategory) => _garmentCategory = garmentCategory;
        public void SetFullOcupancy(bool fullOcupancy) => FullOcupancy = fullOcupancy;

        public void SetFromValues(Dictionary<string, object> values)
        {
            if (values.ContainsKey("targetSlots"))
            {
                var targetSlots = values["targetSlots"]
                    .ToString()
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(slot => Enum.Parse<EquipmentSlotType>(slot.Trim(), true))
                    .ToList();

                SetTargetSlots(targetSlots);
            }

            if (values.ContainsKey("garmentCategory"))
                SetGarmentCategory(
                    Enum.Parse<GarmentCategory>(
                        values["garmentCategory"].ToString(), true
                    )
                );

            if (values.ContainsKey("topLayer"))
                SetTopLayer(Convert.ToBoolean(values["topLayer"]));

            if (values.ContainsKey("fullOcupancy"))
                SetFullOcupancy(Convert.ToBoolean(values["fullOcupancy"]));
        }

        public override IComponent Clone() => new WearableComponent(_targetSlots, _topLayer, _garmentCategory, FullOcupancy);

        public override bool Equivalent(IComponent other)
        {
            bool equivalent = false;

            if (other is WearableComponent otherWearable)
            {
                equivalent = _targetSlots.OrderBy(x => x).SequenceEqual(otherWearable._targetSlots.OrderBy(x => x)) &&
                            _topLayer.Equals(otherWearable._topLayer) &&
                            _garmentCategory.Equals(otherWearable._garmentCategory);
            }

            return equivalent;
        }
    } 
}