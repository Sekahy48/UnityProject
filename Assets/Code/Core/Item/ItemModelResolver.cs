using Core.ECS.Component;
using Core.ECS.Component.ItemComponents;
using Core.ECS.Entity;

namespace Core.Item
{
    /// <summary>
    /// Decide que modelo 3D representa a una entidad: de <see cref="ModelComponent"/> a una
    /// ruta concreta.
    ///
    /// Vive en Core porque no tiene una linea de motor: son componentes, un float y una
    /// cadena. Asi se puede probar sin abrir el editor, y la capa de Unity se queda con lo
    /// unico que solo ella puede hacer —pedir el fichero y colgarlo de un transform— en vez
    /// de mezclar las dos cosas.
    /// </summary>
    public static class ItemModelResolver
    {
        /// <summary>
        /// Ruta del modelo que le toca a esta entidad ahora mismo, o null si no tiene
        /// ninguno.
        ///
        /// El resultado puede cambiar con el tiempo: la magnitud que gobierna las etapas es
        /// estado de la entidad, asi que una herramienta que se desgasta devuelve una ruta
        /// distinta segun cuando se pregunte. Quien dibuja tiene que volver a preguntar
        /// cuando ese estado cambie, no quedarse con la primera respuesta.
        /// </summary>
        public static string ResolvePath(IEntity entity)
        {
            if (entity == null) return null;

            IEntity representative = RepresentativeOf(entity);
            if (representative == null) return null;

            ModelComponent model = representative.GetComponent<ModelComponent>();
            if (model == null || !model.HasStages) return null;

            ModelStage stage = StageOf(representative, model);

            // El indice decide que variante de la etapa toca. Se usa el id de la entidad
            // porque es estable: la misma entidad recibe siempre la misma variante, mientras
            // que elegir al azar en cada consulta haria parpadear un arcon entre dos aspectos
            // por el solo hecho de mirarlo dos veces.
            return stage?.VariantAt(representative.GetIdAsInt());
        }

        /// <summary>
        /// Entidad de la que sale el aspecto.
        ///
        /// Normalmente es la propia entidad, pero un monton en el suelo no tiene modelo
        /// suyo: lo que se ve es uno de los items que contiene. La regla vive aqui y no en
        /// la capa que dibuja para que esta no tenga que distinguir casos.
        /// </summary>
        private static IEntity RepresentativeOf(IEntity entity)
        {
            GroundLotComponent lot = entity.GetComponent<GroundLotComponent>();
            return lot == null ? entity : lot.Representative;
        }

        /// <summary>
        /// Etapa que corresponde al estado actual.
        ///
        /// Si el componente no declara magnitud, o esa magnitud no se puede leer en esta
        /// entidad, cae a la etapa por defecto en vez de quedarse sin modelo. Un item mal
        /// configurado debe verse feo, no invisible: un hueco en el mundo es mucho mas
        /// dificil de rastrear que un modelo que no cambia nunca.
        /// </summary>
        private static ModelStage StageOf(IEntity entity, ModelComponent model)
        {
            if (string.IsNullOrEmpty(model.DrivenBy)) return model.DefaultStage;

            return ItemMagnitudes.TryRead(entity, model.DrivenBy, out float value)
                ? model.StageFor(value)
                : model.DefaultStage;
        }
    }
}
