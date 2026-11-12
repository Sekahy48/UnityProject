using System;
using System.Collections.Generic;
using Core.ECS.Entity;
using Strategy;
using UnityEngine;

namespace MVC.Controller
{
    public class CameraRegister : Unity.IActiveCameraSource
    {
        public enum CameraType { RTS, FPS, TPS}
        private Dictionary<CameraType, ICameraStrategy> _cams = new();
        private Array _cameraCarousel = Enum.GetValues(typeof(CameraType));
        private CameraType _activeCam;

        public CameraRegister()
        {
            GameObject.FindWithTag("MainCamera").GetComponent<Camera>().enabled = false;
        }

        public void AddCamera(CameraType name, ICameraStrategy camera)
        {
            if (!_cams.ContainsKey(name))
            {
                _cams[name] = camera;
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
            if (_cams.TryGetValue(name, out ICameraStrategy camera))
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
            if (_activeCam.Equals(null))
            {
                Debug.LogError("No active camera set. Taking the first one available.");
                return null;
            }

            return GetCamera(_activeCam);
        }

        public Camera Current => GetActiveCamera()?.GetCamera();

        public bool RemoveCamera(CameraType name)
        {
            return _cams.Remove(name);
        }

        public void ActivateCamera(CameraType name)
        {
            if (_cams.TryGetValue(name, out ICameraStrategy camera))
            {
                camera.Activate();
                foreach (var cam in _cams.Values)
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

            _activeCam = name;
        }

        public ICameraStrategy NextCamera()
        {
            int i = Array.IndexOf(_cameraCarousel, _activeCam);
            i = (i + 1) % _cameraCarousel.Length;
            ActivateCamera((CameraType)_cameraCarousel.GetValue(i));
            return GetActiveCamera();
            
        }

    }
}