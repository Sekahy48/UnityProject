namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Un punto en el espacio de coordenadas del panel de UI, en pixeles. Dato de
    /// presentacion: Core no lo interpreta, solo lo transporta desde la vista que lo mide
    /// hasta la vista que pinta con el. Hermano de <see cref="CellSize"/>, y por el mismo
    /// motivo — que los presenters no necesiten el Vector3 de Unity para llevar de la mano un
    /// dato sobre el que no operan.
    ///
    /// <para>Struct readonly: es un valor, dos floats sin identidad. Al viajar dentro del
    /// cierre de una opcion de menu es ademas una FOTO, no una referencia viva: dice donde
    /// estaba el ancla cuando se abrio el menu, no donde esta ahora.</para>
    /// </summary>
    public readonly struct PanelPoint
    {
        public readonly float X;
        public readonly float Y;

        public PanelPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        /// <summary>Sin layout resuelto todavia no hay nada que anclar.</summary>
        public bool IsZero => X == 0f && Y == 0f;
    }
}
