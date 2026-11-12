using System;
using UnityEngine;

namespace MVC.View
{
    /// <summary>
    /// MonoBehaviour acompanante del UIDocument del inventario.
    /// La recarga en vivo de UI Toolkit reconstruye el arbol visual y luego llama a OnEnable
    /// en cada MonoBehaviour del GameObject del UIDocument. Este reenvia esa senal para
    /// que las vistas se reconstruyan y vuelvan a buscar sus elementos.
    /// Solo importa en el Editor; en una build no hace dano (salta una vez al arrancar).
    /// </summary>
    public class UIReloadNotifier : MonoBehaviour
    {
        public static event Action OnUIRecreated;

        private void OnEnable() => OnUIRecreated?.Invoke();
    }
}