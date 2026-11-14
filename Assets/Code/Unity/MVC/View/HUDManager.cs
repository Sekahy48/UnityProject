using System;
using UnityEngine;
using System.Diagnostics;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.Observer;

namespace MVC.View
{
    public class HUDManager : IObserver
    {
        private IEntity _player; 
        public HUDManager(IEntity player)
        {
            _player = player;
        }
        public void Update()
        {
            UpdateStamina();
            UpdateFatigue();
        }
        public void UpdateStamina()
        {
            EnergyComponent fisiologic = _player.GetComponent<EnergyComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a EnergyComponent.");
                return;
            }
            // Aqui iria la logica para bajar la barra de aguante del HUD
            //UnityEngine.Debug.Log("Stamina: " + fisiologic.Stamina + "/" + fisiologic.MaxStamina);
            HUDUtils.GetInstance().ModifyFillable("StaminaBar", fisiologic.Stamina / fisiologic.MaxStamina);
        }

        public void UpdateFatigue()
        {
            EnergyComponent fisiologic = _player.GetComponent<EnergyComponent>();
            if (fisiologic == null)
            {
                UnityEngine.Debug.LogError("Player entity does not have a EnergyComponent.");
                return;
            }
            // Aqui iria la logica para bajar la barra de cansancio del HUD
            // UnityEngine.Debug.Log("Fatigue: " + fisiologic.Fatigue + "/" + fisiologic.MaxFatigue);
            HUDUtils.GetInstance().ModifyFillable("FatigueBar", fisiologic.Fatigue / fisiologic.MaxFatigue);
        }
    }
}