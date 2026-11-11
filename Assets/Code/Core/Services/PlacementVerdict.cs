namespace Core.Services
{
    /// <summary>
    /// Resultado de preguntar "si suelto aqui, que pasa". Lo produce InventoryService por el
    /// mismo camino que la colocacion real, para que el color no pueda mentir.
    /// </summary>
    public enum PlacementVerdict
    {
        Fits,      // cabe entero: apila o encaja
        Partial,   // cabe parte: el resto se queda en la mano
        Swap,      // no cabe, pero los dos nodos pueden cambiarse las celdas
        Blocked,   // celda valida pero no admite nada: colision, peso o pila llena
        Outside    // fuera de la rejilla; a futuro sera "tirar al suelo"
    }
}
