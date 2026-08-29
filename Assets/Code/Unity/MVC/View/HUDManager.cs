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
            EnergyComponent fisiologic = player.GetComponent<EnergyComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a EnergyComponent.");
                return;
            }
            // Logic to reduce the stamina bar in the HUD interface
            //UnityEngine.Debug.Log("Stamina: " + fisiologic.Stamina + "/" + fisiologic.MaxStamina);
            HUDUtils.GetInstance().ModifyFillable("StaminaBar", fisiologic.Stamina / fisiologic.MaxStamina);
        }

        public void UpdateFatigue()
        {
            EnergyComponent fisiologic = player.GetComponent<EnergyComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a EnergyComponent.");
                return;
            }
            // Logic to reduce the fatigue bar in the HUD interface
            // UnityEngine.Debug.Log("Fatigue: " + fisiologic.Fatigue + "/" + fisiologic.MaxFatigue);
            HUDUtils.GetInstance().ModifyFillable("FatigueBar", fisiologic.Fatigue / fisiologic.MaxFatigue);
        }
    }
}