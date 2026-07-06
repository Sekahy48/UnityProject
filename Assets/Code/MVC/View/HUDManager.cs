using System;
using UnityEngine;
using System.Diagnostics;
using ECS.Component;
using ECS.Entity;
using Observer;

namespace MVC.View
{
    public class HUDManager : IObserver
    {
        private IEntity player; 
        public HUDManager(IEntity player)
        {
            this.player = player;
        }
        public void Update()
        {
            this.UpdateStamina();
            this.UpdateFatigue();
        }
        public void UpdateStamina()
        {
            FisiologicComponent fisiologic = player.GetComponent<FisiologicComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a FisiologicComponent.");
                return;
            }
            // Lógica para reducir la barra de stamina en la interfaz HUD
            //UnityEngine.Debug.Log("Stamina: " + fisiologic.GetStamina() + "/" + fisiologic.GetMaxStamina());
            HUDUtils.GetInstance().ModifyFillable("StaminaBar", fisiologic.GetStamina() / fisiologic.GetMaxStamina());
        }

        public void UpdateFatigue()
        {
            FisiologicComponent fisiologic = player.GetComponent<FisiologicComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a FisiologicComponent.");
                return;
            }
            // Lógica para reducir la barra de fatiga en la interfaz HUD
            // UnityEngine.Debug.Log("Fatigue: " + fisiologic.GetFatigue() + "/" + fisiologic.GetMaxFatigue());
            HUDUtils.GetInstance().ModifyFillable("FatigueBar", fisiologic.GetFatigue() / fisiologic.GetMaxFatigue());
        }
    }
}