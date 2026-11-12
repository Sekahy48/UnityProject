using System;
using System.Collections.Generic;
using Core.Observer; 
namespace Core.ECS.Systems
{
    public class ClockSystem : ISubject
    {
        private static ClockSystem _instance = null;
        private readonly List<IObserver> _observers = new();
        private float _timeSpeed = 1.0f; // Velocidad del tiempo por defecto
        private float _adjustedDeltaTime = 0.0f; // Delta de tiempo ajustado
        private float _tickTime = 0.016f; // Duracion del tick por defecto (60 FPS)
        private bool _isRunning = true; // Si el reloj esta en marcha

        private ClockSystem() { }

        public static ClockSystem GetInstance()
        {
            if (_instance == null)
                _instance = new ClockSystem();
            return _instance;
        }

        public void NotifyObservers()
        {
            foreach (var observer in _observers)
            {
                observer.Update();
            }
        }

        public void Attach(IObserver target)
        {
            _observers.Add(target);
        }

        public void Detach(IObserver target)
        {
            _observers.Remove(target);
        }

        public void SetTickTime(float tickTime)
        {
            _tickTime = tickTime;
        }

        public float GetTickTime()
        {
            return _tickTime;
        }

        public void SetTimeSpeed(float timeSpeed)
        {
            _timeSpeed = timeSpeed;
            if (_timeSpeed <= 0)
            {
                _timeSpeed = 0; // Evita una velocidad de tiempo negativa
                _isRunning = false; // Pausa el reloj
            }
            else
            {
                _isRunning = true; // Reanuda el reloj
            }
        }

        public float GetTimeSpeed()
        {
            return _timeSpeed;
        }

        public void Update(float deltaTime)
        {
            if (!_isRunning) return;

            _adjustedDeltaTime += deltaTime * _timeSpeed;

            while (_adjustedDeltaTime >= _tickTime)
            {
                _adjustedDeltaTime -= _tickTime;
                Tick();
            }
        }

        public void Tick()
        {
            NotifyObservers();
        }

        public void Pause()
        {
            _isRunning = false;
        }

        public void Resume()
        {
            _isRunning = true;
        }

        public bool IsRunning()
        {
            return _isRunning;
        }
    }
}
