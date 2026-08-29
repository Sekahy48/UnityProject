using System.Collections.Generic;
using ECS.Component;
using Events;
using Observer;

namespace ECS.Systems
{
    public class MovementSystem : IReactiveSystem
    {
        private const float EXTRA_WEIGHT_SPEED = 0.80f;
        private const float OVERWEIGHT_SPEED = 0.50f;
        private const float IMMOBILE_SPEED = 0.0f;
        private const float NORMAL_SPEED = 1.0f;

        private bool _weightRestrictionActive = false;

        private static readonly GameEventType[] _subscribedEvents =
        {
            GameEventType.ExtraWeight,
            GameEventType.Overweight,
            GameEventType.Immobile,
            GameEventType.NormalWeight
        };

        public IEnumerable<GameEventType> SubscribedEvents => _subscribedEvents;


        public void UpdateOnEvent(GameEvent gameEvent)
        {
            float multiplier = -1;
            bool shouldRestrict = false;
            switch (gameEvent.GetEventType())
            {
                case GameEventType.ExtraWeight:
                    multiplier = EXTRA_WEIGHT_SPEED;
                    break;
                case GameEventType.Overweight:
                    multiplier = OVERWEIGHT_SPEED;
                    shouldRestrict = true;
                    break;
                case GameEventType.Immobile:
                    multiplier = IMMOBILE_SPEED;
                    shouldRestrict = true;
                    break;
                case GameEventType.NormalWeight:
                    multiplier = NORMAL_SPEED;
                    break;
            }

            if (multiplier >= 0)
            {
                MovementComponent movementComponent = gameEvent.GetComponent<MovementComponent>();
                movementComponent.SetWeightSpeedMultiplier(multiplier);

                if (shouldRestrict && !_weightRestrictionActive)
                {
                    movementComponent.AddRunRestriction();
                    _weightRestrictionActive = true;
                }
                else if (!shouldRestrict && _weightRestrictionActive)
                {
                    movementComponent.RemoveRunRestriction();
                    _weightRestrictionActive = false;
                }
            }
        }
    }
}