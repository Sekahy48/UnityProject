namespace Core.MVC.View.UI.Inventory
{
    /// <summary>
    /// Lado de una celda de rejilla, en pixeles de UI. Dato de presentacion: Core no lo
    /// interpreta, solo lo transporta desde el panel que lo mide hasta la vista que pinta con
    /// el. Existe para que los presenters no necesiten el Vector2 de Unity, que ataria Core al
    /// motor por un dato que ni siquiera opera.
    ///
    /// <para>Struct y no clase: es un valor, dos floats sin identidad. Dos celdas de 60x60 son
    /// la misma cosa, no dos objetos distintos que casualmente miden igual. Al ser readonly
    /// tampoco puede mutarse a espaldas de quien lo emitio, y viaja por la pila sin generar
    /// basura en un camino que se recorre en cada movimiento del raton.</para>
    /// </summary>
    public readonly struct CellSize
    {
        public readonly float Width;
        public readonly float Height;

        public CellSize(float width, float height)
        {
            Width = width;
            Height = height;
        }

        /// <summary>Sin layout resuelto todavia no hay celda que medir.</summary>
        public bool IsZero => Width <= 0f || Height <= 0f;
    }
}
