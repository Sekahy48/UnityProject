using Core.ECS.Entity;
using Core.Handler;
using NUnit.Framework;

namespace Core.Tests
{
    /// <summary>
    /// I. Identidad de las entidades. Existe porque la limpieza de estandares del 2026-11-12
    /// renombro el parametro de EntityId a _id y el constructor quedo en "_id = _id": todas
    /// las entidades tenian id 0, y el registro de entidades, indexado por id, se quedaba solo
    /// con la ultima.
    /// </summary>
    public class EntityIdTests
    {
        [Test]
        public void Un_EntityId_guarda_el_numero_que_recibe()
        {
            Assert.AreEqual(42, new EntityId(42).ToInt());
        }

        [Test]
        public void Dos_clones_de_un_prototipo_tienen_ids_distintos()
        {
            InGameEntity prototype = new InGameEntity(IdGenerator.GenerateNewId());

            Assert.AreNotEqual(prototype.Clone().GetIdAsInt(), prototype.Clone().GetIdAsInt());
        }
    }
}
