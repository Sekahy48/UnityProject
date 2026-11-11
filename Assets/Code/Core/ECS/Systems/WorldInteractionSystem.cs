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

            // Tirar una sola unidad es tirarla suelta: el item mismo pasa al mundo, sin
            // monton que lo envuelva. Asi un contenedor tirado (siempre es una unidad) es a
            // la vez lo que se alcanza y lo que se abre.
            if (lotEvent.Lots.Count == 1 && lotEvent.Lots[0].Amount == 1)
            {
                PlaceItem(lotEvent.Lots[0].Item, lotEvent.GetEntity());
                return;
            }

            IEntity pile = _entityManager.CreateEntity(EntityType.GroundLot);
            if (pile == null) return;

            pile.GetComponent<GroundLotComponent>().AddRange(lotEvent.Lots);
            PlaceAt(pile, lotEvent.GetEntity());

            _linker.Link(pile, EntityType.GroundLot);
        }

        /// <summary>
        /// Pone un item suelto en el mundo.
        ///
        /// <para>Se pone un CLON. Un item que sale de una pila puede ser la misma instancia
        /// que siguen usando las unidades que se quedan (una ItemEntity se comparte entre
        /// sublotes mientras es inmutable): darle posicion a esa instancia se la daria
        /// tambien a las del inventario.</para>
        ///
        /// <para>Pareja de <see cref="TakeItem"/>: lo que se pone aqui se quita alli.</para>
        /// </summary>
        private void PlaceItem(ItemEntity item, IEntity dropper)
        {
            if (item == null) return;

            ItemEntity placed = item.Clone();
            WorldPresence.AddTo(placed);
            PlaceAt(placed, dropper);

            _entityManager.Register(placed);
            _linker.Link(placed, EntityType.WorldItem);
        }

        /// <summary>
        /// Saca del mundo un item suelto que se acaba de recoger.
        ///
        /// Al inventario no va este item sino una copia limpia
        /// (<see cref="WorldPresence.CleanCopyOf"/>); este se destruye entero, igual que un
        /// monton vacio. Por eso basta con <see cref="Despawn"/>: desenlazar y dar de baja.
        /// </summary>
        public void TakeItem(ItemEntity item)
        {
            if (item == null || !WorldPresence.IsIn(item)) return;
            Despawn(item);
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

        /// <summary>
        /// Hasta donde llega el actor, medido DESDE LOS OJOS hasta la superficie del volumen.
        /// 2 m y no menos porque desde 1,7 m de altura algo en el suelo a tus pies ya esta a
        /// mas de 1,5. Constante de ajuste.
        /// </summary>
        private const float REACH = 2.0f;

        /// <summary>
        /// Margen extra sobre el alcance normal para decidir cuando algo abierto queda
        /// demasiado lejos y se cierra. Histeresis: si abrir y cerrar usaran el mismo limite,
        /// quedarse justo en el borde abriria y cerraria el panel con cualquier balanceo.
        /// </summary>
        private const float CLOSE_MARGIN = 0.5f;

        /// <summary>
        /// Coseno del semiangulo del cono de mirada: 0.94 son unos 20 grados a cada lado.
        /// Estrecho porque ahora la mirada incluye la inclinacion de la camara y se apunta
        /// con el centro de la pantalla; con 45 grados, apuntar a una manzana no la separaba
        /// de la de al lado. Constante de ajuste.
        /// </summary>
        private const float VIEW_CONE_COS = 0.94f;

        /// <summary>
        /// Altura de los ojos como fraccion de <c>BodyComponent.Height</c>. Se deriva de la
        /// estatura, como la altura de la mano al tirar, en vez de fijarse: un personaje
        /// bajito mira desde mas abajo sin tocar nada.
        /// </summary>
        private const float EYE_HEIGHT_RATIO = 0.94f;

        /// <summary>Altura de los ojos de quien no tiene cuerpo.</summary>
        private const float DEFAULT_EYE_HEIGHT = 1.6f;

        private struct Candidate
        {
            public IEntity Entity;
            public float Distance;
            public float Aim;
        }

        /// <summary>
        /// Desde donde y hacia donde mira un actor, ya resuelto para este fotograma.
        /// Struct y no tupla de seis floats porque viaja por tres metodos y los nombres
        /// importan: origen y direccion no se pueden confundir.
        /// </summary>
        private readonly struct Gaze
        {
            public readonly float Ox, Oy, Oz;
            public readonly float Dx, Dy, Dz;

            public Gaze(float ox, float oy, float oz, float dx, float dy, float dz)
            {
                Ox = ox; Oy = oy; Oz = oz;
                Dx = dx; Dy = dy; Dz = dz;
            }
        }

        /// <summary>
        /// Que tiene delante y a mano el actor ahora mismo, o null si nada.
        ///
        /// <para><b>Se llama una vez por fotograma, desde el actor.</b> Nunca al reves —
        /// preguntar por cada entidad si alcanza al jugador convertiria una busqueda en
        /// tantas como objetos haya.</para>
        ///
        /// <para><b>Gana el mas centrado, no el mas cercano.</b> Con la mirada del jugador
        /// disponible, lo que espera es "lo que tengo en el centro de la pantalla": si
        /// ganara el mas cercano, apuntar a una manzana a 1,2 m te la robaria otra a 0,8 m
        /// en el borde del cono. La distancia solo desempata. Lo que el rayo de la mirada
        /// atraviesa cuenta como perfectamente centrado, asi que entre dos cosas en la misma
        /// linea gana la de delante.</para>
        ///
        /// <para>El filtro externo, el caro, va al final y en ese mismo orden: una pared
        /// delante del mas centrado no impide interactuar con el siguiente.</para>
        /// </summary>
        public IEntity FindTarget(IEntity actor)
        {
            if (actor == null) return null;

            Gaze? gaze = GazeOf(actor);
            if (!gaze.HasValue) return null;

            CollectCandidates(actor, gaze.Value);
            _candidates.Sort((a, b) =>
            {
                int byAim = b.Aim.CompareTo(a.Aim);
                return byAim != 0 ? byAim : a.Distance.CompareTo(b.Distance);
            });

            foreach (Candidate candidate in _candidates)
            {
                if (IsInSight(actor, candidate.Entity, gaze.Value))
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

            Gaze? gaze = GazeOf(actor);
            if (!gaze.HasValue) return false;

            if (!IsWithinReach(target, gaze.Value, out _, out _)) return false;

            return IsInSight(actor, target, gaze.Value);
        }

        public bool IsInRange(IEntity actor, IEntity target)
        {
            if (actor == null || target == null || ReferenceEquals(actor, target)) return false;
            if (!ReferenceEquals(_entityManager.GetEntity(target.GetIdAsInt()), target)) return false;

            Gaze? gaze = GazeOf(actor);
            if (!gaze.HasValue) return false;

            InteractionVolumeComponent volume = target.GetComponent<InteractionVolumeComponent>();
            PositionComponent targetPos = target.GetComponent<PositionComponent>();
            if (volume == null || targetPos == null) return false;

            float distance = volume.DistanceFrom(targetPos, gaze.Value.Ox, gaze.Value.Oy, gaze.Value.Oz);
            return distance <= REACH + CLOSE_MARGIN;
        }

        /// <summary>
        /// Recoge lo que esta dentro del alcance y dentro del cono de mirada.
        /// </summary>
        private void CollectCandidates(IEntity actor, Gaze gaze)
        {
            _candidates.Clear();

            List<IEntity> reachable = _entityManager.GetEntitiesWithComponent(typeof(InteractionVolumeComponent));

            foreach (IEntity other in reachable)
            {
                if (ReferenceEquals(other, actor)) continue;
                if (!IsWithinReach(other, gaze, out float distance, out float aim)) continue;

                _candidates.Add(new Candidate { Entity = other, Distance = distance, Aim = aim });
            }
        }

        /// <summary>
        /// Desde donde y hacia donde mira el actor.
        ///
        /// <para>El origen son los ojos, no los pies. Medir desde los pies, con la mirada
        /// horizontal del cuerpo, dejaba una franja de unos 30 cm de distancia en la que un
        /// monton recien tirado —a la altura de la mano— entraba en el cono: mas cerca
        /// quedaba demasiado alto, mas lejos fuera de alcance.</para>
        ///
        /// <para>La direccion sale de <see cref="GazeComponent"/>, que escribe la camara con
        /// su inclinacion. Quien no lo tenga —o no lo tenga aun escrito— mira hacia donde
        /// apunta su cuerpo, que es lo que hacia todo antes y deja la busqueda funcionando
        /// sin motor detras.</para>
        /// </summary>
        private static Gaze? GazeOf(IEntity actor)
        {
            PositionComponent pos = actor.GetComponent<PositionComponent>();
            if (pos == null) return null;

            BodyComponent body = actor.GetComponent<BodyComponent>();
            float eye = body == null ? DEFAULT_EYE_HEIGHT : body.Height * EYE_HEIGHT_RATIO;

            GazeComponent gaze = actor.GetComponent<GazeComponent>();
            (float dx, float dy, float dz) = gaze != null && gaze.HasDirection
                ? gaze.Direction
                : pos.Forward();

            return new Gaze(pos.X, pos.Y + eye, pos.Z, dx, dy, dz);
        }

        /// <summary>
        /// La parte geometrica del alcance: distancia desde los ojos a la superficie del
        /// volumen, y punteria. Lo comparten <see cref="FindTarget"/> y
        /// <see cref="CanReach"/>; el filtro externo queda fuera porque <c>FindTarget</c> lo
        /// aplica despues de ordenar.
        /// </summary>
        /// <param name="aim">Lo centrado que esta: 1 si el rayo de la mirada lo atraviesa,
        /// si no el coseno del angulo hasta su centro. Mayor es mejor.</param>
        private static bool IsWithinReach(IEntity target, Gaze gaze, out float distance, out float aim)
        {
            distance = float.MaxValue;
            aim = -1f;

            PositionComponent targetPos = target.GetComponent<PositionComponent>();
            InteractionVolumeComponent volume = target.GetComponent<InteractionVolumeComponent>();
            if (targetPos == null || volume == null) return false;

            distance = volume.DistanceFrom(targetPos, gaze.Ox, gaze.Oy, gaze.Oz);
            if (distance > REACH) return false;

            aim = AimAt(volume, targetPos, gaze);
            return aim >= VIEW_CONE_COS;
        }

        /// <summary>
        /// Si desde los ojos se ve el objetivo: la parte que decide el motor.
        ///
        /// <para>Se traza primero al centro del volumen y, si esta tapado, al punto mas alto.
        /// Solo al centro, una manzana detras de una valla baja con el borde asomando saldria
        /// tapada; el segundo rayo solo se paga en esos casos dudosos.</para>
        ///
        /// <para>Sin filtro (sin motor detras) todo esta a la vista.</para>
        /// </summary>
        private bool IsInSight(IEntity actor, IEntity target, Gaze gaze)
        {
            if (_reachFilter == null) return true;

            PositionComponent targetPos = target.GetComponent<PositionComponent>();
            InteractionVolumeComponent volume = target.GetComponent<InteractionVolumeComponent>();
            if (targetPos == null || volume == null) return false;

            (float cx, float cy, float cz) = volume.CenterInWorld(targetPos);
            if (_reachFilter.IsClear(actor, target, gaze.Ox, gaze.Oy, gaze.Oz, cx, cy, cz)) return true;

            (float x, float y, float z)? top = volume.TopPointInWorld(targetPos);
            return top.HasValue
                && _reachFilter.IsClear(actor, target, gaze.Ox, gaze.Oy, gaze.Oz,
                                        top.Value.x, top.Value.y, top.Value.z);
        }

        /// <summary>
        /// Lo centrado que esta un objetivo en la mirada.
        ///
        /// <para>Se mide contra el <b>centro del volumen</b>, no contra el origen de la
        /// entidad (que esta en su base): apuntar al centro de un arcon es apuntar al arcon,
        /// no a sus patas.</para>
        ///
        /// <para>Pero un objeto grande visto de cerca puede ocupar mas angulo que el cono:
        /// mirando la esquina de un arcon, su centro queda fuera. Por eso, ademas, si el rayo
        /// de la mirada <b>atraviesa</b> el volumen cuenta como perfectamente centrado. Se
        /// comprueba con el punto del rayo mas cercano al centro: si ese punto esta dentro,
        /// el rayo pasa por dentro. Es una aproximacion (una caja alargada vista de canto
        /// puede escaparsele) y no pretende ser un raycast: basta para lo que se decide.</para>
        ///
        /// <para>Un objetivo practicamente en los ojos pasa siempre: ahi la direccion no
        /// esta definida.</para>
        /// </summary>
        private static float AimAt(InteractionVolumeComponent volume, PositionComponent targetPos, Gaze gaze)
        {
            (float cx, float cy, float cz) = volume.CenterInWorld(targetPos);

            float vx = cx - gaze.Ox, vy = cy - gaze.Oy, vz = cz - gaze.Oz;
            float length = (float)Math.Sqrt(vx * vx + vy * vy + vz * vz);
            if (length < 1e-4f) return 1f;

            float along = vx * gaze.Dx + vy * gaze.Dy + vz * gaze.Dz;

            if (along > 0f)
            {
                float px = gaze.Ox + gaze.Dx * along;
                float py = gaze.Oy + gaze.Dy * along;
                float pz = gaze.Oz + gaze.Dz * along;
                if (volume.DistanceFrom(targetPos, px, py, pz) <= 0f) return 1f;
            }

            return along / length;
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
            InventoryComponent inventoryComponent = target.GetComponent<InventoryComponent>();

            if (lot != null && !lot.IsEmpty)
            {
                actions.Add(WorldAction.PickUp);
                actions.Add(WorldAction.Inspect);
            }
            else if (target is ItemEntity)
            {
                // Un item suelto: lo mismo que un monton de una unidad.
                actions.Add(WorldAction.PickUp);
                actions.Add(WorldAction.Inspect);
            }

            if (inventoryComponent != null)
            {
                actions.Add(WorldAction.Inventory);
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
