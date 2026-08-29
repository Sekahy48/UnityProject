namespace Services
{
    /// <summary>
    /// Resultado de preguntar "si suelto aqui, que pasa". Lo produce InventoryService por el
    /// mismo camino que la colocacion real, para que el color no pueda mentir.
    /// </summary>
    public enum PlacementVerdict
    {
        Fits,      // cabe, apila o encaja
        Blocked,   // celda valida pero no admite: colision, peso o pila llena
        Outside    // fuera de la rejilla; a futuro sera "tirar al suelo"
    }
}
