using System.Collections.Generic;

namespace Core.ECS.Component.ItemComponents
{
    /// <summary>
    /// Un tramo de la magnitud que gobierna el aspecto de un item, y las variantes que le
    /// corresponden.
    ///
    /// La etapa aplica cuando el valor de la magnitud es mayor o igual que <see cref="Threshold"/>,
    /// asi que una etapa de umbral 0 es el caso por defecto. Las variantes de una misma etapa
    /// son intercambiables: representan lo mismo con otro aspecto, y quien dibuja elige una.
    ///
    /// Es inmutable a proposito. Los componentes se clonan por entidad, y una etapa que
    /// nadie puede modificar puede compartirse entre todos los clones sin copiarla: el
    /// prototipo y sus mil manzanas apuntan a la misma lista de rutas.
    /// </summary>
    public class ModelStage
    {
        private readonly List<string> _paths;

        public ModelStage(float threshold, List<string> paths)
        {
            Threshold = threshold;
            _paths = paths ?? new List<string>();
        }

        public float Threshold {get;}

        public IReadOnlyList<string> Paths => _paths;

        public int VariantCount => _paths.Count;

        /// <summary>
        /// Variante en la posicion indicada, dando la vuelta al llegar al final.
        ///
        /// El indice se envuelve en vez de recortarse para que quien llama pueda pasar
        /// cualquier numero estable derivado de la entidad —su id, por ejemplo— sin saber
        /// cuantas variantes hay. Lo que importa es que la misma entidad reciba siempre la
        /// misma variante: si se eligiera al azar en cada consulta, un arcon parpadearia
        /// entre dos aspectos por el simple hecho de mirarlo dos veces.
        /// </summary>
        /// <param name="index">Cualquier entero, tambien negativo</param>
        /// <returns>Ruta de la variante, o null si la etapa no tiene ninguna</returns>
        public string VariantAt(int index)
        {
            if (_paths.Count == 0) return null;

            int wrapped = index % _paths.Count;
            if (wrapped < 0) wrapped += _paths.Count;

            return _paths[wrapped];
        }

        public bool Equivalent(ModelStage other)
        {
            if (other == null || Threshold != other.Threshold || _paths.Count != other._paths.Count)
                return false;

            for (int i = 0; i < _paths.Count; i++)
            {
                if (_paths[i] != other._paths[i]) return false;
            }

            return true;
        }
    }
}
