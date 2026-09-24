using System.Collections.Generic;
using Core.ECS.Entity;
using Core.Inventory;

namespace Core.ECS.Component
{
    /// <summary>
    /// Lo que hay en un monton tirado en el suelo.
    ///
    /// Un monton es una entidad del mundo, no un inventario: no tiene rejilla, no tiene
    /// techo de peso y nadie coloca nada dentro con precision. Solo guarda los lotes que
    /// alguien solto de una vez, y se recoge entero o por partes. Por eso no reutiliza
    /// <see cref="InventoryComponent"/>, que arrastraria una rejilla y unas reglas de
    /// colocacion que aqui no significan nada.
    ///
    /// Una tirada produce un monton, aunque suelte varias variantes a la vez: el jugador
    /// hizo un gesto y espera ver un objeto en el suelo, no cinco apilados en el mismo
    /// punto.
    /// </summary>
    public class GroundLotComponent : BasicComponent
    {
        private readonly List<SubLot> _lots;

        public GroundLotComponent() : this(null) {}

        public GroundLotComponent(IReadOnlyList<SubLot> lots)
        {
            _lots = lots == null ? new List<SubLot>() : new List<SubLot>(lots);
            _name = "GroundLotComponent";
        }

        public IReadOnlyList<SubLot> Lots => _lots;

        public bool IsEmpty => _lots.Count == 0;

        public int TotalUnits
        {
            get
            {
                int total = 0;
                foreach (SubLot lot in _lots) total += lot.Amount;
                return total;
            }
        }

        public float TotalWeight
        {
            get
            {
                float total = 0f;
                foreach (SubLot lot in _lots) total += lot.TotalWeight;
                return total;
            }
        }

        /// <summary>
        /// Item que representa al monton de cara al mundo: el del primer lote.
        ///
        /// Un monton mixto tiene que verse de alguna manera, y cualquier eleccion es
        /// arbitraria. Se toma el primero porque es el orden en que salieron del inventario
        /// y porque no depende de nada que pueda cambiar despues: elegir "el mas pesado" o
        /// "el mas numeroso" haria que el monton cambiara de aspecto al recoger una parte.
        /// </summary>
        public ItemEntity Representative => _lots.Count == 0 ? null : _lots[0].Item;

        public void Add(SubLot lot)
        {
            _lots.Add(lot);
        }

        public void AddRange(IReadOnlyList<SubLot> lots)
        {
            if (lots == null) return;
            foreach (SubLot lot in lots) _lots.Add(lot);
        }

        /// <summary>
        /// Los lotes se copian y las entidades no: una <see cref="ItemEntity"/> puede estar
        /// compartida entre varios lotes y es inmutable mientras lo este, asi que clonarla
        /// aqui crearia items nuevos sin que nadie lo pidiera.
        /// </summary>
        public override IComponent Clone()
        {
            return new GroundLotComponent(_lots);
        }

        public override bool Equivalent(IComponent other)
        {
            if (!(other is GroundLotComponent otherLot) || _lots.Count != otherLot._lots.Count)
                return false;

            for (int i = 0; i < _lots.Count; i++)
            {
                if (_lots[i].Amount != otherLot._lots[i].Amount) return false;
                if (!_lots[i].Equivalent(otherLot._lots[i])) return false;
            }

            return true;
        }
    }
}
