using System;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Que componentes hacen que algo "este en el mundo", escrito una sola vez.
    ///
    /// <para>Estar en el mundo es tener sitio (<see cref="PositionComponent"/>) y forma a la
    /// que apuntar (<see cref="InteractionVolumeComponent"/>). El cuerpo en el motor es
    /// aparte: lo pone <c>IEntityLinker.Link</c> y lo quita <c>Unlink</c>.</para>
    ///
    /// <para>Poner y quitar leen la misma lista (<see cref="COMPONENTS"/>). Si fueran dos
    /// listas, el dia que se anadiera un componente de mundo a una y no a la otra, un item
    /// recogido volveria al inventario con restos del suelo, dejaria de ser equivalente a
    /// sus iguales y no se apilaria con ellos.</para>
    /// </summary>
    public static class WorldPresence
    {
        /// <summary>
        /// Radio de la esfera con la que nace lo que se pone en el mundo, para poder apuntarle
        /// desde el primer fotograma. El linker la cambia por la caja medida del modelo en
        /// cuanto carga.
        /// </summary>
        public const float DEFAULT_VOLUME_RADIUS = 0.2f;

        private static readonly Type[] COMPONENTS =
        {
            typeof(PositionComponent),
            typeof(InteractionVolumeComponent)
        };

        /// <summary>
        /// Le da a la entidad presencia en el mundo, en el origen. Colocarla es cosa de quien
        /// la pone ahi.
        /// </summary>
        public static void AddTo(IEntity entity)
        {
            entity.AddComponent(new PositionComponent(0f, 0f, 0f));
            entity.AddComponent(new InteractionVolumeComponent(new SphereVolume(DEFAULT_VOLUME_RADIUS)));
        }

        /// <summary>
        /// Copia de un item del mundo sin nada de su presencia en el: lo que entra en un
        /// inventario al recogerlo.
        ///
        /// <para>Se recoge una COPIA y no el propio item porque el item tiene que entrar ya
        /// limpio (la equivalencia que decide con que se apila compara todos los
        /// componentes) y porque, si no cabe, el del suelo no debe haberse tocado. El clon
        /// tampoco lleva el puente con el motor: <c>InGameEntity.Clone</c> no lo copia.</para>
        /// </summary>
        public static ItemEntity CleanCopyOf(ItemEntity item)
        {
            ItemEntity copy = item.Clone();
            foreach (Type type in COMPONENTS) copy.RemoveComponent(type);
            return copy;
        }

        /// <summary>Si la entidad esta en el mundo (tiene todos los componentes de presencia).</summary>
        public static bool IsIn(IEntity entity)
        {
            if (entity == null) return false;
            foreach (Type type in COMPONENTS)
                if (!entity.HasComponent(type)) return false;
            return true;
        }
    }
}
