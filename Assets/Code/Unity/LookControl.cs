using System.Collections.Generic;
using UnityEngine;

namespace Unity
{
    /// <summary>Por que se ha bloqueado la vista. Un motivo por cosa que la necesita.</summary>
    public enum LookLockReason
    {
        /// <summary>El jugador lo pidio con la tecla (Alt).</summary>
        Manual,

        /// <summary>El inventario esta abierto: el raton es un cursor.</summary>
        Inventory,

        /// <summary>El menu radial esta abierto: el raton elige opcion.</summary>
        RadialMenu,

        /// <summary>El panel de inspeccion esta abierto: el raton puede cerrarlo con la X.</summary>
        Inspect
    }

    /// <summary>
    /// UNICO sitio que decide si la camara lee el raton y como esta el cursor.
    ///
    /// <para>Bloqueo por motivos, no por un booleano: la vista esta bloqueada mientras haya
    /// ALGUN motivo activo. Con un booleano, cerrar el radial desbloquearia una vista que
    /// habia bloqueado Alt, o al reves. Cada motivo lo pone y lo quita su dueno.</para>
    ///
    /// <para>El cursor se deriva de aqui y se toca SOLO cuando cambia lo que toca hacer:
    /// con camara de avatar y vista libre, bloqueado y oculto (se apunta con el centro de la
    /// pantalla); en cualquier otro caso, libre y visible. Al liberarlo tras estar
    /// bloqueado, Unity lo deja en el centro de la pantalla, que es justo el centro del menu
    /// radial.</para>
    /// </summary>
    public class LookControl
    {
        private readonly HashSet<LookLockReason> _reasons = new HashSet<LookLockReason>();
        private bool _avatarCamera;

        /// <summary>Estado del cursor que se aplico por ultima vez, para no reaplicarlo.</summary>
        private bool? _captured;

        /// <summary>Si la camara NO debe girar con el raton.</summary>
        public bool IsLocked => _reasons.Count > 0;

        public void Set(LookLockReason reason, bool locked)
        {
            bool changed = locked ? _reasons.Add(reason) : _reasons.Remove(reason);
            if (changed) Apply();
        }

        public void Toggle(LookLockReason reason) => Set(reason, !_reasons.Contains(reason));

        /// <summary>
        /// Si la camara activa es de avatar (FPS/TPS). En RTS el raton siempre es un cursor,
        /// asi que nunca se captura aunque no haya ningun motivo de bloqueo.
        /// </summary>
        public void SetAvatarCamera(bool avatar)
        {
            if (_avatarCamera == avatar && _captured.HasValue) return;
            _avatarCamera = avatar;
            Apply();
        }

        /// <summary>
        /// Vuelve a capturar el cursor si deberia estarlo y alguien lo solto por fuera. En el
        /// Editor, Esc suelta el cursor siempre (lo hace Unity, no nosotros), y Apply solo toca
        /// el cursor cuando cambia el motivo: sin esto se quedaba visible hasta el siguiente
        /// cambio. Se llama cada fotograma; solo escribe si hay diferencia.
        /// </summary>
        public void Reassert()
        {
            if (_captured != true || Cursor.lockState == CursorLockMode.Locked) return;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Apply()
        {
            bool capture = _avatarCamera && !IsLocked;
            if (_captured == capture) return;

            _captured = capture;
            Cursor.lockState = capture ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !capture;
        }
    }
}
