# Phase 1 — Inventory: Implementation Milestones

## Where we are right now

> Esta seccion existe para el relevo entre conversaciones: reescribirla al cerrar cada tarea.

**Milestone 5 cerrado. M6 a medias: hechas T1, T3, T4 y T7; T2 a medio camino.** Quedan T5
(cerrar por distancia) y T6 (carros y NPCs). Se cerro ademas M7 T3b por el camino: un
contenedor guardado ocupa celdas y pesa igual que puesto.

**Lo siguiente, concreto: falta el cableado y falta `Unlink`.** Tirar funciona de punta a
punta —el item sale del inventario, aparece un monton en el mundo con su modelo 3D— y existe
la consulta que decide a que puedes llegar (`WorldInteractionSystem.FindTarget`). **Pero no
la llama nadie**, asi que nada de eso se ejecuta todavia. Para cerrar T2 hacen falta tres
piezas, y la segunda arrastra un agujero de arquitectura:

1. **Input.** Una fuente de interaccion por estrategia de camara, hermana de
   `IInventoryInputSource`, solo en FPS y TPS —el RTS se queda sin interaccion—. Algo por
   fotograma pregunta `FindTarget` y guarda el objetivo. Pulsacion corta = accion por
   defecto; mantener = menu radial.
2. ~~**Ejecutar `WorldAction.PickUp`**~~ **HECHO** en `WorldInteractionService.PickUp`
   (decisiones en la seccion de M6). Texto original: moviendo los lotes al inventario con lo que ya sabe
   hacer `InventorySystem`. Decidido: si no cabe todo se mueve lo que quepa y se avisa
   ("peso completo" / "demasiado volumen"); si no cabe nada, **la accion se ofrece igual** y
   mueve cero con su aviso, porque una E que no aparece no le explica al jugador por que.
   Decidido tambien que la composicion la haga **un servicio nuevo**, no el sistema tirando
   del `SystemManager`: mismo camino que con el equipo.
3. ~~**`Unlink`, que no existe.**~~ **HECHO**: `IEntityLinker.Unlink` y
   `WorldInteractionSystem.Despawn`, sin llamadas aun; la primera sera un monton que se
   vacia al recogerlo. Decisiones en la seccion de M6.

**Lo que funciona hoy.** Mover items dentro de la rejilla y entre paneles, por clic-agarre y
por arrastre indistintamente, con el fantasma coloreado segun un veredicto que recorre las
mismas decisiones que la colocacion real, imantado tanto a los slots de equipo como celda a
celda. Menu contextual con submenus. Equipar y desequipar por los tres caminos —menu, clic y
arrastre— incluyendo capas concretas. Desglose de variantes en su propio desplegable, cada
fila agarrable y con su menu. Cantidades parciales por gesto (shift) y por menu, e
intercambio de dos items cuando ninguno admite al otro. El fantasma distingue cuatro
respuestas: verde entra entero, amarillo entra parte, azul se intercambia, rojo nada.

**Y contenedores de verdad.** Una mochila equipada aparece como pestaña del panel del jugador
y se opera como cualquier inventario; su menu la manda a los huecos laterales. Su peso y el de
su contenido cuelgan del arbol del portador, y la capacidad se pregunta hacia arriba nivel a
nivel —bolsillo, mochila, personaje—, con aviso en la barra cuando el que frena no es el que
miras. Un contenedor no puede meterse dentro de si mismo ni de lo que lleva dentro.

**Y despues de T2**, lo que queda de M6 ya no es inventario-como-interfaz:

- **T6, carros y NPCs.** Casi solo datos, y su valor real es que pone a prueba T1, T3 y T4 con
  contenido de verdad en vez de dos arcones de prueba.
- **T5, cerrar el panel por distancia.** Pequeña, y ahora barata: la comprobacion de distancia
  ya existe en `WorldInteractionSystem`.

**Pendiente tecnico de M6:** limpiar `IInventoryElement`. Conviven las operaciones reales del
Composite —`Extract`, `GetAmount(variant)`, `HasVariants`— con los restos del diseño BFS
anterior: `StackOntoHere`, `ModifyAmountHere`, `ContainsHere`, `GetAmountHere`,
`DeleteItemHere`, `FindHere`, `FindNodesHere`, `SetAmount` y los metodos de hoja que lanzan.

**Pendiente de datos:** falta en Stack&Go una segunda prenda de **capa exterior** para el
pecho de **categoria distinta** a `Plate` —una capa o tunica de categoria `Robe` con
`topLayer: true`—, que es lo unico que permite alcanzar `TopLayerBlocked`. Dos pecheras no
sirven: chocan antes en `DuplicateCategory`, porque `EquipmentSlot.CanEquip` comprueba la
categoria antes que la capa. El `Insert(Count - 1)` en cambio ya es probable con el catalogo
actual: Pechera es `topLayer` y Camisa no, asi que equipar la camisa con la pechera puesta la
mete debajo.

**Deuda conocida que no bloquea:** ni el popup de capas ni el de variantes son destino de
soltado (se agarra desde ellos, no se suelta en una capa o variante concreta), y en el de
variantes la asimetria chirria mas, porque el sitio del que sacas una manzana parece
obviamente un sitio donde devolverla; el menu de una fila del desplegable no ofrece "Dividir"
aunque `SplitNode` acepte variante; `EquipmentSystem` sigue sin reaccionar a eventos pese a
implementar `IReactiveSystem`; `GetAvailableActions` acumula flags sueltos (`hasVariants`,
`splittable`), que es el mismo olor que `MenuContext` vino a arreglar un nivel mas arriba; la
ropa puesta aun no pesa —solo lo que lleva dentro un contenedor equipado—, asi que equipar
desde un arcon sigue metiendo peso gratis, decision ya tomada (opcion "el equipo pesa, con
coeficiente") y pendiente de aplicar; `UpdateInventoryTabs` recorre las capas de todos los
slots, asi que un contenedor de ocupacion completa saldria con una pestaña por slot; y
`ClearInveotryTabs` lleva una errata en su nombre.

---

## Architecture decisions (locked)

- **Item IDs**: typeId (int) from JSON catalog (Stack&Go). Instances tracked via sub-lots.
- **Stacking**: By equivalence. Visual stack with internal sub-lots `List<(ItemEntity, int)>`. Random consumption, inspect to manage individually.
- **Tetris**: Grid IS the capacity system (not just visual). Few grid sizes (1×1, 1×2, 2×1, 2×2, 2×3...) + per-item maxStackSize replaces float volume. StorageComponent defines gridW × gridH instead of maxVolume. Weight remains as float.
- **Equipment layers**: Ordered outside→in (mechanical, for Phase 2 damage). Slots disableable (amputation).
- **Layout**: Split fixed. Left: personal (health placeholder + equipment). Right: inventory with tetris view. Bottom: item inspection strip (always visible when inventory is open). Layout: left = item icon (large), center-left = description, center-right = stats/details (condition, weight, durability, etc.). Works across all panel configs (single inventory, inventory + container, container-to-container).
- **External containers**: Opens as second panel alongside your inventory.
- **Equip UX**: Drag & drop between zones + right-click context menu.
- **Grid state**: Core (persisted with save). Organization reward is intrinsic — better packing = more items fit.
- **Item catalog**: JSON from Stack&Go, loaded at startup.

---

## Invariantes del inventario

Reglas que el codigo da por ciertas y que no se deducen leyendo una clase suelta. Romper
cualquiera de ellas no da un error de compilacion: da un fallo silencioso en otro sitio.

**La mano es una reserva, no una posesion.** Agarrar no mueve nada: `HandBuffer` apunta de
que origen sale y cuantas unidades quedan reservadas, y las unidades siguen en su nodo. La
extraccion y su vuelta atras ocurren dentro de la misma llamada a `InventoryService.RunTransfer`,
asi que al terminar esa llamada o lo movido esta en el destino o ha vuelto a su sitio. De ahi
que cancelar sea gratis y que `HandBuffer.Clear` pueda soltar la referencia sin devolver nada.
El dia que algun camino extraiga al AGARRAR en vez de al colocar, esa garantia desaparece.

**Durante un `InventoryChanged`, la mano puede ir un paso por detras.** `RunTransfer` anuncia
al cerrar su transaccion, pero `PlaceFromHand` llama a `NotifyPlaced` despues. En esa ventana
el HandBuffer conserva su origen —`IsHandCarrying()` responde que si— mientras el nodo del que
reservaba ya se vacio, de modo que `GetHeldItem()` puede devolver null. Quien lea la mano desde
un observador de ese evento tiene que tolerarlo: se pregunta por el item, no por si hay mano.

**La consulta y la ejecucion recorren las mismas decisiones y en el mismo orden.** `EquipItem`
arranca llamando a `CanEquip`, `EquipFromHand` a `EvaluateEquip`, `SwapFromHand` a
`CanSwapWith`, y `PlaceAt` decide con el mismo `EvaluatePlacement` que pinta el fantasma. Las
cuatro veces que este proyecto ha pintado verde sin mover nada han sido la misma causa:
alguien pregunto una parte de la decision en vez de la decision entera, o la pregunto en otro
orden.

**El arbol refleja contencion; la rejilla refleja colocacion.** Un contenedor guardado es hijo
en el arbol Y ocupa celdas; uno equipado es hijo sin celdas. De ahi sale que la UI no necesite
ninguna bandera para saber si pintarlo: tiene celdas o no las tiene.

**Salir de la lista de un inventario es dejar de tener padre.** `AddContainer` lo pone;
`CleanNode`, `CleanTree` y `RemoveContainer` lo quitan. Un contenedor con padre obsoleto dice
pertenecer a un arbol en el que ya no esta, y quien luego intente colgarlo vera que "ya cuelga
de ahi" y no hara nada.

**Un techo de peso no aplica a algo que ya esta debajo de el.** La regla vive en
`InventoryObject.OwnFreeWeight`, y `FreeWeight` es el unico recorrido hacia arriba del sistema
de peso: `FitByWeight` y `CarrierBlocks` se derivan de el. Mover del bolsillo a la mochila o de
la mochila al jugador no cambia lo que el jugador carga.

**Recorrer los slots de equipo no es lo mismo que recorrer las prendas puestas.** Una prenda de
ocupacion completa esta registrada en todos sus slots a la vez. Para contar —peso, pestañas,
inventarios equipados— se usa `EquipmentComponent.EquippedItems()`; el recorrido por slots solo
sirve cuando lo que importa es el slot en si.

---

## Milestone 1 — Item catalog & numeric IDs

**Goal**: Replace string-based item identification with numeric typeIds.

**Tasks**:

- [x] 1. ~~Create `ItemDefinition` class~~ — replaced by prototype approach. `ItemCatalog` stores fully assembled `ItemEntity` prototypes (with all their components). Creating a new item = `prototype.Clone()`. No separate definition class needed.
- [x] 2. Create `ItemCatalogue` class in Core/Item/ — `Dictionary<int, ItemEntity>` of prototypes indexed by typeId. JSON loader creates fully assembled `ItemEntity` prototypes (with BaseItemComponent + any other components) and registers them. When the game needs a new item, `catalog.CreateItem(typeId)` clones the prototype.
- [x] 3. Define JSON schema: Stack&Go exports `data.json` with items and their components (BaseItem, Material, Damage, etc.). Each component's `values` map to the corresponding ECS component fields. Recipes are exported too but ignored in Phase 1.
- [x] 4. Create `JsonItemCatalogLoader` in Core/Item/. The loader reads `data.json` (path from `CoreConfig.CatalogPath`), creates `ItemEntity` prototypes with their components via `IJsonLoadable.SetFromValues()`, and registers them in `ItemCatalogue`. File paths resolved via `CoreConfig` static class (replaces the original bridge interface approach).
- [x] 5. Persistent typeId assignment: `TypeIdMapper` class maintains a `name → typeId` mapping persisted as `id_mapping.json` (path from `CoreConfig.MappingPath`). On load: existing names keep their typeId, new names get `max(existing) + 1`, deleted item IDs are never reused. File I/O via `System.IO` + `CoreConfig` static paths (replaces the original bridge interface approach).
- [x] 6. Refactor `BaseItemComponent` — remove `_volume` field (grid replaces it), add `_maxStackSize` (int), keep `_typeId` (already added). Fields remaining: typeId, weight, dimensions, durability, maxDurability, condition, maxCondition, description, iconPath, maxStackSize. All cloned per-instance from prototype.
- [x] 7. Item typeId lives in `BaseItemComponent._typeId` (not in `ItemEntity` directly). The loader assigns it via `TypeIdMapper.GetOrAssignId(name)` and sets it with `BaseItemComponent.SetTypeId()`. `ItemEntity` constructor auto-assigns `"ItemEntity"` as its entity type — no longer receives a type string parameter.
- [x] 8. Refactor `IInventoryElement` — `GetId()` returns int (typeId) instead of string name. Remove `GetTotalVolume()` from interface and all implementations (volume replaced by grid).
- [x] 9. Update `InventoryObject` BFS methods to use typeId
- [x] 10. Update `ItemObject` accordingly (adapt to int-based IDs, remove volume references — eases BatchItem transition in M2)
- [x] 11. Update `PrototypeFactory` to create items from catalog
- [x] 12. Create test JSON with sample items (6 items: Espada de hierro, Arco corto, Manzana, Odre, Venda, Arcón pequeño — exported from Stack&Go, placed in StreamingAssets/)
- [x] 13. Delete obsolete `ItemDatabase`, `ConcreteItemBuilder`, and `IItemBuilder` (replaced by `ItemCatalogue` + prototype pattern)
- [x] 14. Fix `InventoryView.OnItemClicked` event signature from `Action<string>` to `Action<int>`, update `ItemDisplayData.Id` to int

**Decided**: No separate `ItemDefinition` class. `ItemCatalog` stores `ItemEntity` prototypes with all their components pre-assembled. Creating a new item = `catalog.CreateItem(typeId)` which clones the prototype. Data is duplicated per instance (acceptable trade-off for simplicity).

---

## Milestone 2 — Sub-lot stacking

**Goal**: Items stack visually but maintain internal sub-lots for different states.

**Tasks**:

- [x] 1. Create `BatchItem` as internal data structure for `ItemObject`. `ItemObject` keeps its role as composite leaf (implements `IInventoryElement`) but replaces its `_item` + `_amount` fields with a `BatchItem` that holds `List<(ItemEntity, int)>` sub-lots. `BatchItem` owns a `typeId` (set from the first entity added, immutable). `ItemObject.GetId()` delegates to `BatchItem.TypeId`. `ItemObject` is NOT deleted — it remains the composite leaf wrapper.
- [x] 2. Add unique `nodeId` (int, autoincremental via `NodeIdGenerator`) to both `ItemObject` and `InventoryObject`, separate from `typeId`. `GetNodeId()` added to `IInventoryElement`. Needed for grid positioning (M3) and UI selection (M5). Search-by-nodeId operations deferred — UI elements will hold direct references to nodes instead of searching by ID. NodeId search infrastructure will be added later only if a real use case appears (serialization, networking, undo).
- [x] 3. Equivalence-based grouping: items with same typeId live in the same BatchItem, sub-lots split by property differences (durability, condition, enchants)
- [x] 4. Item addition and stacking logic: `StackOnto` finds first compatible BatchItem by typeId (BFS) and delegates to `BatchItem.AddAmount()`; overflow creates new nodes via `AddItem`. `AddItem` always creates leaf nodes (ItemObject), looping if amount exceeds maxStackSize. `AddContainer` creates branch nodes (InventoryObject), separated from item logic. `BatchItem.AddAmount()` merges into existing sub-lot if Equivalent, creates new sub-lot if different state, respects maxStackSize cap. Multiple ItemObjects of the same typeId are allowed (split stacks, overflow).
- [x] 5. `BatchItem.ConsumeRandom()` consumes 1 unit from a random sub-lot. `ConsumeAmount(item, n)` consumes from a specific sub-lot matched by Equivalent. `ConsumeAll()` clears the batch.
- [x] 6. `BatchItem.GetSubLots()` returns a copy of the sub-lot list for UI inspection
- [x] 7. `BatchItem.GetTotalAmount()` sums amounts across all sub-lots
- [x] 8. `BatchItem.GetTotalWeight()` sums (weight × amount) per sub-lot

**Decided**: Unified leaf node — no distinction between "single item" and "stack". Everything is a `BatchItem`. Eliminates special cases in composite tree logic.

**Decided**: Pulling items out of a stack via inspect creates a new BatchItem in the same inventory. It's a real data operation, not just visual.

**Tasks (pending)**:

- [x] 9. Add `StackOnToNode(int nodeId, ItemEntity item, int amount)` to composite (IInventoryElement + implementations). Finds node by nodeId, calls `node.StackOntoHere()`. No fallback. Returns remaining. Also add `AddItemAt(ItemEntity item, int amount, int row, int col)` only on InventoryObject (not in interface — grid coordinates are branch-specific). Creates one node at specified cell, no loop. Returns remaining. Refactor: `ItemObject.StackOntoHere` now delegates to `_batch.AddAmount()` instead of throwing — allows `StackOnto`, `StackOntoHere` and `StackOntoNode` in InventoryObject to use the interface without explicit casts.

---

## Milestone 3 — Weight & grid space enforcement

**Goal**: Inventory operations (add, stack, transfer) check grid space and weight limits, rejecting or penalizing when exceeded.

**Tasks**:

- [x] 1. Refactor `StorageComponent` — replaced `maxVolume` (float) with `gridW` and `gridH` (int). Removed `weightRatio` (grid gives containers their mechanical advantage, no need for weight multiplier). `maxWeight` stays as float.
- [x] 2. Create `TetrisGridState` in Core — 2D int matrix (nodeId per cell, -1 if free) + list of `GridElement` (ItemObject reference + row/col position). `CanPlace`, `Place`, `Remove`, `FindFirstFit`, `GetFreeCellCount`. `GridElement` in separate file. Core only, no UI — rendering is M5.
- [x] 3. Grid + weight enforcement on inventory operations. Grid checks in composite (`AddItem`, `StackOnto`, `StackOntoHere`, `StackOntoNode`, `AddItemAt` use TetrisGridState). Weight checks in `InventorySystem` wrapping composite calls: `TryStackOntoHere` (stacks first, overflow creates new nodes — covers all automatic add cases), `TryStackOnToNode`, `TryAddItemAt`. All Try methods share `GetFitByWeight` (weight first) → delegate to composite (grid) → return remaining. `TryAddItem` removed (redundant with `TryStackOntoHere`). `InventoryComponent.Inventory` changed from `IInventoryElement` to `InventoryObject` — root is always a branch node. `ConsumeRandom(int amount)` added to `IInventoryElement` — leaf delegates to batch, branch throws. Extraction orchestration deferred to `InventoryService` (M6). `CleanTree` now also removes deleted nodes from grid via `_grid.Remove(nodeId)`.
- [x] 4. `InventorySystem` fires events: `INVENTORY_FULL`, `EXTRA_WEIGHT`, `OVERWEIGHT`, `IMMOBILE`. Fired from `EvaluateAndFireEvents(entity, fullGrid)` called in each Try method. Weight thresholds: EXTRA_WEIGHT (0.70), OVERWEIGHT (0.85), IMMOBILE (1.0). Used by: UI (HUD indicators), movement system (speed reduction), AI (NPCs stop picking up items).
- [x] 5. Weight debuff integration: `MovementSystem` listens to weight events and sets `_weightSpeedMultiplier` on `MovementComponent` (EXTRA_WEIGHT=0.80, OVERWEIGHT=0.50, IMMOBILE=0.0). Run restriction via `AddRunRestriction`/`RemoveRunRestriction` semaphore pattern on OVERWEIGHT/IMMOBILE. `FatigueStaminaSystem` migrated to same pattern. Stub health effects for Phase 2. Normal weight restoration deferred to InventoryService (M6) — triggered when items are consumed/removed.
**Decided**: Two capacity systems. Grid space is a hard limit: no free cells that fit → transfer rejected; partial if stackable and existing BatchItem has room under maxStackSize. Weight has two thresholds — soft (transfer allowed but debuff: movement speed reduction, notify overloaded, health consequences stub for Phase 2) and hard (immobile, transfer rejected entirely). Health consequences of overload are stubbed as interface for Phase 2.

---

## Milestone 4 — Equipment system overhaul

**Goal**: Equipment panel with ordered layers, drag & drop, slot disabling.

**Tasks**:

- [x] 1. `EquipmentSlot` — List<ItemEntity> ordered by layer (last = outermost). `Add` for equip on top, `RemoveAt(Count-1)` to remove outer layer, iterate backwards for damage.
- [x] 2. Add `enabled` flag to `EquipmentSlot` (default true, false = incapacitated — missing limb, broken bone, severe injury) and `maxLayers` int (hard cap on stacked items per slot). `EquipItem` guarded by `_enabled`. Renamed `maxAmount` → `maxLayers`.
- [x] 3. Create `WearableComponent` in Core/ItemComponents — determines which `EquipmentSlotType` a wearable item targets. An item is equippable if and only if it has this component (ECS-idiomatic: `HasComponent<WearableComponent>()`). Fields: targetSlot (EquipmentSlotType), topLayer (bool — if true, nothing can be equipped on top of this item in that slot), garmentCategory (enum: Shirt, Vest, Plate, Robe, Glove, Boot, Helmet, Hood, Satchel... — extensible).
- [x] 4. Layer validation in `EquipItem`: checks enabled, maxLayers, targetSlot, duplicate garmentCategory, topLayer. Returns `EquipResult` enum (Success, SlotDisabled, MaxLayersReached, WrongSlot, DuplicateCategory, NotWearable, TopLayerBlocked). TopLocked items inserted below topLayer via `Insert(Count-1)`.
- [x] 5. Add `UnequipItem` to `EquipmentSlot` (returns bool via `List.Remove`) and `EquipmentComponent` (delegates to slot, throws `InvalidOperationException` on absent WearableComponent or item not found). Removes from any layer position (consistent with equip-below-topLayer rule).
- [x] 6. Create `EquipmentSystem` (new system, SRP — separate from InventorySystem). `TryEquip` and `TryUnequip` return `EquipResult`, log via `EquipResult.GetMessage()` extension method, and fire `EquipmentChanged` event on success.
*Tasks 7–10 (equipment UI) moved to M5 — all UI work consolidated there.*

**Decided**: Equipment layout is cross + side column (not a grid). Layout:
```
Cross (body):              Side column:
       [Head]              [Back]  (multi-slot: 2 shoulder bags / 1 backpack)
[LHand][Chest][RHand]      [Hip]   (tool belt, sword sheath, pouch)
       [Legs]
       [Feet]
```
`Hands` enum removed — gloves are a wearable layer on LeftHand/RightHand (garmentCategory: Glove). Equipping gloves applies to both hands; unequipping from either hand removes both. Weapons/tools are a separate layer (garmentCategory: Weapon/Tool) and are per-hand. Face/Neck items (mask, goggles, scarf, necklace) are layers on Head slot with their own garmentCategory.

**Decided**: Layer order is validated. Two rules:
1. **topLayer** (component data): `WearableComponent` has a `topLayer` bool. Rigid/structured items (armor, chestplate) are topLayer — nothing can be equipped on top of them. Covers: no shirt over chestplate, no armor over armor.
2. **No duplicate garment category** (system logic, NOT component data): `InventorySystem` enforces that you can't equip two items with the same `garmentCategory` in the same slot. Iron plate armor and studded leather armor are both `Plate` → can't stack. A shirt and a camisole are both `Shirt` → can't stack. One Shirt + one Vest + one Plate = OK. System-level validation, can be relaxed in the future if needed.
- `maxLayers` on `EquipmentSlot` remains as a hard safety cap.
- Equip order: new items always go on top (outermost). To change order, unequip and re-equip.

---

## Milestone 5 — Tetris grid UI & split view

**Goal**: Inventory displayed as tetris grid where grid IS the capacity system. Split layout with personal panel.

**Tasks**:

Tooling:
- [x] 0. **UI live-reload support (dev iteration speed).** UI Toolkit's Live Reload is enabled (Game view ⋮ menu) but editing a UXML/USS while playing logs `UI was recreated and no companion MonoBehaviour found, some UI functionality may have been lost`: Unity rebuilds the visual tree and then looks for a MonoBehaviour on the `UIDocument`'s GameObject to notify, so it can re-acquire element references. There is none — `InventoryView.Initialize()` runs once from the `InventoryPresenter` constructor, itself called from `GameMain.Awake()`. After a reload every cached reference (`_root`, `_itemGrid`, `_equipmentSlots`, `_leftTabs`, `_leftPanels`, `_subSlotsPopUp`, `_itemsLayer`) points at orphaned elements, so the panel looks reloaded but is dead. Note this is editor-only: C# changes can never hot-swap into a running process, and that is a separate problem (see Future: disabling Domain Reload requires auditing the static state in `NodeIdGenerator`, `TextureCache`, `EventBus`, `CoreLogger`, `CoreConfig`).

  How it was solved:
  1. `UIReloadNotifier : MonoBehaviour` (Unity/MVC/View/), attached in the scene to the same GameObject as the inventory `UIDocument`. Its whole body is `private void OnEnable() => OnUIRecreated?.Invoke();` over a `public static event Action OnUIRecreated`. The event is static because nothing creates the instance — Unity does, from the scene — so there is no natural reference to subscribe to. `GameMain.OnDestroy` unsubscribes: whoever subscribes, unsubscribes.
  2. The view/presenter wiring moved out of `Awake` into `GameMain.BuildViewsAndPresenters()`, called from both `Awake` and the reload event — one code path, no editor-only variant that could drift from the real one.
  3. **`GameContext` revived** instead of adding loose fields to `GameMain`: `presenterManager` already lived inside `GameSystemContext`, so the handler reaches it via `_gameContext.System.PresenterManager`. Dropped `CameraRegister` and `ViewManager` from `GameContext` — both self-instantiated and would have gone out of sync with the ones `Awake` builds. `viewManager` stays a local inside the build method, since each rebuild wants fresh views anyway.
  4. `PresenterManager.ReplacePresenter` added. `RegisterPresenter` silently ignores an existing key (first registration wins, which protects against accidental duplicates), so a rebuild would have been a no-op and `InputManager` would have kept driving the dead view — failing silently.
  5. Double-init guard by frame number (`_lastRebuildFrame == Time.frameCount`). Awake/OnEnable ordering across GameObjects is undefined, so if `GameMain` ran first the notifier's startup `OnEnable` would build a *second* view over the same VisualElements — duplicate click/drag handlers and two micro-buttons per equipment slot. A frame counter separates "startup, same frame" from "genuine reload, later frame".
  6. Reopens via `IPresenter.IsOpen()` + `Open(_gameContext.Session.Player)`. Required giving `IPresenter` an actual contract (`Open`/`Close`/`IsOpen`/`Refresh`) — it was an empty marker interface.
  7. Verified: editing USS while playing with the inventory open applies the change, keeps the window open, and leaves tabs, sub-slot popup and close button working. No more `no companion MonoBehaviour` warning.

  Cleanup done alongside: removed the dead `Logic` wrapper instantiation from `Awake` (superseded by `GameDataContext`), and the orphaned `OnItemClicked` event (its only emitter was the deleted card-based `RenderItems`; it will come back in task 14 with a signature that identifies a *stack*, not a typeId).

Foundation:
- [x] 1. Render inventory grid in UI based on player's TetrisGridState dimensions.
- [x] 2. Split view layout: left panel (personal) + right panel (inventory). Replaces current tab-based UI.

Left panel (personal — tabbed):
- [x] 3. Tab system in left panel to switch between Health and Equipment views (inventory panel stays fixed)
- [x] 4. Health tab: placeholder (future Zomboid-style health UI)
- [x] 5. Equipment tab: render cross + side column layout with slot VisualElements (from M4). Slot textures resolved by convention from the UXML element name (`slot-head` → `EquipmentSlotType.Head` via `Enum.TryParse`). Three visual states per slot: disabled, empty (`images/slots/<name>.png`) and equipped (top layer's `iconPath`). Runtime textures loaded and cached by `TextureCache` (Unity layer, reads from StreamingAssets); fixed UI art assigned via USS `url()`.
- [x] 6. Equipment tab: click slot to see/manage layers (from M4). **Viewing done**: micro-button rendered in each slot corner (`position: absolute`), visible only when the slot holds more than one layer. Toggling it opens/closes a single shared popup (last child of `main-area`, so it draws above everything) anchored to the slot's top-right corner via `worldBound` + `WorldToLocal`. Popup content rebuilt on every open (no caching). Sub-slots ordered outermost→innermost, skipping the layer already shown in the main slot. Click outside closes it (`ClickEvent` bubbling to root + `StopPropagation` on button and popup). **Pending**: managing layers (equip/unequip from sub-slots) — depends on drag & drop, tasks 10-13.

Right panel (inventory):
- [x] 7. UI: render grid with item blocks sized by dimensions (w×h from BaseItemComponent). Blocks live in an `items-layer` created by `GenerateGrid` as the last child of the generated `inventory-grid` (so it matches the grid's exact size, unlike the outer `item-grid` container which stretches). Each block is `position: absolute` and sized/placed in **percentages** (`col * 100 / gridW`, etc.) instead of pixels — no cell-size constant duplicated between USS and C#, and the layout survives any change to `.inventory-grid-cell`. Data flows as `GridItemDisplayData` (composes `ItemDisplayData` + row/col) built from `TetrisGridState.GetElements()`.
- [x] 8. UI: grid is fixed size (gridW × gridH), not scrollable — what you see is what you have. `ScrollView` removed: grid dimensions are designed to fit, so a scroller would be a patch for a problem deliberately avoided — and it would fight the pointer-drag gestures coming in task 10, since `ScrollView` captures drags to pan its content. Without it, a grid that doesn't fit overflows visibly instead of hiding the problem. The old `item-grid` wrapper survives (renamed `grid-mount`): it is the mount point `GenerateGrid` can `Clear()` without destroying its `stats-bar` sibling, and it carries the `flex-grow: 1` + `align-items: center` that place the fixed-size grid top-centre in the panel. `SetInternallGrid` renamed to `MountGrid` and made private — only `GenerateGrid` ever called it.
- [x] 9. Weight bar above the inventory grid — colour-coded by threshold. Rail (fixed height, `flex-shrink: 0`) with the fill as an absolutely-positioned child whose `width` is `Length.Percent(ratio * 100)`, clamped to 100 for painting but **not** for classifying. The label is a sibling of the fill, not a child: inside it, the text spilled out of the panel whenever the fill was narrower than the text — worse the emptier the inventory.

  Thresholds and classification live in `CarryCapacity` (`EXTRA_WEIGHT` / `OVERWEIGHT` / `IMMOBILE` + `ClassifyLoad`), a pure function returning the matching `GameEventType` — those values already are the vocabulary for these bands, so a parallel enum would only need keeping in sync. Single source of truth: `InventorySystem` uses it to pick which event to post (collapsing a 20-line if/else into three, with the per-band logging split out into `LogLoad`), and `InventoryPresenter` uses it to tell the view which band to paint. Being pure it can be called on demand when opening the inventory, where no event has fired — so **the presenter does not subscribe to the EventBus yet**: `Refresh()` recomputes everything from the model, and nothing changes weight mid-session until task 10. The view receives the band already decided and only maps it to a USS class, so no domain vocabulary leaks into it.

  Colours live in USS (`.load-normal` / `.load-extra` / `.load-over` / `.load-immobile`) rather than `style.backgroundColor`, so palette tweaking benefits from the live reload built in task 0 instead of needing a recompile. Swapping is table-driven from a `Dictionary<GameEventType, string>`: remove all four, add the one — adding a fifth band is one entry, not five edited branches.

  Prerequisite discovered while doing this: **the whole event subsystem was disconnected.** `EventBus.Subscribe` was never called anywhere, and neither `InventorySystem` nor `MovementSystem` was ever instantiated, so no weight event had ever fired and the movement debuff never applied. Fixed by splitting `IGameSystem` into `IPeriodicSystem` (has `Process`, driven by tick or frame) and `IReactiveSystem` (declares `SubscribedEvents`, driven by the bus), and making `SystemManager.RegisterReactiveGameSystem` subscribe on registration — registering *is* subscribing, so a reactive system can no longer end up alive but deaf. Also added the missing `else` posting `NormalWeight` (`MovementSystem` already handled it), without which returning below 0.70 never notified anyone.

Interaction:
- [x] 10. UI: move items within the grid to reorganize (mechanical impact — frees space for new items). Primary interaction is **click-to-grab / click-to-place** (see Decided note below); drag & drop is supported as a secondary gesture over the same "held item" state. Build alongside it a **dev creative panel**: search field + filtered list over `_itemCatalogue.GetAll()` (name + icon), amount field, click to `AddItem` + `Refresh`. Uncategorised for now. Needed to exercise the placement edge cases (full grid, no fit, stacking onto an existing lot, moving a 1x3 into a 1x2 gap) without editing `PrototypeFactory.AddTestItems` and restarting.

  **Decided — where `HandBuffer` lives.** Not in `InventoryPresenter` (interaction state, not game state, and presenters are rebuilt on live reload — the hand must survive that). Not in `PresenterManager` (that is a registry; giving it state would add a second reason to change). Not in an ECS component either (nothing systemic consumes it, it is never serialised, and it holds a reference to *whichever* `InventoryObject` is being manipulated — a chest, a corpse — so it is not player-simulation state). It goes in a new **`GameInteractionContext`** — see Future section.

  **Self-collision on move.** Since the hand moves nothing until placement, the source node's cells stay occupied, so nudging a node onto a position overlapping itself would fail against itself. `TetrisGridState.CanPlace`/`FindFirstFit` take `ignoreNodeId` (cells holding that id count as free) and `Place` calls `Remove` before writing, which covers it — but the decision belongs at the placement call, not in the grid: only the hand knows the source node (`GetSourceNode()`) and whether this placement empties it. Pass `ignoreNodeId = <source nodeId>` **only when the units leaving now empty the source node**, `-1` otherwise. Note the condition is *not* "grabbed == node total": with a node of 20 and `maxStackSize` 10, placing onto an empty cell moves only 10, the source survives with 10, and its cells are legitimately occupied. Compare against the amount that will actually move.
  **Pendiente — feedback de validez al colocar.** Las clases `.hand-buffer-collision` y
  `.hand-buffer-fits` ya existen en el USS (rojo de `.load-immobile`, verde de `.load-normal`)
  pero nadie las aplica. Falta evaluar el destino en cada `PointerMove` y pintar la mano segun
  el veredicto. Requisitos: consulta pura y barata (dispara en cada frame con movimiento) y
  **mismo camino de validacion que la colocacion real**, o el fantasma se pinta verde y al
  soltar falla. Un `Evaluate(...)` interno en `InventoryService`, con dos entradas publicas:
  una que pregunta y otra que ejecuta. El veredicto vuelve como enum de dominio (`Valid`,
  `WouldStack`, `Blocked`...), nunca como color: el presenter lo mapea a clase USS, igual que
  ya se hace con `CarryCapacity.ClassifyLoad` y las bandas de peso.

  **~~Bug — bloque fantasma tras una colocacion invalida.~~ RESUELTO.** Al soltar en sitio
  invalido la mano se cancelaba pero el bloque de origen se quedaba con `item-block-grabbed`
  puesto: parecia agarrado sin estarlo. Era un fallo de repintado, no de dominio — `IsGrabbed`
  se calcula al construir los DTOs, y el nodo atenuado puede estar en un panel distinto de
  aquel donde se solto.
  Resuelto porque toda cancelacion pasa por `InventoryPanelPresenter.OnHandChanged`, que
  `InventoryPresenter` engancha a `HandChanged`, y ese repinta **los tres paneles**, no solo
  el que origino el gesto. Verificado en juego.

- [x] 11. UI: move items from inventory → equipment slot, and from equipment sub-slots back to inventory (from M4). Unblocks the pending half of task 6.
- [x] 12. First-fit auto-place algorithm (for right-click pickup / quick-store): scan grid left-to-right, top-to-bottom, place in first valid position. Used as fallback, not primary flow.
- [x] 13. Right-click context menu on inventory items: [Equip] [Consume] [Drop] [Inspect] (from M4)

Polish:
- [x] 14. Item inspection strip (bottom, full width): left = large item icon, center-left = name + description, center-right = stats (condition, weight, durability, grid size, type). Appears/updates on item click. Must work in all panel configurations (single inventory, inventory + container, container-to-container). **Se alimenta desde `OnInspectionStripUpdateRequired`** en `InventoryPanelPresenter`, que publica al pasar el cursor por una celda, al soltar, desde el menu contextual y desde los slots de equipo. Stats mostradas hoy: peso, durabilidad y tamaño en celdas. Apuntar a nada publica `null`, y la vista lo traduce a campos vacios mas un icono de "sin seleccion" — la ausencia es un estado con forma propia, no un caso de error que haya que evitar.
- [x] 15. Update `InventoryPresenter` to handle stack inspection (sub-lot breakdown via `BatchItem.GetSubLots()`). **Desplegable por variante** (`sublots-popup`), anclado a la esquina superior derecha de la card mediante `PanelPoint` — un valor de Core hermano de `CellSize`, para que los presenters transporten una posicion de UI sin conocer `Vector3`. El ancla se mide al ABRIR el menu contextual y viaja en el cierre de la opcion: cuando se pulsa "Inspeccionar" el evento de puntero ya no existe y nadie sabe de que card salio. Cada fila es interactuable: clic izquierdo agarra esa variante, clic derecho abre su propio menu contextual. La identidad de la variante viaja como `ItemEntity` y no como indice, porque todo el modelo empareja variantes por `Equivalent` y un indice habria que traducirlo de vuelta justo en el momento en que puede estar obsoleto.

  Salio de aqui `MenuContext` (`Core/MVC/Presenter/Inventory/`), que absorbio los seis parametros que se propagaban por `RenderContextualMenu` y `BuildOptions`. Tres fabricas —`FromGrid`, `FromSublot`, `FromEquipment`— hacen inconstruible el estado ilegal, igual que la hoja y la rama de `MenuOption`, y eliminan la excepcion de "ni nodo ni variante". `Target` es el nodo que contiene lo enfocado y `Item` la `ItemEntity` concreta: celda = nodo sin variante, fila = nodo con variante, equipo = variante sin nodo.

  Y `DisplayDTOsBuilder.BuildNodeData(ItemObject)`, porque hay hechos que son del NODO y no del item — si la pila tiene varias variantes no se puede saber desde una `ItemEntity` suelta. Con mezcla, la franja dice `Durabilidad: Variable` en vez de la del representante, que es lo que hacia antes sin avisar. El peso se partio en total y unitario por el mismo motivo.

- [x] 16. Cantidades parciales. **Por gesto**: shift + clic izquierdo coge una unidad, shift + clic derecho coge la mitad (redondeando hacia arriba: al partir impar, quien hace el gesto se queda la parte grande); con la mano llena los mismos gestos dejan una unidad o la mitad de lo que se lleva. La vista traduce la tecla a un `GrabPortion` y Core nunca sabe de teclados. El gesto se resuelve en el *press*, asi que `GrabGesture` marca el gesto como consumido (`_placedThisGesture`) para que el *release* no coloque el resto detras. Shift + derecho NO abre el menu contextual: es lo que hay que ceder para que el gesto exista.

  **Por menu**: "Dividir" con campo numerico (`max: Amount - 1`, porque separar todo no separa nada) crea una pila nueva en el primer hueco de la misma rejilla. Va por `RunTransfer` como cualquier transferencia, con su vuelta atras. La accion solo se ofrece cuando hay hueco: una accion que no puede cumplirse no debe aparecer, porque no tiene forma de explicar por que no pasa nada.

  Dos cosas salieron de montar esto. `PlacementVerdict.Partial` (amarillo, el de `.load-extra`): que entre parte de lo que llevas ya ocurria —por peso— y se truncaba en silencio. Y un bug de paridad: `EvaluatePlacement` decidia a trozos con un `return` por guarda, y la rama de apilar sobre una pila compatible salia por su cuenta **sin comprobar el peso**, asi que el fantasma pintaba verde y no se movia nada. Reescrito para calcular cuantas unidades aterrizarian de verdad (`UnitsThatWouldLand`) y derivar el veredicto de ese numero: asi ninguna rama puede olvidarse de una regla.

  Mientras se sobrevuela con shift se evalua UNA unidad aunque el gesto pueda acabar siendo la mitad: al pasar el cursor todavia no hay boton, y "cabe al menos una" es la respuesta valida para los dos. Evaluar la mitad pintaria rojo donde un shift + izquierdo coloca sin problema. Y como pulsar shift quieto no genera `PointerMove`, el modificador se **sondea** (`WatchModifier`, 50 ms) en vez de escucharse con `KeyDown`/`KeyUp`: los eventos de tecla exigen foco y el foco lo tienen los campos numericos del menu.

  **Rellenar la mano** (`HandBuffer.GrabMore`): repetir el gesto sobre el origen de lo que ya llevas suma mas en vez de descargar. No es agarrar otra vez —eso revienta contra la guarda de `Grab`— sino subir la reserva, porque agarrar no mueve nada y lo reservado sigue contando en su nodo; de ahi que el tope sea `Ungrabbed()` y que la mitad se mida sobre el resto sin reservar, asi que con 40 da 20, 10, 5 y converge. "Es el mismo origen" se resuelve contra la rejilla en cada pulsacion (`IsGrabbedFrom`) y no contra nada recordado: asi abandonar el item y volver sigue contando, y los items de varias celdas salen gratis porque `GetNodeAt` resuelve cualquier celda al mismo nodo. El canje es que ya no se puede devolver una porcion al nodo del que salio usando shift — pero eso era un no-op de todas formas, porque `ignoreNodeId` lo neutralizaba.

- [x] 17. Intercambio de items (`PlacementVerdict.Swap`, azul). Llevando A y pulsando sobre un B incompatible, cada uno pasa a las celdas del otro. **Decidido: el intercambio NO pasa por la mano.** Las alternativas eran dejar B agarrado —lo que obliga a sacarlo del inventario a un staging y rompe la propiedad de la que cuelga todo el diseño, que cancelar es gratis: si cancelas despues de intercambiar, las celdas de B las ocupa A y no hay vuelta— o resolverlo en un solo gesto. Lo segundo es ademas lo unico **evaluable antes de hacerlo**, que es lo que permite pintar el azul sin mentir; con la otra opcion el azul significaria "empieza y ya veremos".

  Limitado al mismo inventario a proposito: entre paneles el intercambio mueve peso y necesita las dos mitades en una transaccion, mientras que dentro de uno no se mueve ni una unidad de `BatchItem` y el peso total no cambia — es una reposicion de dos nodos en la rejilla, sin `Extract`, sin `Restore` y sin pesos que recomprobar. Exige tambien la mano con el nodo ENTERO (`Ungrabbed() == 0`): con media pila el origen sigue ocupando sus celdas y no hay hueco que ofrecer a cambio.

  El que va en la mano se coloca en la **celda pulsada**, no en la esquina del desplazado: lo que llevas se coloca donde apuntas, igual que en cualquier otra colocacion, y el otro se conforma con la esquina que queda libre porque nadie decide por el. Consecuencia a tener presente: el mismo par de items puede dar azul en una celda y rojo en la de al lado, y es correcto.

  Dos bugs de paridad salieron de aqui, los dos con la misma forma — **preguntar una parte de la decision en vez de la decision entera**. `PlaceAt` consultaba `CanSwapWith` primero, y apilar sobre una pila compatible cumple todas las condiciones de un intercambio, asi que intercambiaba lo que debia apilarse; se cura preguntando a `EvaluatePlacement`, que es quien conoce el ORDEN (`Fits` → `Partial` → `Swap` → `Blocked`). Y la consulta validaba cada colocacion por separado ignorando al otro nodo, asi que dos huellas de destino que se pisan pasaban las dos comprobaciones y colisionaban al colocar la segunda: de ahi el no-solape explicito, con el que el orden de colocacion deja de importar.

**Decided**: No auto-placement as primary flow. Items enter the player's inventory by manual drag from world containers. The player decides where each item goes. Auto-sort and first-fit exist as convenience tools, not as the default path. This reinforces the realistic logistics theme.

**Decided**: Click-to-grab, click-to-place is the **primary** interaction; drag & drop is supported as a **secondary** gesture. They are not two systems: both drive the same "held item" state, so validation and placement logic is written once. Disambiguated by a movement threshold on pointer events (no `ClickEvent`, which UI Toolkit synthesises from down+up and would fire spuriously on short drags):

- `PointerDown` on an item → record origin, `CapturePointer`, mark pending
- `PointerMove` beyond ~5px → it's a drag; item follows the cursor while the button is held
- `PointerUp` under the threshold → it was a click; enter held mode (item follows the cursor until the next click)
- `PointerUp` while dragging → place here

The three placement cases below apply identically to both gestures. Note: the sub-slots popup currently closes via a `ClickEvent` handler on `_root`; that will need migrating to pointer events too, or drags will close it mid-operation.

Left click picks up a stack into the cursor. Clicking again places it. Overflow stays in cursor. Three placement cases:
1. **Over matching item** → `TryStackOnToNode`. Overflow by maxStackSize stays in cursor.
2. **Over empty cell** → `TryAddItemAt`. Weight rejection keeps items in cursor. Grid always fits (one stack, one cell group).
3. **Over non-matching / occupied** → items stay in cursor, nothing happens.
Shift+click / right-click / context menu → `TryStackOntoHere` (immediate level with fallback to AddItem for auto-placement). Used for quick transfers.
Closing inventory / ESC with items in cursor → items return to their original position.

---

## Milestone 6 — Container interaction & transfer

**Goal**: Open external inventories (chests, carts) and transfer items between them.

**Tasks**:

- [x] 1. External container opens as additional panel (extra column). Support opening TWO external containers simultaneously (e.g. cart-to-cart transfer without going through personal inventory).
- [ ] 2. World item pickup: actions (chopping, mining, etc.) spawn items as world entities with position. Pickup goes to **hands** (carry buffer) → player loads into cart/chest/storage (world containers). Bulky items (logs, planks, ore) do NOT go into personal inventory — personal inventory is pocket/backpack scale only. Crafting uses **proximity**: pulls materials from ALL accessible sources — personal inventory, backpack, AND nearby world containers (cart, chest, etc.). Hands buffer details TBD: capacity, interaction with equipped tool, slot reuse vs dedicated carry state.
- [x] 3. Drag & drop between your inventory and external container
- [x] 4. Transfer respects both containers' grid space and weight limits
- [ ] 5. Container closes when player moves away (distance check or explicit close)
- [ ] 6. NPC/cart/wheelbarrow inventories work the same way — carts are central to logistics
- [x] 7. Backpack/bag as equipped container: inventory panel gets tabs (pockets, backpack, shoulder bag, etc.). Clicking a tab switches the grid view to that container's grid. Each tab has its own `TetrisGridState` and `StorageComponent`.

**Decided, y CORREGIDO al implementarlo**: una bolsa aporta celdas **y** conserva su propio techo de peso. La condicion es **paralela**: lo que entra tiene que caber por peso en la bolsa Y en quien la lleva. La nota original decia que el peso "subia" al personaje y que el `StorageComponent` de la bolsa solo definia dimensiones; se descarto porque dejaba `MaxWeight` sin uso y hacia inexpresable una mochila grande pero endeble.

La cadena la resuelve `InventoryObject`: `OwnFreeWeight(source)` conoce el techo propio y la UNICA regla sobre el origen —un techo no aplica a algo que ya esta debajo de el, asi que mover del bolsillo a la mochila o de la mochila al jugador no cambia lo que el jugador carga—, `FreeWeight(source)` es el unico recorrido hacia arriba, y `FitByWeight` y `CarrierBlocks` se **derivan** de ella en vez de recorrer por su cuenta. `CarryCapacity.FitByWeight` desaparecio: `CarryCapacity` se queda con las reglas puras (`GetMaxLoad`, `ClassifyLoad`) y no recorre inventarios.

Peso total del personaje = su inventario + la mochila + lo que lleve dentro, por recursion del arbol. Un contenedor **no se cuenta a si mismo**: su barra mide lo que le has metido, y su propio peso lo suma quien lo lleva.

**Refactor [x] HECHO — `InventoryService`**: existe y es el coreografo. El arbol se quedo con lo estructural (añadir/quitar hijos, recorrer, limpiar) y el servicio compone los flujos: `RunTransfer` como transaccion unica con rollback, `PlaceFromHand`, `EvaluatePlacement`/`EvaluateEquip`, `TryEquipItem`/`TryUnequipItem`, `SplitNode`, `SwapFromHand`, `GetAvailableActions`.

**Decidido al hacerlo, distinto de lo que estaba planeado**: `TransferResult` (`Success`, `PartialStack`, `GridFull`, ...) **no se hizo**, y no por olvido. Las operaciones devuelven **sobrantes** (`int`), que para mover cantidades dice mas que un enum cualitativo: "quedan 7 en la mano" permite seguir operando, mientras que `PartialStack` obliga a volver a preguntar cuanto. El veredicto cualitativo vive donde si hace falta —`PlacementVerdict` para pintar, `EquipResult` para el equipo, porque ahi no hay parciales— y el motivo del rechazo se pierde a proposito: un color no puede transportarlo.

**Lo que si queda de este refactor**: limpiar `IInventoryElement`. Siguen ahi `StackOntoHere`, `ModifyAmountHere`, `ContainsHere`, `GetAmountHere`, `DeleteItemHere`, `FindHere`, `FindNodesHere` y `SetAmount`, mas los metodos de hoja que lanzan excepcion. Valorar tambien partir la interfaz (estructural vs consulta).

**Refactor [x] CUMPLIDO con otra estructura — `EquipmentService`**: no existe esa clase, y no va a existir. La composicion entre `EquipmentSystem` e `InventorySystem` la hace `InventoryService` (`TryEquipItem`, `TryUnequipItem`), y ninguno de los dos sistemas conoce al otro, que era el objetivo. Sacarlo a un servicio propio partiria en dos algo que comparte una sola maquinaria —`RunTransfer` y su rollback— para que equipar y desequipar sean transferencias como las demas en vez de un camino aparte. Si algun dia se separa, sera por tamaño de `InventoryService`, no por diseño.

**Empezado T2 — la mitad de tirar.** `WorldInteractionSystem` (`Core/ECS/Systems/`) es el
sistema reactivo que escucha `ItemDropped` y pone en el mundo lo que sale de un inventario.
Hasta ahora ese evento **no tenia ningun oyente**: `DropItems` sacaba los items y lo tirado
desaparecia.

Existe como sistema propio, y no repartido entre `InventoryService` y la capa de Unity,
porque va a acoger todo lo que cruza esa frontera: colocar en vez de tirar, y la
interaccion de vuelta —acercarse, recoger, abrir un arcon del suelo—. Es la misma pregunta
—que existe en el mundo y como pasa de ahi a un inventario— y sin un dueno se reparte en
trozos que nadie reconoce como lo mismo. No conoce Unity: crea entidades de Core y pide el
enlace por `IEntityLinker`.

**Una tirada = un monton**, aunque lleve varias variantes: el jugador hizo un gesto y espera
ver un objeto en el suelo, no cinco superpuestos. El monton es una entidad `groundLot` con
`GroundLotComponent`, que **no** es un `InventoryComponent`: no hay rejilla, ni techo de
peso, ni colocacion, y arrastrar esas reglas al suelo seria inventar un problema. Un monton
mixto se ve como el item de su primer lote, elegido asi porque no cambia al recoger una
parte (el "mas pesado" o el "mas numeroso" si cambiarian).

**Donde cae.** Delante y a la altura de las manos, no en los pies. Eso necesita saber hacia
donde mira el que tira, que **no es la rotacion**: un cuaternion es una instruccion de giro,
y la direccion sale de aplicarla al vector "delante" (0,0,1). Para los ejes canonicos la
formula general se reduce a una columna de la matriz de rotacion, asi que
`PositionComponent.Forward()` son tres multiplicaciones por componente y ni una funcion
trigonometrica. Ya existia junto a `Right()` en ese componente, que es donde toca: nadie mas
tiene por que saber como se pasa de un cuaternion a una direccion.

La componente vertical de ese vector **se descarta** a proposito: si el que tira mirase
hacia abajo, desplazar en la direccion de la mirada enterraria el monton. Se quiere "un paso
al frente y a la altura de las manos", asi que el frente se toma en horizontal y la altura
se suma aparte, como fraccion de `BodyComponent.Height` para que un personaje bajito suelte
mas bajo sin tocar nada.

`Forward()` y `Right()` devuelven **tuplas** y no un tipo vector porque Core no tiene
ninguno. **Estar atentos**: ya son dos usos, y un tercer sitio que pase vectores por Core es
la senal para crear un `Vec3` como se hizo con `GridPos` — esos dos metodos y la propia
`PositionComponent` son los primeros candidatos a usarlo.

**Alcanzar cosas del mundo, sin fisica.** `FindTarget(actor)` decide que tiene delante y a
mano el jugador, y lo hace **entero en Core**: distancia al volumen del objetivo, cono de
mirada, y un filtro externo opcional. Se evaluo contra la alternativa —colliders trigger en
una capa y `Physics.OverlapSphere`— y se eligio Core, pero **no por rendimiento**: con una
consulta por fotograma las dos son indistinguibles (del orden de microsegundos, ~0.01% del
fotograma). La razon es que la decision queda en un metodo que se prueba sin abrir el editor
y que no depende de que nadie haya configurado bien una capa en el inspector.

Para que eso funcione con objetos grandes hace falta pasar de punto a region, y de ahi sale
`InteractionVolume`: esfera, caja orientada y capsula, cada una sabiendo contestar "¿a que
distancia estas de mi?". Jerarquia y no enum con `switch` porque la pregunta es siempre la
misma y quien busca objetivos no tiene por que saber que forma mira. **La caja cubre casi
todo**; la capsula existe para el dia en que un tronco se comporte raro.

**El volumen se mide, no se declara.** La entidad nace con la esfera por defecto de su
arquetipo —asi se le puede apuntar desde el primer fotograma, sin esperar al modelo— y el
linker la sustituye por la caja de las envolventes cuando el `.glb` termina de cargar. No
hay campo en Stack&Go y no deberia haberlo: el volumen ya esta en la geometria, y guardarlo
aparte seria el mismo hecho dos veces. De ahi sale un criterio reutilizable: **lo que decide
un diseñador va a Stack&Go, lo que se deriva del asset se mide, lo que es constante de
ajuste vive en el codigo.** Peso y modelo son lo primero; volumen es lo segundo; alcance y
angulo del cono son lo tercero.

`IReachFilter` es la escotilla para lo que Core no puede saber —si hay una pared en medio—.
Interfaz con nombre y no un `Func` suelto: un delegado en una firma no dice que comprueba.
**Se aplica el ultimo**, sobre los pocos candidatos que han sobrevivido, y los candidatos se
recorren por cercania para que una pared delante del mas proximo no bloquee el siguiente.

`GetAvailableActions(actor, target)` repite el patron del menu contextual del inventario:
el dominio contesta y la interfaz pinta. **El orden es la prioridad de diseño y es parte del
contrato**, porque `GetDefaultAction` —lo que hace la pulsacion corta— es la primera de la
lista. Se deriva en vez de decidirse aparte para que no puedan contradecirse.

Pendiente de T2: falta el input y el menu radial (tecla corta = accion por defecto, mantener
= menu), que son estrategia de camara y UI — solo FPS y TPS, el RTS se queda sin
interaccion. Falta decidir si dos tiradas seguidas se funden en el mismo monton (el sitio es
`OnItemsDropped`, buscando uno cercano antes de crear). Y `FindTarget` llama a
`GetEntitiesWithComponent`, que reserva una lista nueva en cada llamada: una por fotograma
es inocuo hoy, pero si aparecen mas consultas por fotograma toca darle a `EntityManager` una
variante que escriba en una lista prestada.

**Hecho `Unlink` (M6 T2, pieza 1 de 3).** Hasta ahora ninguna entidad enlazada se habia
destruido nunca, y `EntityManager.RemoveEntity` solo la sacaba del diccionario: su GameObject
se quedaba en la escena para siempre. Decisiones:

- **Quien enlaza, desenlaza.** `IEntityLinker.Unlink(entity)` lo llama el mismo sistema que
  llamo a `Link`, a traves de `WorldInteractionSystem.Despawn` (unlink + remove). Se descarto
  que `RemoveEntity` desenlazara solo: la creacion no puede ser automatica —entre crear y
  enlazar hay que colocar—, y automatizar solo la destruccion dejaria dos reglas para lo
  mismo. Precio asumido: otro sistema que destruya tiene que acordarse. **Si aparece un
  segundo sistema que destruye entidades, se sube a `EntityManager`.**
- **`Object.Destroy`, sin pool.** Un pool sin medicion es optimizar a ciegas.
- **Destruye siempre**, sin distinguir lo que `Link` encontro en la escena (el jugador) de lo
  que creo. La muerte del jugador se diseña cuando toque; `Unlink` no esta pensado para el.
- **Quita el `UnityEntityComponent`.** Su presencia es el hecho "tiene cuerpo en el motor";
  si sobreviviera al objeto, quien aun guarde la entidad (un objetivo cacheado, un panel
  abierto) creeria poder tocar algo destruido.
- **Sin evento `EntityRemoved`**: hoy no lo escucharia nadie. Candidato cuando un panel
  abierto (T5, contenedores del suelo) necesite enterarse.
- **Carrera del modelo cerrada en `ModelCache.Instantiate`.** Comprobaba si el padre habia
  muerto tras el primer `await` (cargar el `.glb`) pero no tras el segundo (instanciar). Con
  el `.glb` ya en cache la espera larga es la segunda, que es justo recoger algo recien
  tirado: el modelo quedaba suelto en la raiz de la escena, sin entidad que lo destruyera, o
  `MeasureInteractionVolume` tocaba un transform destruido. Se completo la misma comprobacion
  alli, y no en `AttachModel`, para que la regla viva entera en un sitio y cubra a cualquier
  llamador futuro. No verificado: si glTFast cruza fotogramas en esa espera depende de su
  *defer agent*; si no los cruza, el caso no se da, y la guarda no cuesta nada.

**Hecho `PickUp` (M6 T2, pieza 2 de 3): `WorldInteractionService`.** Sin llamadas todavia
—las hara la fuente de input—, construido en `GameMain`. Decisiones:

- **Servicio nuevo, no fundido con `InventoryService`.** Aquel gira alrededor de la mano y
  de `RunTransfer`; aqui no hay ni mano ni rollback. Saca los dos sistemas del
  `SystemManager`, como ya hacia `InventoryService.DropItems`.
- **Paridad con el alcance: `WorldInteractionSystem.CanReach(actor, target)`.** `FindTarget`
  decide en un fotograma y la accion se ejecuta despues (soltar la tecla, elegir en el
  menu), con el jugador quiza movido. `FindTarget` busca y no sirve para comprobar un
  objetivo conocido, asi que se extrajo la parte geometrica (`IsWithinReach`: distancia y
  cono) y la comparten las dos, con el mismo filtro. `CanReach` ademas da por inalcanzable
  una entidad que ya no esta en `EntityManager`. `PickUp` empieza con `CanReach` y con
  `GetAvailableActions`. **Que el menu radial se cierre solo al alejarte es de la interfaz
  (pieza 3) y no sustituye esto**: alejarse y pulsar pueden caer en el mismo fotograma.
- **Destino: el inventario raiz del actor**, sin entrar en contenedores equipados, igual que
  la transferencia rapida. Meter en la mochila es abrirla y colocar. Si algun dia existe
  "colocar automaticamente en el mejor sitio", sera del inventario y lo usaran las dos.
- **El motivo se deduce, no se pide.** `TryStackOntoHere` devuelve un solo sobrante. El
  servicio pregunta antes la misma `FitByWeight` que usa por dentro: si se coloco menos de
  lo que el peso permitia, freno la rejilla; si no, el peso. No se cambio su firma: tiene
  mas llamadas, y el motivo del rechazo se dejo fuera del inventario a proposito
  (`TransferResult`, arriba).
- **Un motivo por gesto, y gana el peso.** El peso lleno bloquea todo lo que viene detras; la
  rejilla solo tipo a tipo.
- **Un unico punto publica el aviso** (`Announce`): `InventoryFull` para volumen y el nuevo
  `WeightLimitReached` para peso, nunca los dos. Por eso `EvaluateAndFireEvents` se llama
  con `fullGrid: false`: si no, saldria "sin espacio" cuando el motivo resuelto es el peso.
  **Hoy nadie los escucha**; el aviso es un log hasta que haya HUD.
- **Una ronda de eventos por gesto**: cada lote con `announce: false` y un solo
  `EvaluateAndFireEvents` al final, como ya hacen `PlaceAmountFromHand` y `SplitNode`.
- **Vaciar y destruir son una sola llamada**: `WorldInteractionSystem.TakeFromLot` resta y,
  si el monton queda vacio, hace `Despawn`, que sigue privado. El servicio nunca destruye.
  `GroundLotComponent.RemoveUnits` identifica el lote por su variante y no por indice,
  porque quitar un lote desplaza a los de detras.
- **Invariante escrita: un monton es de un solo tipo.** Sus lotes son variantes del tipo, o
  un unico contenedor entero. Hoy la garantiza que tirar sale siempre de un nodo; un camino
  futuro que suelte varios tipos debe crear un monton por tipo. Residuo aceptado: si las
  variantes tienen distinta etapa de modelo, el monton se ve con la del primer lote.

**Pendiente: el filtro de pared no existe.** `IReachFilter` esta definido y
`WorldInteractionSystem` lo acepta, pero no hay ninguna implementacion y `GameMain` no le
pasa ninguno: hoy se alcanza y se recoge a traves de paredes. La implementacion prevista es
`LineOfSightFilter` en la capa de Unity (un rayo del actor al objetivo). Como `FindTarget` y
`CanReach` ya lo consultan, enchufarlo no toca Core.

**Recoger va al inventario, no a las manos, por ahora.** El enunciado de T2 dice que lo
recogido va a la reserva de las manos y que lo voluminoso no entra en el inventario
personal. Para cerrar T2 se recoge directamente al inventario del actor con lo que ya sabe
`InventorySystem`; las manos como reserva de carga quedan para cuando se diseñen (capacidad,
herramienta equipada, slot propio). Es aplazamiento consciente, no olvido.

**Decidido al jugarlo**: desequipar sin sitio **deja la prenda puesta**. `TryUnequipItem` ya lo hace por su rollback, asi que no queda nada pendiente aqui. Se descarta el drop-to-ground que se habia planteado: quitarte algo y que acabe en el suelo sin haberlo pedido convierte un gesto de gestion en una perdida, y el jugador ya tiene "Tirar" para eso. De paso esto suelta la unica atadura que este refactor tenia con M6 T2.

---

## Milestone 7 — Polish & integration testing

**Goal**: Edge cases, polish, and full flow testing.

**Tasks**:

- [ ] 1. Edge cases: what happens to tetris positions when items are consumed/removed? (free cells, leave gaps, or auto-compact?)
- [ ] 2. Edge cases: stack overflow — item added to full BatchItem (maxStackSize reached) but grid has space → create new BatchItem in free cells
- [ ] 3. Edge cases: item removed from middle of grid → gap handling
- [x] 3b. Nested containers don't occupy grid cells. `InventoryObject.AddContainer` adds the child to `_inventory` but never calls `_grid.Place`, so a chest inside a backpack takes up no space and isn't rendered by `RenderGridItems` (which iterates `TetrisGridState.GetElements()`). Decide whether containers should occupy cells like any other item — they have `DimensionW/H` in `BaseItemComponent` already — and if so route `AddContainer` through the grid. Until then `InventoryObject.Clone()` copies them by list only, outside the grid.
- [ ] 4. Integration tests for full inventory flow (add, remove, transfer, equip, stack, inspect)
- [ ] 5. UI polish: drag feedback, placement preview, invalid placement indicator
- [ ] 6. Performance: stress test with large grids (cart/chest with many items)
- [ ] 7. **HUD de avisos del mundo.** Hoy no existe ningun canal para mensajes en pantalla
  fuera del inventario. `WorldInteractionService.Announce` ya publica un unico evento por
  gesto —`InventoryFull` (demasiado volumen) o `WeightLimitReached` (peso completo), con la
  prioridad ya resuelta: gana el peso— y de momento solo lo escribe en el log. Falta un HUD
  que escuche esos eventos, **filtrando por actor** (un NPC que recoja no debe avisarte), y
  pinte un mensaje breve. Va aqui y no en M6 porque es pulido de un flujo que ya funciona:
  el jugador puede recoger sin el; lo que pierde es saber por que algo se quedo en el suelo.

**Note**: The old "organization bonus" concept is no longer needed — with grid-as-capacity, good organization is its own reward (more items fit). If a bonus mechanic is desired later, it can be added as a Phase 2+ feature.

---

## Design note — `EntityType` y el campo que no existia para nadie

El arquetipo de entidad era una cadena: `CreateEntity("groundLot")`. Con una errata eso
compilaba y fallaba en ejecucion, y `ResolveGameObject` comparaba contra un literal repetido
en otro fichero — renombrar un arquetipo y olvidar uno de los dos sitios dejaba todo
compilando y la entidad sin representacion. Ahora es el enum `EntityType`, clave del
registro de prototipos y unico argumento de `IEntityLinker.Link`.

Lo interesante no es el enum sino lo que salio al mirar: `InGameEntity` guardaba ese tipo en
un campo `NameId`, y ese campo **mezclaba dos preguntas**. Los cuatro arquetipos del mundo
respondian a "¿que clase de cosa eres?" y eran claves de prototipo; el `"ItemEntity"` que
pasaba `ItemEntity` respondia a "¿de que clase de C# eres?", no estaba en ningun registro y
no lo consultaba nadie. Compartian campo porque los dos eran texto.

Se opto por **quitar el campo entero** en vez de tiparlo. Quien necesita el arquetipo lo
tiene en la mano al crear la entidad; quien necesita saber si algo es un item usa
`is ItemEntity`, que no puede desincronizarse de la verdad. Con el campo se fueron
`GetEntityType()` de `IEntity` y el parametro `type` de `CreateCloneInstance`, que
`ItemEntity` ya ignoraba.

**Efecto lateral que descubrio un defecto:** el unico lector del campo era
`InGameEntity.Equivalent`, que empezaba comparando tipos. Al quitarlo hacia falta otra
guarda, y la buena resulto ser la que faltaba desde el principio: **comparar cuantos
componentes tiene cada una**. El recorrido era de un solo sentido —comprueba que los mios
estan en el otro, no al reves—, asi que una entidad con componentes de mas pasaba por
equivalente y la respuesta cambiaba segun cual de las dos preguntara. Comparar clases en
lugar del campo no lo habria tapado: el jugador y un monton son los dos `InGameEntity`.

---

## Design note — Composition-derived types

The `ItemType` enum currently acts as an explicit category. But with ECS composition, item type emerges naturally from which components an entity has: equippable = has `WearableComponent`, consumable = has `NutritionComponent`, weapon = has `DamageComponent`, etc. When implementing game logic, prefer querying component presence (`HasComponent<T>()`) over switching on `ItemType`. The enum can stay as UI metadata (inventory tab filters, icon badges) but should not drive mechanical decisions. This keeps the system open to new item archetypes without modifying enums or adding switch cases.

---

## Modelos 3D — modulo de autoria en Stack&Go

Implementado en Stack&Go. **La parte de Unity sigue sin tocar**, a proposito.

**Forma del dato.** Un item tiene una lista de *etapas* y, si hay mas de una, una *magnitud*
que decide cual aplica. Una etapa es un umbral mas una o varias *variantes*
intercambiables: mismo estado, aspecto distinto, elige quien lo dibuje. El consumidor
recorre las etapas de mayor a menor umbral y se queda con la primera cuyo umbral no supere
el valor de la magnitud, asi que una etapa de umbral 0 es el caso por defecto. Un item con
un solo modelo es, simplemente, una etapa de umbral 0 con un fichero — no hay un camino
especial para el caso simple.

**Por que la magnitud se elige y no se infiere.** La alternativa era cablear "desgaste" como
el eje de variacion. Se descarto porque el eje real depende del item: una antorcha varia por
combustible, un cultivo por madurez, una herramienta por durabilidad. El desplegable se
puebla con los campos FLOAT/INT de todos los componentes definidos, cualificados como
`Componente.campo`, y solo numericos porque un umbral es una comparacion de orden: un
booleano o un enum no tienen "mayor o igual".

**Los modelos no son un componente en Stack&Go, pero si al exportar.** Dentro de la
aplicacion son tablas propias colgando del item (`item_model_stages`, `item_model_files`) y
una columna `model_driven_by`, con un DAO interno dentro de `ItemDAO` — el mismo molde que
`ItemComponentDAO`, que tampoco esta en `DAOType` ni en `DataContext`. La razon es que las
etapas no tienen vida propia fuera del item: no se comparten, no se listan, se borran con
el. Al exportar si viajan como un `ModelComponent` mas dentro de `components`, porque para
el juego "que modelo muestro" es una propiedad del item igual que su peso, y darle forma
propia en el JSON obligaria a leer el fichero de dos maneras.

**Tres nombres para un fichero, y no es redundancia.** `storedName` es un UUID y es lo unico
que guarda la base de datos: la carpeta es plana y compartida, asi que conservar el nombre
de origen haria que dos ficheros llamados igual se pisaran. `originalName` es etiqueta de
editor. El nombre legible (`<id>_<nombre saneado>_<NN>.glb`) se construye **solo al
exportar**, que es cuando existe el id y cuando importa que se entienda; dentro de la
aplicacion no serviria, porque un item puede cambiar de nombre y el fichero no se enteraria.

**Donde se copia el fichero.** En `Item.setModelStages`, es decir en el modelo, igual que
`Entry.setImagePath` con los iconos. Es el unico punto por el que pasan los tres caminos
—alta, modificacion e importacion—, y repartirlo por los controladores seria la misma regla
escrita tres veces. Se copia al **guardar**, no al elegir: una edicion abandonada no deja
ficheros huerfanos. Hasta entonces el fichero esta "pendiente" y lleva su ruta de origen en
un campo distinto, en vez de reutilizar `storedName` para significar dos cosas segun el
momento.

**Borrar filas no basta.** Es la unica diferencia real con los componentes: una fila de
componente no deja nada detras, un `.glb` si. Cada guardado compara los nombres almacenados
de antes con los de despues y barre la diferencia, comprobando primero que ninguna otra fila
los referencie. Y como borrar una coleccion o una cuenta se lleva sus items **en cascada**,
sin que ningun codigo Java vea pasar cada uno, esos dos borrados llaman ademas a una escoba
que repasa la carpeta entera.

**Validacion antes de exportar, no durante.** Se comprueba que los ficheros sigan en disco,
que no haya umbrales repetidos ni etapas vacias, y que la magnitud siga existiendo y siendo
numerica. Todo de golpe y antes de abrir el selector de fichero: una exportacion a medias es
peor que ninguna, porque el autor se lleva un zip que parece bueno y el fallo aparece dentro
del juego, lejos de donde se arregla.

**Los dos exportadores siguen siendo dos.** El completo es copia entre dispositivos: los
modelos viajan con su `storedName` y el importador los devuelve a su etapa (con nombre
almacenado nuevo, porque dos equipos no tienen por que ponerse de acuerdo en un UUID). El de
coleccion es material de juego: nombres legibles y `ModelComponent`. Unificarlos fue
considerado y descartado — sirven a publicos distintos. Lo unico que se le anadio al de
coleccion es el `id` del item, que antes no viajaba.

**Hecho en Unity.** `ModelComponent` + `ModelStage` en `Core/ECS/Component/ItemComponents/`,
registrados en `JsonItemCatalogLoader` bajo la clave `"Model"`. El componente guarda **rutas
relativas, no mallas**: `Core` no sabe cargar glTF ni tiene por que, y traducir una ruta en
algo dibujable es trabajo de Unity. Aqui solo vive la regla de cual toca.

Tres decisiones dentro del componente:

- **Reordena las etapas al leerlas**, aunque el exportador ya las mande ordenadas. El orden
  es de lo que depende `StageFor`, y confiar en el otro lado significa que el dia que cambie
  alli, aqui falla en silencio eligiendo siempre la etapa mas baja.
- **`StageFor` nunca devuelve null**: si el valor no alcanza ningun umbral, cae a la etapa
  mas baja. Un item mal configurado debe verse feo, no invisible — un hueco en el mundo es
  mucho mas dificil de diagnosticar que un modelo que no cambia.
- **El umbral se parsea con cultura invariante.** Con la del sistema, un Windows en espanol
  leeria `0.7` como el numero siete: el item cargaria sin error y jamas alcanzaria esa etapa.

`ModelStage` es inmutable, asi que `Clone` comparte las etapas con el clon en vez de
copiarlas: el prototipo y sus mil manzanas apuntan a la misma lista de rutas.

El nombre del tipo en el JSON es `"Model"`, sin sufijo, para no ser el unico de los diez que
lo lleva. Cambiado tambien en el exportador de Stack&Go.

**`ModelCache`** (`Unity/Services/`, glTFast 6.20). Mismo papel que `TextureCache` y por la
misma razon: los modelos los nombra el catalogo exportado, no el proyecto, asi que no pueden
ser prefabs ni referencias de inspector.

Lo que se cachea es el `GltfImport` —el fichero ya interpretado, que sabe instanciarse N
veces—, **no** el GameObject: cachear el objeto obligaria a clonar una jerarquia entera cada
vez, que es mas caro y mas fragil que pedirle otra instancia a quien ya tiene los datos. Y
se cachea la **tarea**, no el resultado: dos entidades que aparecen en el mismo fotograma
piden el mismo modelo antes de que la primera carga acabe, y guardando el resultado la
segunda encontraria la cache vacia y releeria los mismos megabytes.

`Instantiate` promete **una sola raiz** y para eso instancia con
`SceneObjectCreation.Always`. Por defecto glTFast usa `WhenMultipleRootNodes`: si la escena
del fichero tiene un unico nodo raiz lo cuelga directamente del padre, y si tiene varios crea
un contenedor. O sea que la forma de lo instanciado dependeria de como estuviera montado el
`.glb` en Blender, y quien llama no podria saber si bajo su transform acaba de aparecer un
objeto o cinco. Los bytes se leen a mano en vez de darle la ruta al paquete, igual que en
`TextureCache`, para no depender de como entienda cada plataforma una URI `file://`.

**Lo que el `ModelCache` no hace, a proposito** — las cuatro son baratas de anadir y ninguna
paga hoy lo que cuesta, pero conviene saber que no estan:

- **Sin pooling.** Cada instancia se crea y se destruye. Con muchos objetos repetidos en el
  mundo habra que reciclar jerarquias.
- **Sin liberacion automatica.** Todo modelo cargado sigue en memoria hasta que alguien llame
  a `Clear()`, aunque no quede ninguna instancia en pantalla. Falta una politica de descarga
  (contar instancias vivas, o descargar por escena).
- **Sin cancelacion.** Si la entidad que pidio el modelo muere durante la carga, la carga
  termina igual y el resultado se tira.
- **Sin modelo por defecto.** Un fichero que falta devuelve null y deja un hueco.

**Coste de memoria del camino en ejecucion.** Un modelo importado en el editor llega a la
GPU con las texturas en formato comprimido (BC/DXT); uno cargado en ejecucion desde un `.glb`
con PNG dentro se descodifica a RGBA sin comprimir, entre cuatro y ocho veces mas VRAM por
textura segun el formato de destino. La salida cuando duela no es abandonar el catalogo, sino texturas mas pequenas
o KTX2/Basis dentro del `.glb` (`com.unity.cloud.ktx`), que si llega comprimido a la GPU.

**`ItemComponentRegistry` + `INumericFields` + `ItemMagnitudes`** (`Core/Item/` y
`Core/ECS/Component/`). Resuelven la pregunta "de `"Material.hardness"` a un numero de esta
entidad", que es la que faltaba para elegir etapa.

El **registro** es el unico sitio que traduce el nombre con el que un componente viaja en el
catalogo a algo del dominio. Antes ese conocimiento estaba dentro de
`JsonItemCatalogLoader`, que lo necesitaba para fabricar; en cuanto aparecio un segundo
interesado —encontrar un componente ya puesto, por nombre— habria hecho falta un segundo
mapa con las mismas claves y nada que obligara a mantenerlos iguales. Registra con un
generico (`Register<MaterialComponent>("Material")`) para que el tipo aparezca una sola vez
por linea.

`INumericFields` lo implementan los componentes con campos numericos. **Se descarto
reflexion**: ataria el nombre que el autor escribe en Stack&Go al del miembro en C#, de modo
que un renombrado en un refactor compilaria sin quejarse y rompería el dato en ejecucion,
lejos y en silencio.

El primer intento escribia el nombre dos veces por campo —una en `SetFromValues` y otra en
`TryGetNumericValue`— y nada obligaba a que las dos listas coincidieran. Lo resuelve
**`NumericFields<T>`**: cada componente declara sus campos una sola vez, con su nombre, su
lector y su escritor, y las dos direcciones se derivan de esa declaracion. Dos detalles no
obvios:

- **Estatico por clase, no por instancia**, y por eso los accesores reciben el componente en
  vez de capturarlo. Un mapa por instancia seria un diccionario de delegados por cada
  manzana del mundo, y de componentes hay tantos como entidades.
- **Lista, no diccionario**, porque el orden de declaracion es el orden de aplicacion y eso
  importa: `SetHunger` recorta contra `maxHunger`, asi que aplicar el hambre antes que su
  maximo la recortaria contra cero. La busqueda por nombre recorre la lista, que no pasa de
  una docena de entradas y no se consulta por fotograma.

El escritor pasa por el metodo que ya existia (`SetDurability`, `SetHunger`) en vez de tocar
el campo, para que cargar desde el catalogo respete los mismos recortes y validaciones que
cualquier otra escritura.

`ItemMagnitudes.TryRead` vive en el dominio del item y no en la capa visual, porque la misma
pregunta la hara el crafting por estado (hierro por encima de cierta temperatura). Devuelve
false sin ruido en todos los casos de "no se sabe": quien pregunta ya necesita un plan para
eso —el resolutor de modelos cae a la etapa por defecto— y una excepcion obligaria a
envolver cada consulta.

**`ItemModelResolver` + el linker.** `ResolvePath(entity)` vive en Core porque no tiene una
linea de motor —componentes, un float y una cadena—, asi se prueba sin abrir el editor y el
linker se queda con lo unico que solo el puede hacer. Sabe ademas que un `groundLot` se
representa por el item de su primer lote, para que la capa que dibuja no distinga casos.

`UnityEntityLinker.ResolveGameObject` ya no ramifica sobre `entityType` con un caso: ahora
son dos casos que si son distintos de verdad. El jugador **se busca** (esta en la escena con
su prefab y su camara, no sale del catalogo); lo demas **se crea** como GameObject vacio. El
vacio es la entidad de cara al motor —posicion, y manana collider—, y el modelo es un hijo
suyo que se puede sustituir al cambiar de etapa sin rehacer nada.

El modelo se cuelga **sin esperar**: `Link` es sincrono y quien crea entidades no debe
bloquearse por un fichero. El precio es una ventana en la que la entidad existe y no se ve;
si llega a molestar, la salida es precargar al arrancar, no esperar aqui. Si la entidad
muere durante esa ventana, `ModelCache` lo detecta —anota si habia padre al empezar y lo
comprueba al terminar— y descarta el modelo en vez de tocar un transform destruido.

Quien resolvio el GameObject dice tambien si lo ha **creado**, y de eso depende la direccion
del sincronizado inicial: lo que ya estaba en la escena manda sobre Core, lo recien creado
obedece a Core. Antes se adivinaba mirando si la posicion era el origen, que es cierto por
casualidad y falso en cuanto algo nazca en (0,0,0).

**Pendiente en Unity:** `public int id` en `ItemData` y `TypeIdMapper` indexado por el,
eliminando `id_mapping.json`; modelo por defecto para el item que no traiga ninguno; volver
a preguntar la ruta cuando cambie la magnitud (hoy el modelo se resuelve una vez, al
enlazar, y no se entera de que el item se desgaste); y el collider del monton, que segun lo
hablado sera una primitiva dimensionada midiendo las cajas envolventes del modelo ya
cargado, nunca un `MeshCollider` ni las dimensiones de rejilla del `BaseItemComponent`.

---

## Future (not Phase 1)

Hand added notes (by me by hand):
  - Review all the repo to rename some atributes that could be in the wrong format (for ex, atributes nor starting in _ + camelCase or public atributes nor formated in PascalCase withoput _)
  - If in a future the system defined as reactive (expected in origin to be triggered by gameEvents) show they are direct-call driven, change the super type IReactiveSystem to something that fits the new semantic and dont force the system to implement unfitting features (like observer behavior)


- [x] **`GameInteractionContext`** — fourth context alongside `GameDataContext` / `GameSessionContext` / `GameSystemContext`, holding per-player *interaction* state (as opposed to world data, session state or infrastructure). First and currently only inhabitant: `HandBuffer` (the held stack for click-to-grab / drag & drop). Expected to grow with the open external container, the currently selected node for the inspection strip, and similar UI-interaction state.

  Rationale for a context rather than a presenter field or an ECS component: it must survive the presenter rebuild on UI live reload, it is shared by every presenter that can grab or place (own inventory, chest, corpse, dev creative panel), and there is exactly one per *player*.

  **Coop/multiplayer angle (the reason it is its own box).** The eventual split is authoritative state (server) vs per-client state. `HandBuffer` never touches an inventory at all — it holds references and a count, and `NotifyPlaced` only discounts what someone else already moved — so the whole grab state is client-side by construction. The network boundary falls on the transaction that does move things (`InventoryService.PlaceAmountFromHand`, over `InventorySystem.TryMoveItemTo`), which becomes the request to the server. Note that `GameSystemContext` **already mixes both boxes today** — `SystemManager` is authoritative, `PresenterManager` is inherently per-client. `PresenterManager` is the expected second inhabitant of this context; moving it is the natural next step, not part of this task.

  **Implementation note:** build it in `GameMain.Awake`, **not** in `BuildViewsAndPresenters` — that method runs again on every live reload, which would hand back an empty hand and defeat the whole point. Inject it into `InventoryPresenter`'s constructor; the presenter receives it, never creates it.

  **Naming collision to resolve:** M6 T2 calls the bulky-item carry buffer "hands". That one *is* game state (persists with the inventory closed, counts toward weight) and will likely be an ECS component. Two different things called "hand" — consider renaming this one (`HeldStack`, `CursorHand`, `GrabState`) and leaving `Hands` to M6.

- [ ] Dependency injection via context aggregator / service layer. `GameContext` (Unity/MVC/Controller/) agrupa los sub-contextos de Core (Data, Session, System, Interaction) mas las piezas de Unity, con API de constructor encadenado. **Ya no es codigo muerto**: `GameMain.Awake` lo crea y lo puebla, y de el salen `InputManager` y `HUDManager`. Lo que sigue pendiente es lo otro: decidir si los servicios se reparten por ahi o se pasa a un proveedor al estilo de Stack&Go (`ServiceConsumer` + servicios suministrados por un controlador). Hoy `InventoryService` se construye suelto en `GameMain` con un comentario que ya avisa de que si crecen los servicios toca un localizador.
- [ ] Player-facing UI scale setting. `PanelSettings-Inventory` is set to `Constant Pixel Size` (1 UI unit = 1 screen pixel), which is the sharpest option and correct while developing at the monitor's native resolution — `Scale With Screen Size` was resampling every glyph and icon by a fractional factor and made the whole panel look soft. The trade-off is that on a 4K display the UI would render at half its physical size. Fix when it matters by exposing `panelSettings.scale` as an options slider rather than reverting the scale mode; integer factors (1x, 2x) keep it pixel-perfect. Related: judge UI sharpness with the Game view maximised (Shift+Space) or in a build — at 1920x1080 the editor layout can never show the game at 1:1.
- [ ] Relocate pure rule helpers out of `ECS.Systems`. `CarryCapacity` sits in the systems namespace and is named like one, but it is a **static stateless class**: it implements neither `IPeriodicSystem` nor `IReactiveSystem`, is never registered, holds no state and processes no entities. It owns `GetMaxCarryWeight` plus the encumbrance thresholds and `ClassifyLoad`. Its own comment admits it is a placeholder ("when the real system loop is implemented, this will become a system with its own component"). Misleading as it stands — the meaningful split is *live registered object with side effects* (`InventorySystem`: posts events, mutates inventories, must be injected as an instance) versus *pure function anyone can call for free* (`CarryCapacity`). Consider a `Core/Rules/` namespace for the latter, and move it back when it genuinely becomes a system.
- [x] **Value objects for coordinates and sizes.** `GridPos` (fila, columna) en `Core/Inventory/` y `CellSize` (lado de celda en px) en `Core/MVC/View/UI/Inventory/`, ambos `readonly struct`.

  `GridPos` no es azucar sintactico: dos `int` adyacentes con significados distintos son intercambiables para el compilador, y `TryAddItemAt(..., col, row)` compilaba igual que la version correcta. Ese bug ya ocurrio una vez (el `IndexOutOfRangeException` del `PointerUp`). Con un solo parametro la inversion solo puede colarse al construirlo. De paso sustituye el centinela `(-1, -1)` de `FindFirstFit` y `PointToCoords` por `GridPos.None` / `IsNone`, y unifica el calculo de coordenadas de la vista: `PointerDown` y `PointerUp` duplicaban la division por el tamano de celda en vez de usar `PointToCoords`.

  `CellSize` existe por otra razon: `Core/` tenia `using UnityEngine` en los dos presenters de inventario para transportar un `Vector2` que ninguno de los dos leia. No se uso `System.Numerics.Vector2` porque dos tipos llamados `Vector2` en el mismo archivo obligarian a poner alias en cada frontera, y porque un tipo matematico para un dato que no se opera es ruido. La conversion ocurre en un solo sitio, la vista.

  **No** son candidatos a struct, y se decidio explicitamente dejarlos como clases: los DTO de pintado (`ItemDisplayData` tiene trece campos y es mutable — un struct mutable se copia al iterarlo y las escrituras se pierden), y `GridElement` (mutable, con identidad, referencia a un nodo).

- [x] **`SubLot`** — hecho. `Core/Inventory/SubLot.cs` es un `readonly struct` con `Item`, `Amount`, `TotalWeight`, `Deconstruct` y `Equivalent`. No se gano rendimiento —ya era un tipo valor— sino nombre y un sitio donde colgar el peso, antes recalculado a mano en cada punto. Lo usan `BatchItem`, `ItemLotEvent`, `GroundLotComponent`, los origenes de agarre y el servicio.

- [ ] **Revisar `EntityId` / `NameId`** (`Core/Handler/`). Ambos comparan convirtiendo a texto: `EntityId.Equals` hace `id.ToString() == another.ToString()`, y `CompareTo` ordena **alfabeticamente** un entero — el id 10 va antes que el 9. Consecuencias: `new NameId("5")` es igual a `new EntityId(5)`, dos identidades de tipos distintos que jamas deberian coincidir; y cada comparacion asigna dos cadenas, en un camino que se recorre por cada busqueda de entidad. Tampoco implementan `IEquatable<T>`, asi que usarlos como clave de diccionario boxea y pasa por el `Equals(object)` lento.

  El arreglo: comparar por el valor real (int con int, string con string), rechazar la comparacion entre tipos distintos de handler, implementar `IEquatable<T>` y ordenar numericamente en `EntityId`. Pasarlos a `struct` fue considerado y **descartado**: se usan a traves de `IHandler`, y un struct en variable de interfaz se boxea — se perderia justo la ventaja buscada, con conversiones invisibles de propina. El problema no es class-vs-struct, es la comparacion.

- [ ] 3D item preview in inventory UI. Ahora es viable sin trabajo nuevo: `ModelCache` ya
  instancia un modelo bajo cualquier transform, asi que es montar un soporte delante de una
  camara de previsualizacion y pedirle la ruta a `ItemModelResolver`.
- [ ] Stack&Go full bridge. **Parcialmente hecho**: el catalogo entero, los iconos y ahora los
  modelos 3D viajan en el zip y se cargan en ejecucion, y el `id` de Stack&Go ya se exporta.
  Lo que falta es (a) que Unity use ese `id` como typeId y desaparezca `id_mapping.json`, y
  (b) automatizar el paso manual de descomprimir el zip en `StreamingAssets`.
- [ ] Save/load inventory state (serialization)
- [ ] Item tooltips with detailed stats
- [ ] Normalize `this.` usage — remove unnecessary `this.` references (underscore-prefixed fields make it redundant)
- [ ] Move `prototypes` dictionary out of `EntityManager` — entity creation should go through `PrototypeFactory`, not be managed internally by `EntityManager`
- [ ] **Fabrica de eventos.** Hoy cada llamante construye el evento a mano (`new ItemLotEvent(GameEventType.ItemDropped, origin, lots)`), lo que permite emparejar un `GameEventType` con una clase de evento que no le corresponde — nada impide `new ItemLotEvent(GameEventType.Overweight, ...)`. La redundancia clase/enum se resolvio agrupando por **forma de la carga util** (`ItemLotEvent` transporta `List<SubLot>`, lo llamen tirar, recoger o destruir) y dejando el significado en el enum, pero eso traslada el riesgo al sitio de llamada.

  Salida prevista: constructor privado + fabricas estaticas por caso (`ItemLotEvent.Dropped(entity, lots)`, `.PickedUp(...)`), que fijan el tipo y se leen mejor que un `new` con un enum suelto. **Aplazado a proposito**: con un solo caso emitido (`ItemDropped`) la fabrica es maquinaria que no paga lo que cuesta. Revisar cuando haya dos o tres.

  Regla que acompaña la decision: si un caso nuevo necesita una carga util **distinta**, es otra clase de evento, no un campo opcional mas en esta. Ahi es cuando `ItemLotEvent` se convertiria en cajon de sastre con la mitad de los campos nulos.

- [ ] **Porciones desde el desplegable de sub-lotes.** Los gestos con modificador (shift + clic
  izquierdo para una unidad, shift + clic derecho para la mitad) se han montado solo sobre las
  celdas de la rejilla. Las filas del desplegable de variantes podrian aceptar los mismos, pero
  de momento no: si quieres mover una cantidad concreta de una variante concreta, abres el
  desplegable y usas su menu contextual, que ya distingue la variante. Revisar si la doble via
  compensa, o si tener el gesto solo en la rejilla resulta incoherente al jugarlo.

- [ ] Filtered consumption for crafting: `ConsumeFiltered(Predicate<ItemEntity> filter, int amount)` in `BatchItem` + wrapper in `InventorySystem`. Recipes need items matching not just typeId but specific state (e.g., hot iron ingot vs cold). `Equivalent()` may be too strict — evaluate whether a looser matching system is needed (partial match, predicate-based). Uses `BfsFindAll(typeId)` + filter per sub-lot. Additive, no structural refactor needed.
