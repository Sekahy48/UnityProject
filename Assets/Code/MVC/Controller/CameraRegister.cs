using System;
using System.Collections.Generic;
using ECS.Entity;
using Strategy;
using UnityEngine;

namespace MVC.Controller
{
    public class CameraRegister

    {
        public enum CameraType { RTS, FPS, TPS}
        private Dictionary<CameraType, ICameraStrategy> Cams = new();
        private Array CameraCarousel = Enum.GetValues(typeof(CameraType));
        private CameraType activeCam;

        public CameraRegister()
        {
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().enabled = false;
        }

        public void AddCamera(CameraType name, ICameraStrategy camera)
        {
            if (!Cams.ContainsKey(name))
            {
                Cams[name] = camera;
            }
            else
            {
                Debug.LogWarning($"Camera with name {name} already exists.");
            }
        }

        public void InitizalizeCameras(IEntity player)
        {
            AddCamera(CameraType.RTS, new RTSCameraStrategy());
            AddCamera(CameraType.FPS, new FirstPersonCamera(player)); 
            AddCamera(CameraType.TPS, new ThirdPersonCamera(player)); 
        }
        
        public ICameraStrategy GetCamera(CameraType name)
        {
            if (Cams.TryGetValue(name, out ICameraStrategy camera))
            {
                return camera;
            }
            else
            {
                Debug.LogError($"Camera with name {name} not found.");
                return null;
            }
        }

        public ICameraStrategy GetActiveCamera()
        {
            if (activeCam.Equals(null))
            {
                Debug.LogError("No active camera set. Taking the first one available.");
                return null;
            }

            return GetCamera(activeCam);
        }

        public bool RemoveCamera(CameraType name)
        {
            return Cams.Remove(name);
        }

        public void ActivateCamera(CameraType name)
        {
            if (Cams.TryGetValue(name, out ICameraStrategy camera))
            {
                camera.Activate();
                foreach (var cam in Cams.Values)
                {
                    if (!cam.Equals(camera))
                    {
                        cam.Deactivate();
                    }
                }
            }
            else
            {
                Debug.LogError($"Camera with name {name} not found.");
            }

            activeCam = name;
        }

        public ICameraStrategy NextCamera()
        {
            int i = Array.IndexOf(CameraCarousel, activeCam);
            i = (i + 1) % CameraCarousel.Length;
            ActivateCamera((CameraType)CameraCarousel.GetValue(i));
            return GetActiveCamera();
            
        }

    }
}