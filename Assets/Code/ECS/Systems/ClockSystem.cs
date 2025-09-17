using System;
using System.Collections.Generic;
using Observer; 
namespace ECS.Systems
{
    public class ClockSystem : ISubject
    {
        private static ClockSystem instance = null;
        private readonly List<IObserver> observers = new();
        private float timeSpeed = 1.0f; // Default time speed
        private float adjustedDeltaTime = 0.0f; // Adjusted delta time
        private float tickTime = 0.016f; // Default tick time (60 FPS)
        private bool isRunning = true; // Flag to check if the clock system is running

        private ClockSystem() { }

        public static ClockSystem GetInstance()
        {
            if (instance == null)
                instance = new ClockSystem();
            return instance;
        }

        public void NotifyObservers()
        {
            foreach (var observer in observers)
            {
                observer.Update();
            }
        }

        public void Attach(IObserver target)
        {
            observers.Add(target);
        }

        public void Detach(IObserver target)
        {
            observers.Remove(target);
        }

        public void SetTickTime(float tickTime)
        {
            this.tickTime = tickTime;
        }

        public float GetTickTime()
        {
            return tickTime;
        }

        public void SetTimeSpeed(float timeSpeed)
        {
            this.timeSpeed = timeSpeed;
            if (timeSpeed <= 0)
            {
                this.timeSpeed = 0; // Prevent negative time speed
                this.isRunning = false; // Pause the clock system
            }
            else
            {
                this.isRunning = true; // Resume the clock system
            }
        }

        public float GetTimeSpeed()
        {
            return timeSpeed;
        }

        public void Update(float deltaTime)
        {
            adjustedDeltaTime += deltaTime * timeSpeed;

            while (adjustedDeltaTime >= tickTime)
            {
                adjustedDeltaTime -= tickTime;
                Tick();
            }
        }

        public void Tick()
        {
            NotifyObservers();
        }

        public void Pause()
        {
            isRunning = false;
        }

        public void Resume()
        {
            isRunning = true;
        }

        public bool IsRunning()
        {
            return isRunning;
        }
    }
}
