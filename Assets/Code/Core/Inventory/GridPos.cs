using System;

namespace Core.Inventory
{
    /// <summary>
    /// Celda de una rejilla: fila y columna. Existe para que el compilador distinga lo que dos
    /// enteros sueltos no distinguen — pasar (col, row) donde tocaba (row, col) compila
    /// perfectamente y falla en ejecucion, y ya ha ocurrido en este proyecto. Con un solo
    /// parametro la inversion solo puede colarse al construirlo, no en cada llamada.
    ///
    /// <para>Struct readonly: es un valor sin identidad. La celda (2,3) ES (2,3); dos celdas
    /// (2,3) no son dos objetos distintos que casualmente coinciden, y por eso la igualdad va
    /// por contenido.</para>
    /// </summary>
    public readonly struct GridPos : IEquatable<GridPos>
    {
        public readonly int Row;
        public readonly int Col;

        public GridPos(int row, int col)
        {
            Row = row;
            Col = col;
        }

        /// <summary>
        /// "Ninguna celda". Sustituye al centinela (-1, -1) que antes se repetia a mano en
        /// FindFirstFit y en PointToCoords.
        /// </summary>
        public static readonly GridPos None = new GridPos(-1, -1);

        /// <summary>Coordenada negativa: fuera de cualquier rejilla, sea cual sea su tamano.</summary>
        public bool IsNone => Row < 0 || Col < 0;

        public bool Equals(GridPos other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is GridPos other && Equals(other);
        public override int GetHashCode() => (Row * 397) ^ Col;

        public static bool operator ==(GridPos a, GridPos b) => a.Equals(b);
        public static bool operator !=(GridPos a, GridPos b) => !a.Equals(b);

        public override string ToString() => $"({Row}, {Col})";
    }
}
