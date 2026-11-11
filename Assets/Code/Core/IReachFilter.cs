using Core.ECS.Entity;

namespace Core
{
    /// <summary>
    /// Condicion de alcance que Core no puede comprobar por si mismo: si hay algo entre los
    /// ojos del actor y el objetivo.
    ///
    /// <para>La busqueda de objetivos decide con lo que sabe: distancia al volumen y
    /// direccion de la mirada. Lo que no sabe es si hay una pared en medio, porque eso vive
    /// en la geometria de la escena y es cosa del motor. En vez de subir la decision entera
    /// a la capa de Unity, esta se queda en Core y el motor aporta un filtro.</para>
    ///
    /// <para><b>Core pasa los puntos.</b> Donde estan los ojos y a que punto del objetivo se
    /// mira lo decide Core (<c>WorldInteractionSystem</c>); el filtro solo contesta si la
    /// linea entre ellos esta libre. Si el filtro recalculara los ojos habria dos alturas de
    /// ojos, y el dia que una cambiase discreparian. Las entidades viajan solo para que el
    /// filtro pueda no tropezar con el cuerpo del actor ni con el propio objetivo.</para>
    ///
    /// <para>Es una interfaz con nombre y no un delegado suelto a proposito. Un
    /// <c>Func</c> en una firma no dice que comprueba, y quien lea el codigo dentro de unos
    /// meses tendria que ir a buscar quien lo pasa para averiguarlo.</para>
    ///
    /// <para><b>Se aplica el ultimo.</b> Es la comprobacion cara —un rayo contra la escena—,
    /// asi que corre sobre los pocos candidatos que han sobrevivido a distancia y cono, no
    /// sobre todas las entidades del mundo. Reordenarlo no cambiaria el resultado, solo el
    /// coste, que es la clase de regresion que no avisa.</para>
    /// </summary>
    public interface IReachFilter
    {
        /// <summary>Si la linea entre los dos puntos esta libre de obstaculos.</summary>
        /// <param name="actor">Quien mira: su propio cuerpo no cuenta como obstaculo.</param>
        /// <param name="target">A que mira: el propio objetivo tampoco.</param>
        bool IsClear(IEntity actor, IEntity target,
                     float fromX, float fromY, float fromZ,
                     float toX, float toY, float toZ);
    }
}
