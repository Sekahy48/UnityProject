using System.Collections.Generic;
using Core.Contexts;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems;
using Core.Events;
using Core.Inventory;
using AC = Core.Utils.ArgumentChecker;

namespace Core.Services
{
    /// <summary>
    /// Coreografo de las acciones entre el mundo y un inventario.
    ///
    /// <para>Existe por la misma razon que <see cref="InventoryService"/>: recoger necesita a
    /// <see cref="WorldInteractionSystem"/> (que hay en el suelo y cuando desaparece) y a
    /// <see cref="InventorySystem"/> (que cabe y donde), y los sistemas no se conocen entre si.
    /// Se descarto que <c>WorldInteractionSystem</c> tirara del <c>SystemManager</c> para
    /// llegar al inventario: seria un sistema componiendo a otro, que es justo lo que los
    /// servicios vienen a evitar.</para>
    ///
    /// <para>No se fundio con <c>InventoryService</c> porque aquel ya es grande y todo lo suyo
    /// gira alrededor de la mano y de <c>RunTransfer</c>; aqui no hay mano ni rollback.</para>
    /// </summary>
    public class WorldInteractionService
    {
        private readonly GameSystemContext _systemContext;

        public WorldInteractionService(GameSystemContext systemContext)
        {
            AC.CheckNotNull(systemContext, nameof(systemContext));
            _systemContext = systemContext;
        }

        private WorldInteractionSystem World =>
            _systemContext.SystemManager.GetReactiveSystem<WorldInteractionSystem>();

        private InventorySystem Inventories =>
            _systemContext.SystemManager.GetReactiveSystem<InventorySystem>();

        /// <summary>
        /// Por que no entro todo. Un solo motivo por gesto: si se dan los dos, gana
        /// <see cref="Weight"/>, porque el peso lleno bloquea todo lo que venga detras y la
        /// rejilla solo bloquea tipo a tipo —una piedra puede no caber y una aguja si—.
        /// </summary>
        private enum PickUpLimit
        {
            None,
            Volume,
            Weight
        }

        #region Recoger

        /// <summary>
        /// Lleva al inventario del actor todo lo que quepa del monton.
        ///
        /// <para><b>Paridad:</b> empieza repitiendo las dos preguntas que pintaron la accion
        /// —<see cref="WorldInteractionSystem.CanReach"/> y
        /// <see cref="WorldInteractionSystem.GetAvailableActions"/>—, porque entre verla y
        /// ejecutarla el jugador ha podido moverse o el monton desaparecer.</para>
        ///
        /// <para><b>Mover cero es un resultado valido.</b> Recoger se ofrece aunque no quepa
        /// nada, porque una tecla que no aparece no le explica al jugador por que. En ese caso
        /// no se mueve nada y se avisa del motivo.</para>
        ///
        /// <para>Destino: el inventario raiz del actor, sin entrar en contenedores equipados,
        /// igual que la transferencia rapida. Meter en la mochila es abrirla y colocar.</para>
        /// </summary>
        /// <returns>Unidades que se quedan en el suelo.</returns>
        public int PickUp(IEntity actor, IEntity pile)
        {
            WorldInteractionSystem world = World;
            if (!world.CanReach(actor, pile)) return UnitsOn(pile);
            if (!world.GetAvailableActions(actor, pile).Contains(WorldAction.PickUp)) return UnitsOn(pile);

            InventoryComponent inventory = actor.GetComponent<InventoryComponent>();
            if (inventory == null) return UnitsOn(pile);

            InventorySystem inventories = Inventories;
            PickUpLimit limit = PickUpLimit.None;
            int left = 0;

            // Copia: TakeFromLot quita lotes del monton, y al ultimo lo destruye.
            List<SubLot> lots = new List<SubLot>(pile.GetComponent<GroundLotComponent>().Lots);

            foreach ((ItemEntity variant, int amount) in lots)
            {
                // Se pregunta por lote y justo antes de mover: cada respuesta tiene que ver
                // el peso que dejaron los lotes anteriores. source null porque lo recogido
                // viene de fuera del arbol y no hay techo del que eximirlo.
                int fitByWeight = inventory.Inventory.FitByWeight(variant, amount, null);

                int notPlaced = inventories.TryStackOntoHere(actor, variant, amount, announce: false);
                int placed = amount - notPlaced;

                limit = Worst(limit, LimitOf(amount, fitByWeight, placed));
                left += notPlaced;

                if (placed > 0) world.TakeFromLot(pile, variant, placed);
            }

            // Una ronda de eventos por gesto, no una por lote. fullGrid va a false a
            // proposito: el aviso de "no cabe" sale de Announce, con el motivo ya resuelto.
            inventories.EvaluateAndFireEvents(actor, false);
            Announce(actor, inventory, limit);

            return left;
        }

        /// <summary>
        /// Por que un lote no entro entero, deducido sin tocar <c>TryStackOntoHere</c>.
        ///
        /// Ese metodo pregunta primero el peso y luego coloca en la rejilla, y devuelve un
        /// solo numero. Si se coloco menos de lo que el peso permitia, lo que freno fue la
        /// rejilla; si no, fue el peso. Se deduce aqui en vez de cambiar su firma porque ese
        /// metodo tiene mas llamadas, y porque el motivo del rechazo se dejo fuera del
        /// inventario a proposito (ver <c>TransferResult</c> en FASE1_HITOS, M6).
        /// </summary>
        private static PickUpLimit LimitOf(int amount, int fitByWeight, int placed)
        {
            if (placed < fitByWeight) return PickUpLimit.Volume;
            if (fitByWeight < amount) return PickUpLimit.Weight;
            return PickUpLimit.None;
        }

        private static PickUpLimit Worst(PickUpLimit a, PickUpLimit b) => a >= b ? a : b;

        /// <summary>
        /// Unico punto que publica por que algo se quedo en el suelo. Con el motivo ya
        /// resuelto aqui, quien escuche recibe uno solo y no tiene que saber que el peso
        /// manda sobre la rejilla.
        ///
        /// Hoy nadie escucha estos eventos; el log es el aviso hasta que exista el HUD.
        /// </summary>
        private static void Announce(IEntity actor, InventoryComponent inventory, PickUpLimit limit)
        {
            switch (limit)
            {
                case PickUpLimit.Weight:
                    EventBus.GetInstance().Post(new GameEvent(GameEventType.WeightLimitReached, actor, inventory));
                    CoreLogger.Instance.Log("Peso completo: parte del monton se queda en el suelo.");
                    break;
                case PickUpLimit.Volume:
                    EventBus.GetInstance().Post(new GameEvent(GameEventType.InventoryFull, actor, inventory));
                    CoreLogger.Instance.Log("Demasiado volumen: parte del monton se queda en el suelo.");
                    break;
            }
        }

        private static int UnitsOn(IEntity pile) =>
            pile?.GetComponent<GroundLotComponent>()?.TotalUnits ?? 0;

        #endregion
    }
}
