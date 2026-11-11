using System;

namespace Core.ECS.Component
{
    /// <summary>
    /// Hacia donde mira un actor, con la inclinacion de la cabeza incluida.
    ///
    /// <para>Existe porque la direccion del cuerpo no basta: <c>PositionComponent</c> solo
    /// gira en horizontal, y para apuntar a una manzana en el suelo hay que poder mirar
    /// hacia abajo. La mirada es otro hecho, con otro dueno: la escribe quien controla la
    /// cabeza —la camara en FPS y TPS, manana la IA de un NPC— y la leen la busqueda de
    /// objetivos y la comprobacion al ejecutar, que asi ven la misma.</para>
    ///
    /// <para>Guarda solo la direccion. El origen (los ojos) se deriva de la posicion y la
    /// estatura, y guardarlo aqui seria el mismo hecho dos veces.</para>
    /// </summary>
    public class GazeComponent : BasicComponent
    {
        private float _dx, _dy, _dz;
        private bool _hasDirection;

        public GazeComponent()
        {
            _name = "GazeComponent";
        }

        /// <summary>
        /// Si ya se ha escrito alguna mirada. Hasta entonces quien busca debe usar la
        /// direccion del cuerpo: una mirada a cero no apunta a ningun sitio.
        /// </summary>
        public bool HasDirection => _hasDirection;

        /// <summary>Direccion unitaria de la mirada.</summary>
        public (float x, float y, float z) Direction => (_dx, _dy, _dz);

        /// <summary>Se normaliza aqui para que nadie que la lea tenga que hacerlo.</summary>
        public void SetDirection(float x, float y, float z)
        {
            float length = (float)Math.Sqrt(x * x + y * y + z * z);
            if (length < 1e-6f) return;

            _dx = x / length;
            _dy = y / length;
            _dz = z / length;
            _hasDirection = true;
        }

        /// <summary>
        /// Un prototipo clonado no hereda la mirada de nadie: la mirada es del momento, no
        /// del tipo de entidad.
        /// </summary>
        public override IComponent Clone() => new GazeComponent();

        public override bool Equivalent(IComponent other) => other is GazeComponent;
    }
}
