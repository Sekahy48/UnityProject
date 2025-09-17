using System.Collections.Generic;
using Strategy;
using UnityEngine;

namespace MVC.Controller
{
    public class CameraRegister

    {
        public enum CameraType { RTS, FPS }
        private Dictionary<CameraType, ICameraStrategy> Cams = new();
        private CameraType activeCam;
        
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
                camera.activate();
                foreach (var cam in Cams.Values)
                {
                    if (!cam.Equals(camera))
                    {
                        cam.deactivate();
                    }
                }
            }
            else
            {
                Debug.LogError($"Camera with name {name} not found.");
            }

            activeCam = name;
        }

    }
}