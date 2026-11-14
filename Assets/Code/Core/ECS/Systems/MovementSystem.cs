using System.Collections.Generic;
using Core.ECS.Component;
using Core.Events;
using Core.Observer;
using AC = Core.Utils.ArgumentChecker;

namespace Core.ECS.Systems
{
    public class MovementSystem : IReactiveSystem
    {
        private const float EXTRA_WEIGHT_SPEED = 0.80f;
        private const float OVERWEIGHT_SPEED = 0.50f;
        private const float IMMOBILE_SPEED = 0.0f;
        private const float NORMAL_SPEED = 1.0f;

        private bool _weightRestrictionActive = false;

        public IEnumerable<GameEventType> SubscribedEvents { get; } = new[]
        {
            GameEventType.ExtraWeight,
            GameEventType.Overweight,
            GameEventType.Immobile,
            GameEventType.NormalWeight
        };


        public void UpdateOnEvent(GameEvent gameEvent)
        {
            AC.CheckNotNull(gameEvent, nameof(gameEvent));

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