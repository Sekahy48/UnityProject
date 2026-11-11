using System;
using UnityEngine;

namespace MVC.View
{
    /// <summary>
    /// Companion MonoBehaviour for the inventory UIDocument.
    /// UI Toolkit's Live Reload rebuilds the visual tree and then calls OnEnable
    /// on every MonoBehaviour of the UIDocument's GameObject. This forwards that
    /// signal so the views can be rebuilt and re-acquire their element references.
    /// Editor-only concern; harmless in a build (fires once at startup).
    /// </summary>
    public class UIReloadNotifier : MonoBehaviour
    {
        public static event Action OnUIRecreated;

        private void OnEnable() => OnUIRecreated?.Invoke();
    }
}