using System;
using System.Collections.Generic;
using Core.ECS.Component;
using Core.ECS.Entity;
using Core.ECS.Systems; 
using AC = Core.Utils.ArgumentChecker;

namespace Core.Inventory
{
    public class InventoryObject : IInventoryElement
    {
        private const int BASE_GRID_W = 7;
        private const int BASE_GRID_H = 5;

        /* Margen contra la coma flotante al traducir kilos libres a piezas. Ver FitByWeight. */
        private const float WEIGHT_EPSILON = 1e-4f;

        private List<IInventoryElement> _inventory;
        private TetrisGridState _grid;
        private int _id;
        private int _nodeId;
        private ItemEntity _item;

        /* Inventario que contiene a este, o null si es una raiz. Solo se conoce al padre
           inmediato: la pregunta del peso sube por la cadena y nadie necesita saber cuantos
           niveles hay ni quien esta arriba del todo. */
        public InventoryObject Parent {get; private set;}

        /* Entidad a la que pertenece este inventario: el arcon, la mochila, el personaje. De
           ella sale su techo de peso, y por eso una raiz sin entidad no tiene techo propio. */
        private IEntity _holder;

        public InventoryObject(ItemEntity item)
        {
            AC.CheckNotNull(item, item.GetCompoundIdentification().ToString());
            _id = item.GetComponent<BaseItemComponent>().TypeId;
            _nodeId = NodeIdGenerator.GenerateId();
            _item = item;
            _holder = item;
            _inventory = new List<IInventoryElement>();

            StorageComponent storage = item.GetComponent<StorageComponent>();
            _grid = new TetrisGridState(storage.GridH, storage.GridW);
        }

        /// <param name="holder">Entidad dueña de este inventario, de la que sale su techo de
        /// peso. Null deja el inventario sin techo propio.</param>
        public InventoryObject(IEntity holder = null)
        {
            _id = 0;
            _nodeId = NodeIdGenerator.GenerateId();
            _item = null;
            _holder = holder;
            _inventory = new List<IInventoryElement>();
            _grid = new TetrisGridState(BASE_GRID_H, BASE_GRID_W);
        }

        public int GetTypeId() => _id;
        public int GetNodeId() => _nodeId;
        public ItemEntity GetItemEntity() => _item;

        /// <summary>
        /// Reapunta este inventario a la entidad a la que pertenece.
        ///
        /// Existe por el clonado: Clone copia el arbol pero no la entidad, asi que el clon
        /// nace apuntando al item ORIGINAL y sacaria de el su techo de peso. Quien clona la
        /// entidad es el unico que conoce la nueva, y la enchufa aqui.
        /// </summary>
        public void Rebind(ItemEntity owner)
        {
            AC.CheckNotNull(owner, nameof(owner));

            _item = owner;
            _holder = owner;
        }
        public bool IsLeaf() => false;

        /// <summary>Un contenedor no es una pila: no tiene variantes que desglosar.</summary>
        public bool HasVariants() => false;
        public int GetAmount() => 1;
        public void SetAmount(int amount) { } // containers don't have an amount
        public List<IInventoryElement> GetChildren() => new List<IInventoryElement>(_inventory);
        public TetrisGridState GetGrid() => _grid;



        //#region BFS helper

        private IInventoryElement BfsFind(int id)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetTypeId().Equals(id))
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        private IInventoryElement BfsFindByNodeId(int nodeId)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetNodeId().Equals(nodeId))
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        private List<IInventoryElement> BfsFindAll(int id)
        {
            List<IInventoryElement> results = new List<IInventoryElement>();
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.GetTypeId().Equals(id))
                    results.Add(current);
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return results;
        }

        private IInventoryElement BfsFindEquivalent(ItemEntity item)
        {
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.IsLeaf() && current.GetTypeId() == item.GetComponent<BaseItemComponent>().TypeId)
                    return current;
                if (!current.IsLeaf())
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
            }
            return null;
        }

        //#endregion 

        //#region Global operations 

        public int AddItem(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, nameof(item));
            AC.CheckPositive(amount, nameof(amount));

            if (ContainerOf(item) != null)
                return PlaceContainer(item, node => _grid.TryFirstPlace(node) ? 0 : 1, amount);

            while (amount > 0)
            {
                ItemObject newNode = new ItemObject(item, amount);
                if (!_grid.TryFirstPlace(newNode)) return amount;   // no cabe: devuelve lo que sobra

                _inventory.Add(newNode);
                amount -= newNode.GetAmount();
            }

            return 0;
        }

        /// <summary>
        /// El inventario que un item trae consigo, o null si no es un contenedor.
        ///
        /// Un item con inventario se guarda en una rejilla como la RAMA que ya es, no envuelto
        /// en una hoja nueva: envolverlo dejaba su contenido fuera del arbol, y lo que no cuelga
        /// del arbol no pesa — una mochila pesaba distinto puesta que guardada.
        /// </summary>
        private static InventoryObject ContainerOf(ItemEntity item)
            => item.GetComponent<InventoryComponent>()?.Inventory;

        /// <summary>
        /// Cuelga aqui el inventario propio de un contenedor, colocandolo con la estrategia que
        /// reciba.
        /// </summary>
        /// <param name="place">Coloca el nodo en la rejilla y devuelve lo que no pudo colocar.</param>
        /// <returns>Unidades no admitidas: un contenedor es siempre UNO, asi que lo que pase de
        /// ahi vuelve intacto — dos nodos apuntando al mismo inventario serian el mismo
        /// inventario en dos celdas.</returns>
        private int PlaceContainer(ItemEntity item, Func<IInventoryElement, int> place, int amount)
        {
            InventoryObject container = ContainerOf(item);

            // El contenedor no puede envolver a su destino: ahi es donde estaria el ciclo. Al
            // reves no: que ESTE inventario contenga ya al contenedor es solo recolocarlo.
            if (container.WrapsOrIs(this)) return amount;
            if (place(container) != 0) return amount;

            container.Parent = this;
            _inventory.Add(container);

            return amount - 1;
        }

        /// <summary>
        /// Adds an already-built node to this inventory <b>without placing it on the grid</b>.
        /// Unlike AddItem, which builds its own nodes and needs free cells, this takes a node
        /// that already exists and only makes this inventory its owner.
        ///
        /// <para>Exists for staging containers: a node that has to belong to some inventory to
        /// be operated on (HandBuffer.Grab requires an owner, and the move transaction extracts
        /// the units through it) but is never rendered and never competes for space. Items spawned by
        /// the dev creative panel start here.</para>
        ///
        /// <para>Do NOT use it for the player inventory or a real container: a node outside the
        /// grid occupies no cells, is invisible to RenderGridItems, and CleanNode would find
        /// nothing to free. Grid space is the capacity system, and this bypasses it.</para>
        /// </summary>
        public void AddNode(ItemObject node)
        {
            AC.CheckNotNull(node, nameof(node));
            _inventory.Add(node);
        }

        public void AddContainer(ItemEntity item)
        {
            InventoryObject child = new InventoryObject(item);
            child.Parent = this;
            _inventory.Add(child);
        }

        public void AddContainer(InventoryObject container)
        {
            container.Parent = this;
            _inventory.Add(container);
        }

        /// <summary>
        /// Saca un contenedor de este inventario y lo deja suelto.
        ///
        /// Perder el padre no es un efecto secundario, es el punto: mientras estaba aqui su
        /// peso subia por esta cadena, y al salir deja de hacerlo. Sin esto quedaria una
        /// mochila en el suelo que sigue pesando sobre quien la llevaba.
        /// </summary>
        /// <returns>True si el contenedor estaba aqui.</returns>
        public bool RemoveContainer(InventoryObject container)
        {
            AC.CheckNotNull(container, nameof(container));

            if (!_inventory.Remove(container)) return false;

            container.Parent = null;

            return true;
        }

        /// <summary>
        /// Cuantas de esas unidades caben aqui, contando este inventario y todos los que lo
        /// contienen.
        ///
        /// Derivada de FreeWeight, no paralela a ella: el limite es de kilos y esto solo lo
        /// traduce a piezas. Si fueran dos recorridos distintos, el dia que uno cambie el otro
        /// se queda contando otra cosa — y uno pinta el fantasma y el otro escribe el aviso.
        /// </summary>
        public int FitByWeight(ItemEntity item, int amount, InventoryObject source = null)
        {
            float unit = item.GetComponent<BaseItemComponent>().Weight;
            if (unit <= 0) return amount;   // sin peso no hay limite que aplicar

            float free = FreeWeight(source);
            if (free <= 0) return 0;        // pasado el techo no cabe nada, no "menos que nada"

            // El epsilon es por la coma flotante: con 8 kilos libres y una pieza de 8, la
            // division puede dar 0,9999 y dejar fuera algo que cabe justo.
            float fit = (free + WEIGHT_EPSILON) / unit;

            // Se compara ANTES de convertir a entero: un techo exento vale float.MaxValue, y
            // ese cociente no cabe en un int — la conversion lo dejaba en int.MinValue y todo
            // salia bloqueado justo en los casos que no tenian limite.
            return fit >= amount ? amount : (int)fit;
        }

        /// <summary>
        /// Si este inventario es el dado, o uno de sus antepasados.
        ///
        /// Existe para lo unico que el arbol no puede permitirse: meter un contenedor dentro de
        /// si mismo o de algo que ya lleva dentro. Con la mochila abierta en un panel lateral,
        /// arrastrarla a su propia rejilla es un gesto de dos segundos, y dejaria un ciclo en el
        /// que ningun recorrido —peso, capacidad, limpieza— termina.
        /// </summary>
        public bool WrapsOrIs(InventoryObject other)
        {
            for (InventoryObject current = other; current != null; current = current.Parent)
                if (ReferenceEquals(current, this)) return true;

            return false;
        }

        /// <summary>
        /// Kilos que este inventario admite POR SU CUENTA, sin mirar a quien lo lleva.
        ///
        /// <para>Aqui vive la UNICA regla que hay sobre el origen: un techo no aplica a algo que
        /// ya esta debajo de el. Mover del bolsillo a la mochila, o de la mochila al jugador, no
        /// cambia lo que el jugador carga. Antes eso lo apañaba el ignoreNodeId de la rejilla,
        /// que solo sabia de un mismo inventario y no de la cadena.</para>
        ///
        /// <para>Sin entidad dueña no hay techo: el inventario de paso que fabrica
        /// SpawnIntoHand no es de nadie y no limita nada.</para>
        /// </summary>
        /// <param name="source">Inventario del que sale lo que se va a mover, o null si viene de
        /// fuera del arbol: el equipo, el catalogo, el mundo.</param>
        public float OwnFreeWeight(InventoryObject source = null)
            => _holder == null || (source != null && WrapsOrIs(source))
                ? float.MaxValue
                : CarryCapacity.GetMaxLoad(_holder) - GetTotalWeight();

        /// <summary>
        /// Kilos que admite de verdad: el minimo de toda la cadena.
        ///
        /// Unico recorrido hacia arriba de todo el sistema de peso. Lo que necesite otra forma
        /// de la misma pregunta —unidades, si frena el portador, cuanto pintar de la barra— se
        /// deriva de aqui en vez de recorrer por su cuenta.
        /// </summary>
        public float FreeWeight(InventoryObject source = null)
            => Parent == null
                ? OwnFreeWeight(source)
                : Math.Min(OwnFreeWeight(source), Parent.FreeWeight(source));

        /// <summary>
        /// Si lo que impide meter ese peso es alguien que CONTIENE a este inventario, y no su
        /// propio techo.
        ///
        /// Depende del peso intentado a proposito: con la mochila a 5/30 y el portador al
        /// limite, una venda puede entrar y tres no, asi que la respuesta no es una propiedad
        /// del inventario sino de lo que se esta intentando meter.
        ///
        /// <para>Las dos condiciones hacen falta: la primera dice que no cabe, la segunda que
        /// el culpable esta arriba. Sin la segunda, una mochila llena por su cuenta tambien
        /// diria que la limita el portador.</para>
        /// </summary>
        /// <param name="source">El mismo que en FreeWeight: sin el, el aviso saldria al mover
        /// algo que ya cargaba el portador, justo cuando el fantasma dice que si cabe.</param>
        public bool CarrierBlocks(float weight, InventoryObject source = null)
            => weight > FreeWeight(source) && FreeWeight(source) < OwnFreeWeight(source);

        public int StackOnto(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            IInventoryElement match = BfsFindEquivalent(item);
            if (match != null && match.IsLeaf()) 
                amount = match.StackOntoHere(item, amount); 

            if (amount > 0)
                amount = AddItem(item, amount); 
            return amount;
        }


        /// <summary>
        /// Modifies the first node found holding this typeId, consuming at random across
        /// its sub-lots. Use it for "remove N units of this item from wherever", e.g. a
        /// recipe consuming materials. When the exact node and variant are known, prefer
        /// the overload taking an ItemObject: it skips the BFS and the full CleanTree.
        ///
        /// Cleanup goes through CleanTree, not CleanNode: BfsFind searches the whole tree,
        /// so the node may live inside a nested container — and only that container can
        /// free the grid cells it sits on. CleanNode would find nothing and silently
        /// leave an empty node holding its cells.
        /// </summary>
        public int ModifyAmount(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            IInventoryElement found = BfsFind(id);
            if (found == null) return 0;

            int modified = found.ModifyAmount(id, amount);

            // Solo hay algo que limpiar si el nodo se ha vaciado. Consumir 3 de 20 no
            // justifica recorrer el arbol entero, que es el caso mas frecuente.
            if (found.GetAmount() <= 0) CleanTree();

            return modified;
        }

        /// <summary>
        /// Modifies a node held directly by this inventory, and removes the node if it
        /// ends up empty. Preferred over ModifyAmount(int, int) when the caller already
        /// knows which node it is operating on — no BFS, and no full CleanTree traversal.
        ///
        /// <paramref name="item"/> selects the granularity:
        /// pass it to target one specific sub-lot (moving the rusty swords, not the new
        /// ones), or leave it null to treat the node as a whole, spreading the change
        /// across its sub-lots at random — which is what "place one at a time from a mixed
        /// stack" means, and there is no meaningful way to choose.
        /// </summary>
        /// <param name="node">Node to modify. Must be a direct child of this inventory. TODO revisar comentario</param>
        /// <param name="item">Sub-lot to target (matched by Equivalent), or null for the whole node.</param>
        /// <param name="amount">Positive adds, negative consumes.</param>
        /// <param name="clean">
        /// Whether to drop the node when it ends up empty. Pass false inside a transaction
        /// that may still put units back: cleaning would destroy the node and the rollback
        /// would have to recreate it at its old coordinates. The caller is then responsible
        /// for calling CleanNode once the operation settles.
        /// </param>
        /// <returns>Units actually applied, always positive.</returns>
        public int ModifyAmount(ItemObject node, ItemEntity item, int amount, bool clean = true)
        {
            AC.CheckNotNull(node, nameof(node));

            int modified = item != null
                ? node.ModifyAmount(item, amount)
                : node.ModifyAmount(node.GetTypeId(), amount);

            if (clean && node.GetAmount() <= 0)
                CleanNode(node);

            return modified;
        }

        /// <summary>
        /// Takes units out of a node held directly by this inventory, reporting which variants
        /// came out. Same granularity rules as ModifyAmount: pass an item to take from one
        /// sub-lot, or null to take at random across the node.
        /// </summary>
        /// <param name="clean">
        /// Whether to drop the node if it ends up empty. Pass false inside a transaction that
        /// may put units back — see InventorySystem.TryMoveItemTo.
        /// </param>
        /// <returns>Pairs of (variant, units taken). Feed THESE to the destination: the
        /// breakdown is the point, a bare total would collapse mixed stacks into one variant.</returns>
        /// <summary>
        /// Saca unidades de un HIJO de este inventario, y lo retira si se queda vacio.
        ///
        /// Es la operacion del padre: envuelve el Extract del hijo con la limpieza, que es lo
        /// unico que el hijo no puede hacer por si mismo — quien posee las celdas y la lista
        /// es el contenedor, no el nodo.
        /// </summary>
        /// <param name="clean">False cuando lo extraido puede volver: un nodo limpiado habria
        /// que recrearlo en sus mismas coordenadas. No aplica a las ramas — ver abajo.</param>
        public List<SubLot> ExtractFrom(IInventoryElement child, ItemEntity variant, int amount, bool clean = true)
        {
            AC.CheckNotNull(child, nameof(child));

            List<SubLot> extracted = child.Extract(variant, amount);

            // Una hoja se retira cuando se queda sin unidades. Una rama se retira en cuanto
            // sale, porque no se divide: si Extract devolvio algo, el contenedor entero se fue.
            // El 'clean' no le aplica — existe para que los SOBRANTES puedan volver, y de una
            // rama no hay sobrantes. Lo que la devuelve es ReattachContainer.
            if (!child.IsLeaf())
            {
                if (extracted.Count > 0) CleanNode(child);
            }
            else if (clean && child.GetAmount() <= 0)
            {
                CleanNode(child);
            }

            return extracted;
        }

        /// <summary>
        /// Vuelve a colgar aqui un contenedor que salio de este inventario, en las celdas que
        /// ocupaba.
        ///
        /// Contraparte exacta de la retirada que hace ExtractFrom con una rama. Existe porque
        /// una rama no tiene unidades que devolver: su salida no se deshace sumando, se deshace
        /// volviendo a engancharla.
        /// </summary>
        /// <param name="pos">Celda que ocupaba, o None si no ocupaba ninguna.</param>
        /// <returns>False si ya colgaba de aqui, o si sus celdas ya no estan libres.</returns>
        public bool ReattachContainer(InventoryObject container, GridPos pos)
        {
            AC.CheckNotNull(container, nameof(container));

            if (_inventory.Contains(container)) return false;

            if (!pos.IsNone && !_grid.Place(container, pos)) return false;

            AddContainer(container);

            return true;
        }

        /// <summary>
        /// Saca este contenedor entero. Un contenedor no se divide: o sale o no sale.
        ///
        /// No toca la rejilla ni al padre, igual que el Extract de una hoja no se retira a si
        /// misma: retirar es asunto de quien contiene. Y su contenido viaja dentro de la
        /// entidad devuelta, en su InventoryComponent, asi que no hace falta moverlo aparte.
        /// </summary>
        public List<SubLot> Extract(ItemEntity variant, int amount)
        {
            if (amount <= 0 || GetAmount(variant) <= 0 || _item == null)
                return new List<SubLot>();

            return new List<SubLot> { new SubLot(_item, 1) };
        }

        /// <summary>
        /// Removes an empty node from this inventory and frees its grid cells.
        /// Targeted counterpart to CleanTree: use it when the emptied node is already
        /// known, instead of walking the whole tree to rediscover it.
        /// Only looks at direct children — a node inside a nested container must be
        /// cleaned by that container, which is the one owning the grid it sits on.
        /// </summary>
        /// <returns>True if the node was found and removed.</returns>
        public bool CleanNode(IInventoryElement node)
        {
            AC.CheckNotNull(node, nameof(node));

            if (!_inventory.Remove(node)) return false;

            _grid.Remove(node.GetNodeId());

            // Salir de la lista es tambien dejar de tener padre. Desde que un contenedor puede
            // retirarse por aqui —y no solo por RemoveContainer—, dejarle el padre puesto lo
            // vuelve un huerfano que dice pertenecer a un arbol en el que ya no esta: quien
            // luego intente colgarlo vera que "ya cuelga de ahi" y no hara nada.
            if (node is InventoryObject container) container.Parent = null;

            return true;
        }

        public bool Contains(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFind(id) != null;
        }

        public int GetAmount(int id)
        {
            AC.CheckNotNull(id, "id");
            int total = 0;
            foreach (IInventoryElement node in BfsFindAll(id))
                total += node.GetAmount();
            return total;
        }

        public int GetAmount(ItemEntity variant) => variant == null || ReferenceEquals(_item, variant) ? 1 : 0;

        public void DeleteItem(int id)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement node in BfsFindAll(id))
                node.DeleteItem(id);
            CleanTree();
        }

        public IInventoryElement Find(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFind(id);
        }

        public List<IInventoryElement> FindNodes(int id)
        {
            AC.CheckNotNull(id, "id");
            return BfsFindAll(id);
        }

        //#endregion

        //#region Local operations

        public int StackOntoHere(ItemEntity item, int amount)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");

            // Un contenedor no se apila sobre nada: se coloca como rama, y de eso ya se encarga
            // AddItem. Buscar una hoja equivalente lo convertiria en unidades de una pila.
            if (ContainerOf(item) != null) return AddItem(item, amount);

            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.IsLeaf() && elem.GetTypeId() == item.GetComponent<BaseItemComponent>().TypeId)
                {
                    amount = elem.StackOntoHere(item, amount);
                    break;
                }
            }
            if (amount > 0)
                amount = AddItem(item, amount);
            return amount;
        }

        public int ModifyAmountHere(int id, int amount)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
            {
                if (elem.GetTypeId().Equals(id))
                {
                    int modified = elem.ModifyAmount(id, amount);
                    CleanTree();
                    return modified;
                }
            }
            return 0;
        }

        public bool ContainsHere(int id)
        {
            AC.CheckNotNull(id, "id");
            return FindHere(id) != null;
        }

        public int GetAmountHere(int id)
        {
            AC.CheckNotNull(id, "id");
            int total = 0;
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetTypeId().Equals(id))
                    total += elem.GetAmount();
            return total;
        }

        public void DeleteItemHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> snapshot = new List<IInventoryElement>(_inventory);
            foreach (IInventoryElement elem in snapshot)
                if (elem.GetTypeId().Equals(id))
                    _inventory.Remove(elem);
        }

        public IInventoryElement FindHere(int id)
        {
            AC.CheckNotNull(id, "id");
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetTypeId().Equals(id)) return elem;
            return null;
        }

        public List<IInventoryElement> FindNodesHere(int id)
        {
            AC.CheckNotNull(id, "id");
            List<IInventoryElement> results = new List<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                if (elem.GetTypeId().Equals(id)) results.Add(elem);
            return results;
        }

        //#endregion

        //#region Node operations

        public int StackOntoNode(int nodeId, ItemEntity item, int amount)
        {
            IInventoryElement node = FindNodeById(nodeId);
            return node != null? node.StackOntoHere(item, amount) : amount;
        }

        public IInventoryElement FindNodeById(int nodeId)
        {
            return BfsFindByNodeId(nodeId);
        }

        /// <summary>
        /// Puts units at a grid position, stacking onto whatever is already there when
        /// compatible, and creating a node otherwise.
        ///
        /// <para>The stacking branch is what makes "drop onto an existing pile" work, and it
        /// also lets several variants of a mixed stack land on the same spot: the first call
        /// creates the node, the rest merge into it. Creating a node per call would make every
        /// call after the first fail against the cells the first one just took.</para>
        ///
        /// <para>An occupied cell holding a different typeId rejects everything and returns the
        /// full amount: BatchItem.AddAmount refuses items that are not its type, so "drop onto
        /// something unrelated does nothing" falls out without a special case.</para>
        ///
        /// <para>Stacking skips CanPlace on purpose — the node is already placed and does not
        /// grow. Only maxStackSize limits it.</para>
        /// </summary>
        /// <returns>Units that could not be added.</returns>
        public int AddItemAt(ItemEntity item, int amount, GridPos pos, int ignoreNodeId = -1)
        {
            AC.CheckNotNull(item, "item");
            AC.CheckPositive(amount, "amount");
            // Not CheckPositive: row 0 and column 0 are valid cells.
            if (!_grid.IsInside(pos))
                throw new ArgumentOutOfRangeException(
                    $"AddItemAt: cell {pos} is outside a {_grid.GetGridH()}x{_grid.GetGridW()} grid.");

            // Cualquier celda del nodo vale: soltar sobre el centro de una espada de 1x3
            // encuentra su nodo igual que soltar sobre su esquina de origen.
            //
            // El nodo ignorado no cuenta como ocupante: es el que se esta moviendo, sigue en
            // la grid porque nada sale de ella hasta soltar, y apilar sobre el devolveria las
            // unidades a su sitio original en vez de moverlas.
            GridElement occupant = _grid.GetElementAt(pos);
            if (occupant != null && occupant.GetNode().GetNodeId() != ignoreNodeId)
            {
                // Un contenedor no apila ni recibe apilados: es una cosa, no una pila de una.
                if (!occupant.GetNode().IsLeaf() || ContainerOf(item) != null) return amount;

                return occupant.GetNode().StackOntoHere(item, amount);
            }

            if (ContainerOf(item) != null)
                return PlaceContainer(item, node => _grid.Place(node, pos, ignoreNodeId) ? 0 : 1, amount);

            BaseItemComponent baseInfo = item.GetComponent<BaseItemComponent>();
            if (_grid.CanPlace(pos, baseInfo.DimensionH, baseInfo.DimensionW, ignoreNodeId))
            {
                ItemObject node = new ItemObject(item, amount);

                // Placing may still fail even after CanPlace said yes, so the result decides:
                // adding the node anyway would leave it inside the inventory but off the grid,
                // where nothing renders it and no cell points at it.
                if (!_grid.Place(node, pos, ignoreNodeId)) return amount;

                amount = amount - node.GetAmount();
                _inventory.Add(node);
            }

            return amount;
        }

        //#endregion

        //#region Getters & Utilities

        public void ClearInventory() => _inventory.Clear();

        public void CleanTree()
        {
            foreach (IInventoryElement elem in _inventory)
                elem.CleanTree();

            _inventory.RemoveAll(e =>
            {
                if (e.GetAmount() > 0) return false;

                // Sin condicion de hoja: Remove sobre un nodo que no ocupa celdas no hace nada,
                // y asi esta retirada y CleanNode no pueden divergir. Un contenedor no entra
                // aqui igualmente — su GetAmount es 1, porque vacio sigue siendo un contenedor.
                _grid.Remove(e.GetNodeId());

                if (e is InventoryObject container) container.Parent = null;

                return true;
            });
        }

        public List<IInventoryElement> FlattenInventory()
        {
            List<IInventoryElement> result = new List<IInventoryElement>();
            Queue<IInventoryElement> queue = new Queue<IInventoryElement>();
            foreach (IInventoryElement elem in _inventory)
                queue.Enqueue(elem);

            while (queue.Count > 0)
            {
                IInventoryElement current = queue.Dequeue();
                if (current.IsLeaf())
                    result.Add(current);
                else
                {
                    if (((InventoryObject)current)._inventory.Count > 0)
                        result.Add(current);
                    foreach (IInventoryElement child in ((InventoryObject)current).GetChildren())
                        queue.Enqueue(child);
                }
            }
            return result;
        }

        /// <summary>
        /// Peso de lo que hay aqui dentro.
        ///
        /// Un contenedor NO se cuenta a si mismo: su barra mide lo que le has metido, no lo que
        /// pesa el mueble. Pero un contenedor que va DENTRO de otro si pesa para el de fuera,
        /// asi que ese sumando lo pone el padre — una mochila vacia no es gratis de llevar,
        /// aunque para ella misma este vacia.
        ///
        /// <para>Este es el unico sitio que suma hijos. Si algun dia aparece otro recorrido que
        /// acumule pesos, tiene que acordarse del peso del contenedor o dejara de cuadrar.</para>
        /// </summary>
        public float GetTotalWeight()
        {
            float total = 0f;

            foreach (IInventoryElement elem in _inventory)
            { 
                total += elem.GetTotalWeight();

                if (elem is InventoryObject container && container.GetItemEntity() != null)
                    total += container.GetItemEntity().GetComponent<BaseItemComponent>().Weight;
            }

            return total;
        }
 

        /// <summary>
        /// Dos contenedores equivalen si son del mismo tipo y guardan lo mismo.
        ///
        /// <para><b>No se compara _item como entidad</b>, y no es un olvido: un contenedor y su
        /// entidad son la misma cosa vista por sus dos caras — la entidad lleva un
        /// InventoryComponent que apunta a este mismo objeto, asi que compararla entra otra vez
        /// aqui y no sale. Ademas seria redundante: _id SALE del item (su typeId) y el
        /// contenido se compara elemento a elemento justo debajo.</para>
        ///
        /// <para>Lo que si se pierde: el estado de instancia del contenedor (un arcon dañado y
        /// otro intacto salen equivalentes). Si algun dia importa, se compara aqui de forma
        /// explicita — nunca delegando en la entidad entera, que es por donde se cierra el
        /// ciclo.</para>
        /// </summary>
        public bool Equivalent(IInventoryElement other)
        {
            AC.CheckNotNull(other, "other");
            if (!(other is InventoryObject otherInv)) return false;

            bool generalCheck = this._id.Equals(other.GetTypeId())
                             && this.IsLeaf() == other.IsLeaf()
                             && (this._item == null) == (otherInv._item == null);
            if (!generalCheck) return false;
            if (this._inventory.Count != otherInv._inventory.Count) return false;

            List<IInventoryElement> remaining = new List<IInventoryElement>(otherInv._inventory);
            foreach (IInventoryElement elem in _inventory)
            {
                IInventoryElement match = null;
                foreach (IInventoryElement candidate in remaining)
                {
                    if (elem.Equivalent(candidate)) { match = candidate; break; }
                }
                if (match == null) return false;
                remaining.Remove(match);
            }
            return remaining.Count == 0;
        }

        /// <summary>
        /// Deep-clones the inventory, preserving each item's position in the grid.
        /// Cloning only the element list is not enough: the new InventoryObject starts
        /// with an empty TetrisGridState, so every placed node must be re-placed at its
        /// original coordinates or the clone would report its items as unplaced.
        /// </summary>
        public IInventoryElement Clone()
        {
            // Parent se queda en null a proposito: el clon es un arbol desprendido, y heredar
            // el padre del original lo haria pesar sobre un inventario al que no pertenece.
            InventoryObject clone = this._item != null
                ? new InventoryObject(this._item)
                : new InventoryObject(this._holder);

            // Un solo recorrido por la LISTA, consultando la rejilla para saber si cada hijo
            // ocupa celdas. Recorrer la rejilla y la lista por separado duplicaba lo que
            // estuviera en las dos — que desde que un contenedor puede colocarse, es cualquier
            // contenedor guardado.
            foreach (IInventoryElement elem in _inventory)
            {
                IInventoryElement childClone = elem.Clone();

                // Reapuntado al clon: sin esto seguiria preguntando el peso al arbol original,
                // y el clon pesaria sobre quien lleva el de verdad.
                if (childClone is InventoryObject container) container.Parent = clone;

                GridElement placed = _grid.GetElementOf(elem.GetNodeId());

                // Sin celdas: un contenedor equipado, que cuelga del arbol pero no ocupa sitio.
                if (placed != null && !clone._grid.Place(childClone, placed.GetPos()))
                    throw new InvalidOperationException(
                        $"InventoryObject.Clone: could not re-place node {elem.GetNodeId()} " +
                        $"at {placed.GetPos()} in the cloned grid.");

                clone._inventory.Add(childClone);
            }

            return clone;
        }

        public List<SubLot> ConsumeRandom(int amount)
        {
            throw new InvalidOperationException("AddContainer is not supported on leaf nodes."); 
        }

        //#endregion
    }
}