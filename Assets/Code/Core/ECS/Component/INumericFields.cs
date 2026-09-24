namespace Core.ECS.Component
{
    /// <summary>
    /// Implementado por los componentes que pueden responder por sus campos numericos
    /// dandoles el nombre.
    ///
    /// Existe porque hay preguntas que llegan como texto y no como codigo: un item declara
    /// que su modelo depende de <c>"Material.hardness"</c>, y una receta pedira algun dia un
    /// lingote con <c>"Temperature.celsius"</c> por encima de tanto. En los dos casos el
    /// nombre del campo se decide en Stack&amp;Go y llega en el catalogo, asi que no hay
    /// forma de resolverlo con genericos.
    ///
    /// La alternativa era reflexion, y se descarto: ataria el nombre que el autor escribe en
    /// Stack&amp;Go al nombre del miembro en C#, de modo que un renombrado durante un
    /// refactor compilaria sin quejarse y rompería el dato en tiempo de ejecucion, lejos y
    /// en silencio. Escribiendolo a mano, cada componente es dueno del conocimiento de sus
    /// propios campos y un renombrado rompe donde se ve.
    ///
    /// Solo campos numericos: quien pregunta lo hace para comparar por orden, y un enum o un
    /// booleano no tienen "mayor o igual".
    /// </summary>
    public interface INumericFields
    {
        /// <summary>
        /// Los nombres son los mismos que usa <see cref="IJsonLoadable.SetFromValues"/> en
        /// este componente, porque los dos hablan con el mismo catalogo: alli se escriben al
        /// cargar y aqui se leen despues. Las dos listas tienen que coincidir, asi que se
        /// mantienen juntas en el mismo fichero.
        /// </summary>
        /// <param name="field">Nombre del campo tal como aparece en el catalogo</param>
        /// <param name="value">Valor leido, o 0 si el campo no existe</param>
        /// <returns>True si el componente conoce ese campo</returns>
        bool TryGetNumericValue(string field, out float value);
    }
}
