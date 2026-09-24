using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Component.Interaction;
using Core.ECS.Entity;
using Core.Events;
using AC = Core.Utils.ArgumentChecker;

namespace Core.ECS.Systems
{
    /// <summary>
    /// Todo lo que cruza la frontera entre un inventario y el mundo.
    ///
    /// Hasta ahora esa frontera no existia: <c>InventoryService.DropItems</c> sacaba los
    /// items del inventario, publicaba <see cref="GameEventType.ItemDropped"/> y ahi se
    /// acababa. Nadie escuchaba, asi que lo tirado desaparecia. Este sistema es quien
    /// escucha.
    ///
    /// <para>Esta pensado para acoger tambien lo que venga: colocar un objeto en un sitio
    /// concreto en vez de tirarlo, y la interaccion de vuelta —acercarse y recoger, abrir un
    /// arcon que esta en el suelo—. Todo eso comparte la misma pregunta: que existe en el
    /// mundo y como pasa de ahi a un inventario, o al reves. Repartirlo entre el servicio de
    /// inventario y la capa de Unity dejaria esa pregunta sin dueno.</para>
    ///
    /// <para>No conoce Unity. Crea entidades de Core y pide el enlace con el motor a traves
    /// de <see cref="IEntityLinker"/>, que es una interfaz de Core cuya implementacion vive
    /// al otro lado.</para>
    /// </summary>
    public class WorldInteractionSystem : IReactiveSystem
    {
        private static readonly GameEventType[] _subscribedEvents =
        {
            GameEventType.ItemDropped
        };

        public IEnumerable<GameEventType> SubscribedEvents => _subscribedEvents;

        private readonly EntityManager _entityManager;
        private readonly IEntityLinker _linker;
        private readonly IReachFilter _reachFilter;

        /// <summary>
        /// Lista reutilizada por la busqueda de objetivos. Es un campo y no una variable
        /// local porque la busqueda corre una vez por fotograma y crear una lista nueva cada
        /// vez seria basura para el recolector a cambio de nada.
        /// </summary>
        private readonly List<Candidate> _candidates = new List<Candidate>();

        /// <param name="reachFilter">Condicion externa de alcance, o null si no hay ninguna.
        /// Null es legitimo: sin motor detras, la busqueda sigue funcionando con lo que Core
        /// sabe, que es lo que permite probarla sin abrir Unity.</param>
        public WorldInteractionSystem(EntityManager entityManager, IEntityLinker linker,
                                      IReachFilter reachFilter = null)
        {
            AC.CheckNotNull(entityManager, nameof(entityManager));
            AC.CheckNotNull(linker, nameof(linker));

            _entityManager = entityManager;
            _linker = linker;
            _reachFilter = reachFilter;
        }

        public void UpdateOnEvent(GameEvent gameEvent)
        {
            if (gameEvent == null) return;

            switch (gameEvent.GetEventType())
            {
                case GameEventType.ItemDropped:
                    if (gameEvent is ItemLotEvent lotEvent) OnItemsDropped(lotEvent);
                    break;
            }
        }

        /// <summary>
        /// Pone en el mundo lo que acaba de salir de un inventario.
        ///
        /// Una tirada produce **un** monton aunque lleve varias variantes: el jugador hizo
        /// un gesto y espera ver un objeto en el suelo, no cinco superpuestos en el mismo
        /// punto. Si mas adelante hace falta que dos tiradas seguidas se fundan en el mismo
        /// monton, el sitio para decidirlo es aqui, buscando primero un monton cercano.
        /// </summary>
        private void OnItemsDropped(ItemLotEvent lotEvent)
        {
            if (lotEvent.Lots == null || lotEvent.Lots.Count == 0) return;

            IEntity pile = _entityManager.CreateEntity(EntityType.GroundLot);
            if (pile == null) return;

            pile.GetComponent<GroundLotComponent>().AddRange(lotEvent.Lots);
            PlaceAt(pile, lotEvent.GetEntity());

            _linker.Link(pile, EntityType.GroundLot);
        }

        /// <summary>
        /// Quita unidades de una variante de un monton y, si el monton se queda vacio, lo
        /// saca del mundo.
        ///
        /// Es la unica puerta para vaciar montones, y por eso <see cref="Despawn"/> sigue
        /// siendo privado: quien recoge solo dice cuanto se llevo, y decidir que un monton
        /// vacio desaparece es de quien lo creo. Si el vaciado y la destruccion fueran dos
        /// llamadas separadas, algun dia alguien haria la primera sin la segunda y quedaria
        /// un monton invisible de cero unidades al que se puede apuntar.
        /// </summary>
        /// <returns>Unidades quitadas de verdad.</returns>
        public int TakeFromLot(IEntity pile, ItemEntity variant, int units)
        {
            GroundLotComponent lot = pile?.GetComponent<GroundLotComponent>();
            if (lot == null) return 0;

            int removed = lot.RemoveUnits(variant, units);

            if (lot.IsEmpty) Despawn(pile);

            return removed;
        }

        /// <summary>
        /// Saca una entidad del mundo: de la escena y de Core.
        ///
        /// Es el reverso de <see cref="OnItemsDropped"/> y vive en el mismo sistema por la
        /// misma razon: quien llama a <c>Link</c> es quien llama a <c>Unlink</c>. Se descarto
        /// que <see cref="EntityManager.RemoveEntity"/> desenlazara por su cuenta porque la
        /// creacion no es simetrica —entre crear y enlazar hay que colocar—, y hacer
        /// automatico solo un lado dejaria dos reglas distintas para lo mismo. El precio es
        /// que otro sistema que destruya entidades tiene que acordarse; si aparece un
        /// segundo, esto se sube a <c>EntityManager</c>.
        ///
        /// <para>Hoy solo lo llama <see cref="TakeFromLot"/>, al vaciarse un monton.</para>
        /// </summary>
        private void Despawn(IEntity entity)
        {
            if (entity == null) return;

            _linker.Unlink(entity);
            _entityManager.RemoveEntity(entity.GetIdAsInt());
        }

        #region Busqueda de objetivo

        /// <summary>Hasta donde llega el jugador, medido hasta la superficie del volumen.</summary>
        private const float REACH = 1.5f;

        /// <summary>
        /// Coseno del semiangulo del cono de mirada. 0.707 son 45 grados a cada lado, o sea
        /// 90 de apertura: comodo para no tener que apuntar con precision, y estrecho para
        /// no recoger lo que tienes detras. Valor de partida, para afinar jugando.
        /// </summary>
        private const float VIEW_CONE_COS = 0.707f;

        private struct Candidate
        {
            public IEntity Entity;
            public float Distance;
        }

        /// <summary>
        /// Que tiene delante y a mano el actor ahora mismo, o null si nada.
        ///
        /// <para><b>Se llama una vez por fotograma, desde el actor.</b> Nunca al reves —
        /// preguntar por cada entidad si alcanza al jugador convertiria una busqueda en
        /// tantas como objetos haya.</para>
        ///
        /// <para>El orden de las comprobaciones importa y no es casual: primero lo barato
        /// que descarta mucho (distancia), luego lo barato que descarta el resto (cono), y
        /// solo al final el filtro externo, que es el caro. Ademas se recorre por cercania,
        /// asi que una pared delante del objeto mas proximo no te impide interactuar con el
        /// siguiente.</para>
        /// </summary>
        public IEntity FindTarget(IEntity actor)
        {
            if (actor == null) return null;

            PositionComponent actorPos = actor.GetComponent<PositionComponent>();
            if (actorPos == null) return null;

            CollectCandidates(actor, actorPos);
            _candidates.Sort((a, b) => a.Distance.CompareTo(b.Distance));

            foreach (Candidate candidate in _candidates)
            {
                if (_reachFilter == null || _reachFilter.CanReach(actor, candidate.Entity))
                    return candidate.Entity;
            }

            return null;
        }

        /// <summary>
        /// Si el actor alcanza AHORA MISMO a un objetivo que ya conoce.
        ///
        /// <para>Existe por paridad. <see cref="FindTarget"/> decide a que apuntas en un
        /// fotograma, y la accion se ejecuta despues —al soltar la tecla, al elegir en el
        /// menu—, cuando el jugador puede haberse movido. La ejecucion tiene que volver a
        /// preguntar lo mismo, y <c>FindTarget</c> no sirve para eso porque busca en vez de
        /// comprobar. Las dos comparten <see cref="IsWithinReach"/> y el mismo filtro, asi
        /// que no pueden discrepar sobre que esta a mano.</para>
        ///
        /// <para>Una entidad que ya no esta en el mundo no se alcanza: el objetivo guardado
        /// puede haberse recogido entre medias.</para>
        ///
        /// <para>Que el menu se cierre solo al alejarte es cosa de la interfaz y no sustituye
        /// esto: el alejamiento y la pulsacion pueden caer en el mismo fotograma, en
        /// cualquier orden.</para>
        /// </summary>
        public bool CanReach(IEntity actor, IEntity target)
        {
            if (actor == null || target == null || ReferenceEquals(actor, target)) return false;
            if (!ReferenceEquals(_entityManager.GetEntity(target.GetIdAsInt()), target)) return false;

            PositionComponent actorPos = actor.GetComponent<PositionComponent>();
            if (actorPos == null) return false;

            (float fx, float fy, float fz) = actorPos.Forward();
            if (!IsWithinReach(target, actorPos, fx, fy, fz, out _)) return false;

            return _reachFilter == null || _reachFilter.CanReach(actor, target);
        }

        /// <summary>
        /// Recoge lo que esta dentro del alcance y dentro del cono de mirada.
        /// </summary>
        private void CollectCandidates(IEntity actor, PositionComponent actorPos)
        {
            _candidates.Clear();

            (float fx, float fy, float fz) = actorPos.Forward();

            List<IEntity> reachable = _entityManager.GetEntitiesWithComponent(typeof(InteractionVolumeComponent));

            foreach (IEntity other in reachable)
            {
                if (ReferenceEquals(other, actor)) continue;
                if (!IsWithinReach(other, actorPos, fx, fy, fz, out float distance)) continue;

                _candidates.Add(new Candidate { Entity = other, Distance = distance });
            }
        }

        /// <summary>
        /// La parte geometrica del alcance: distancia a la superficie del volumen y cono de
        /// mirada. Lo comparten <see cref="FindTarget"/> y <see cref="CanReach"/>; el filtro
        /// externo queda fuera porque <c>FindTarget</c> lo aplica despues de ordenar.
        /// </summary>
        /// <param name="fx">Direccion de la mirada del actor, ya calculada: quien recorre
        /// muchos candidatos no deberia sacarla del cuaternion una vez por candidato.</param>
        private static bool IsWithinReach(IEntity target, PositionComponent actorPos,
                                          float fx, float fy, float fz, out float distance)
        {
            distance = float.MaxValue;

            PositionComponent targetPos = target.GetComponent<PositionComponent>();
            InteractionVolumeComponent volume = target.GetComponent<InteractionVolumeComponent>();
            if (targetPos == null || volume == null) return false;

            distance = volume.DistanceFrom(targetPos, actorPos.X, actorPos.Y, actorPos.Z);

            if (distance > REACH) return false;
            return IsInViewCone(actorPos, targetPos, fx, fy, fz);
        }

        /// <summary>
        /// Si el objetivo cae dentro del cono de mirada del actor.
        ///
        /// <para>Se mide contra el origen del objetivo y no contra el punto mas cercano de su
        /// volumen. Es una simplificacion consciente: usar el punto mas cercano haria que un
        /// objeto largo entrase en el cono por su punta, y el jugador lo viviria como
        /// apuntar a un sitio y recoger otro.</para>
        ///
        /// <para>Un objetivo practicamente encima del actor pasa siempre: ahi la direccion
        /// no esta definida y exigir que mire hacia sus propios pies seria absurdo.</para>
        /// </summary>
        private static bool IsInViewCone(PositionComponent actorPos, PositionComponent targetPos,
                                         float fx, float fy, float fz)
        {
            float dx = targetPos.X - actorPos.X;
            float dy = targetPos.Y - actorPos.Y;
            float dz = targetPos.Z - actorPos.Z;

            float length = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            if (length < 1e-4f) return true;

            float dot = (dx * fx + dy * fy + dz * fz) / length;
            return dot >= VIEW_CONE_COS;
        }

        /// <summary>
        /// Que puede hacer el actor con este objetivo.
        ///
        /// <para><b>El orden es la prioridad de diseno</b>, y eso lo convierte en parte del
        /// contrato: la accion de pulsar la tecla es la primera de la lista, y el menu las
        /// ofrece en este orden. Meter una accion nueva al principio cambia lo que hace la
        /// pulsacion corta sin que nada avise, asi que se anaden al final salvo que se
        /// quiera justamente eso.</para>
        /// </summary>
        public List<WorldAction> GetAvailableActions(IEntity actor, IEntity target)
        {
            List<WorldAction> actions = new List<WorldAction>();
            if (actor == null || target == null) return actions;

            GroundLotComponent lot = target.GetComponent<GroundLotComponent>();

            if (lot != null && !lot.IsEmpty)
            {
                actions.Add(WorldAction.PickUp);
                actions.Add(WorldAction.Inspect);
            }

            return actions;
        }

        /// <summary>
        /// La accion de la pulsacion corta: la primera de la lista, o null si no hay
        /// ninguna. Se deriva de la lista y no se decide aparte para que no pueda
        /// contradecirla.
        /// </summary>
        public WorldAction? GetDefaultAction(IEntity actor, IEntity target)
        {
            List<WorldAction> actions = GetAvailableActions(actor, target);
            return actions.Count == 0 ? (WorldAction?)null : actions[0];
        }

        #endregion

        /// <summary>Cuanto se aleja el monton de los pies del que tira, en metros.</summary>
        private const float DROP_DISTANCE = 0.6f;

        /// <summary>
        /// Altura de las manos como fraccion de la estatura. Las manos en reposo quedan
        /// alrededor de ahi, y expresarlo en proporcion en vez de en metros hace que un
        /// personaje mas bajo suelte mas bajo sin tocar nada.
        /// </summary>
        private const float HAND_HEIGHT_RATIO = 0.6f;

        /// <summary>Altura de soltado para quien no tiene cuerpo, como un arcon que vuelca.</summary>
        private const float DEFAULT_DROP_HEIGHT = 0.5f;

        /// <summary>
        /// Coloca el monton delante y a la altura de las manos del que tira.
        ///
        /// <para>Soltarlo en las coordenadas del que tira lo dejaria dentro de el, a la
        /// altura de los pies. Para separarlo hace falta saber hacia donde mira, y eso no es
        /// la rotacion sino el resultado de aplicarla al vector "delante": lo resuelve
        /// <see cref="PositionComponent.Forward"/>.</para>
        ///
        /// <para>La componente vertical no usa ese vector a proposito. Si el que tira mirase
        /// hacia abajo, un desplazamiento en la direccion de la mirada enterraria el monton
        /// en el suelo. Lo que se quiere es "un paso al frente y a la altura de las manos",
        /// asi que el frente se toma en horizontal y la altura se suma aparte.</para>
        ///
        /// <para>Si quien tira no tiene posicion —un arcon de los de prueba no la tiene— el
        /// monton se queda en el origen del mundo. Es visible y raro a proposito: mejor que
        /// aparezca en un sitio absurdo a que desaparezca sin dejar rastro.</para>
        /// </summary>
        private void PlaceAt(IEntity pile, IEntity dropper)
        {
            PositionComponent target = pile.GetComponent<PositionComponent>();
            PositionComponent source = dropper?.GetComponent<PositionComponent>();

            if (target == null || source == null) return;

            (float fx, _, float fz) = source.Forward();

            // El frente en horizontal: se descarta la componente vertical y se renormaliza.
            // Mirando casi en vertical el vector horizontal es casi nulo y normalizarlo
            // amplificaria ruido, asi que en ese caso se suelta a plomo.
            float horizontal = (float)Math.Sqrt(fx * fx + fz * fz);
            float offsetX = horizontal > 1e-4f ? fx / horizontal * DROP_DISTANCE : 0f;
            float offsetZ = horizontal > 1e-4f ? fz / horizontal * DROP_DISTANCE : 0f;

            target.SetPosition(source.X + offsetX,
                               source.Y + DropHeightOf(dropper),
                               source.Z + offsetZ);
        }

        /// <summary>
        /// A que altura salen las cosas de las manos de quien las tira.
        /// </summary>
        private float DropHeightOf(IEntity dropper)
        {
            BodyComponent body = dropper.GetComponent<BodyComponent>();
            return body == null ? DEFAULT_DROP_HEIGHT : body.Height * HAND_HEIGHT_RATIO;
        }
    }
}
