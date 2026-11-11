using System;
using Core.ECS.Component;
using Core.ECS.Entity;

namespace Core.Item
{
    /// <summary>
    /// Lee de una entidad el valor de una magnitud nombrada en el catalogo, como
    /// <c>"Material.hardness"</c>.
    ///
    /// Vive en el dominio del item y no en la capa que dibuja porque la pregunta no es
    /// visual. Hoy la hace el resolutor de modelos —que etapa toca—, pero la misma pregunta
    /// es la que hara el crafting por estado: una receta que exija hierro por encima de
    /// cierta temperatura necesita exactamente esto. Si naciera dentro del resolutor visual,
    /// el dia del crafting habria que sacarla de alli o duplicarla.
    /// </summary>
    public static class ItemMagnitudes
    {
        public const char SEPARATOR = '.';

        /// <summary>
        /// Resuelve un nombre cualificado sobre una entidad concreta.
        ///
        /// Devuelve false sin ruido en todos los casos en que la magnitud no se puede leer:
        /// nombre mal formado, componente desconocido, la entidad no lo lleva, o el
        /// componente no responde por ese campo. Quien pregunta ya tiene que tener un plan
        /// para el "no se sabe" —el resolutor de modelos cae a la etapa por defecto—, y
        /// convertirlo en excepcion obligaria a envolver cada consulta.
        /// </summary>
        /// <param name="entity">Entidad sobre la que leer</param>
        /// <param name="qualifiedName">Nombre en la forma <c>Componente.campo</c></param>
        /// <param name="value">Valor leido, o 0 si no se pudo</param>
        /// <returns>True si la magnitud existe en esta entidad</returns>
        public static bool TryRead(IEntity entity, string qualifiedName, out float value)
        {
            value = 0f;

            if (entity == null || string.IsNullOrWhiteSpace(qualifiedName)) return false;

            int separator = qualifiedName.IndexOf(SEPARATOR);
            if (separator <= 0 || separator == qualifiedName.Length - 1) return false;

            string componentName = qualifiedName.Substring(0, separator);
            string fieldName = qualifiedName.Substring(separator + 1);

            Type componentType = ItemComponentRegistry.TypeOf(componentName);
            if (componentType == null) return false;

            IComponent component = entity.GetComponentByType(componentType);
            if (!(component is INumericFields fields)) return false;

            return fields.TryGetNumericValue(fieldName, out value);
        }
    }
}
