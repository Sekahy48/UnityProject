using Core.ECS.Entity;

namespace Core
{
    /// <summary>
    /// Condicion de alcance que Core no puede comprobar por si mismo.
    ///
    /// <para>La busqueda de objetivos decide con lo que sabe: distancia al volumen y
    /// direccion de la mirada. Lo que no sabe es si hay una pared en medio, porque eso vive
    /// en la geometria de la escena y es cosa del motor. En vez de subir la decision entera
    /// a la capa de Unity, esta se queda en Core y el motor aporta un filtro.</para>
    ///
    /// <para>Es una interfaz con nombre y no un delegado suelto a proposito. Un
    /// <c>Func&lt;IEntity, IEntity, bool&gt;</c> en una firma no dice que comprueba, y quien
    /// lea el codigo dentro de unos meses tendria que ir a buscar quien lo pasa para
    /// averiguarlo. <c>IReachFilter</c>, y una implementacion llamada
    /// <c>LineOfSightFilter</c>, se explican solos.</para>
    ///
    /// <para><b>Se aplica el ultimo.</b> Es la comprobacion cara —un rayo contra la escena—,
    /// asi que corre sobre los pocos candidatos que han sobrevivido a distancia y cono, no
    /// sobre todas las entidades del mundo. Reordenarlo no cambiaria el resultado, solo el
    /// coste, que es la clase de regresion que no avisa.</para>
    /// </summary>
    public interface IReachFilter
    {
        /// <param name="actor">Quien quiere alcanzar</param>
        /// <param name="target">Candidato que ya ha pasado las comprobaciones de Core</param>
        bool CanReach(IEntity actor, IEntity target);
    }
}
