using System;
using Core.ECS.Entity;
using UnityEngine;

namespace Strategy
{
    public interface ICameraStrategy
    {
        void Execute(float deltaTime);
        void Activate();
        void Deactivate();
        Camera GetCamera(); 
  
    }
    
}