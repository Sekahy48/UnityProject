using System.Collections.Generic;
using Core.ECS.Entity; 
using Core.Inventory; 


namespace Core.Events
{
    public class ItemLotEvent : GameEvent
    {
        public IReadOnlyList<SubLot> Lots { get; }
        
        public ItemLotEvent(GameEventType type, IEntity origin, IReadOnlyList<SubLot> lots): base(type, origin, null)
        { 
            Lots = lots; 
        } 
    }
}