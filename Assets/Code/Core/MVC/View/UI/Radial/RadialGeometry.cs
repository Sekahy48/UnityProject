using System;

namespace Core.MVC.View.UI.Radial
{
    /// <summary>
    /// Toda la geometria de un menu radial, en un solo sitio y sin motor.
    ///
    /// <para>Convenio unico para todo el proyecto: coordenadas de pantalla (x a la derecha,
    /// y hacia ABAJO), la opcion 0 arriba del todo y las demas en el sentido de las agujas
    /// del reloj. Quien coloca las opciones y quien decide cual esta bajo el raton usan
    /// estas mismas funciones, asi que lo que se ve y lo que se elige no pueden
    /// discrepar.</para>
    ///
    /// <para>Se elige por ANGULO, no por estar encima de la ficha: toda la porcion de tarta
    /// cuenta como zona de su opcion, y basta un gesto rapido en su direccion. El centro es
    /// zona muerta (no elige nada) y lo que queda muy lejos tampoco.</para>
    /// </summary>
    public static class RadialGeometry
    {
        private const float TWO_PI = (float)(Math.PI * 2.0);

        /// <summary>Angulo del centro de la opcion, en radianes desde "arriba", horario.</summary>
        public static float AngleOf(int index, int count)
        {
            if (count <= 0) return 0f;
            return TWO_PI * index / count;
        }

        /// <summary>
        /// Donde empieza y acaba la porcion (quesito) de la opcion, en radianes desde
        /// "arriba" y en sentido horario. Es la misma porcion que usa <see cref="IndexAt"/>
        /// para elegir: lo que se dibuja es exactamente lo que se elige.
        /// </summary>
        public static (float start, float end) SectorOf(int index, int count)
        {
            if (count <= 0) return (0f, 0f);
            float sector = TWO_PI / count;
            float center = AngleOf(index, count);
            return (center - sector * 0.5f, center + sector * 0.5f);
        }

        /// <summary>
        /// Donde va la opcion respecto al centro del menu, en pixeles de pantalla.
        /// </summary>
        public static (float x, float y) OffsetOf(int index, int count, float radius)
        {
            float angle = AngleOf(index, count);
            return ((float)Math.Sin(angle) * radius, -(float)Math.Cos(angle) * radius);
        }

        /// <summary>
        /// Que opcion hay en esa direccion, o -1 si ninguna.
        /// </summary>
        /// <param name="dx">Posicion del puntero respecto al centro, x a la derecha.</param>
        /// <param name="dy">Idem, y hacia abajo.</param>
        /// <param name="deadZone">Radio central que no elige nada.</param>
        /// <param name="maxDistance">Mas alla de esto tampoco se elige: es "fuera".</param>
        public static int IndexAt(float dx, float dy, int count, float deadZone, float maxDistance)
        {
            if (count <= 0) return -1;

            float distance = (float)Math.Sqrt(dx * dx + dy * dy);
            if (distance < deadZone || distance > maxDistance) return -1;

            // atan2(x, -y) da 0 arriba y crece en sentido horario con y hacia abajo.
            float angle = (float)Math.Atan2(dx, -dy);
            if (angle < 0f) angle += TWO_PI;

            // Cada opcion ocupa la porcion centrada en su angulo: se desplaza medio sector
            // para que la opcion 0 cubra de -medio a +medio sector alrededor de arriba.
            float sector = TWO_PI / count;
            int index = (int)((angle + sector * 0.5f) / sector);
            return index % count;
        }

        /// <summary>Si el punto esta fuera del menu (para "clicar fuera cierra").</summary>
        public static bool IsOutside(float dx, float dy, float maxDistance)
            => dx * dx + dy * dy > maxDistance * maxDistance;
    }
}
