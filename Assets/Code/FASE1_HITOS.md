# Phase 1 — Inventory: Implementation Milestones

## Where we are right now

**Current task: M5 task 10.** Milestone 5 is done except task 6 (half: viewing layers works, managing them needs the interaction) and task 10 itself.

**Done and working:** the inventory window renders the tetris grid with item blocks, the equipment cross with its layer popup, and a weight bar that colour-codes by encumbrance band. A dev item catalog lives in a side panel and lists the prototypes with search. UI live-reload works, so USS edits apply without leaving play mode.

**Built but not yet wired to any input:**

- `HandBuffer` (Core/Inventory) — the "held items" state. Complete with docs, never instantiated by anyone.
- `InventorySystem.TryMoveItemTo` — the transactional move: remove from source, add to destination, roll back the leftover per variant, clean the node last. Never called.
- Supporting primitives added for it: `ItemObject.Extract` / `GetAmount(ItemEntity)` / `ModifyAmount(ItemEntity, int)`, `InventoryObject.Extract` / `CleanNode` / `ModifyAmount(node, item, amount, clean)`, `TetrisGridState`'s `ignoreNodeId`, and `AddItemAt` stacking onto a compatible node instead of always creating one.

**The immediate next step is the window coordinator**, and it is still undecided:

1. What it is called and where it lives.
2. Whether it receives the views' events directly, or the presenters stay as intermediaries and talk up to it.
3. It must own the `HandBuffer` and hold the three windows' presenters — it is the only thing that can see the player inventory, both side slots and the hand at once, which is exactly what "grabbed here, dropping there" needs.

**Also pending on task 10**, once the coordinator exists: the pointer state machine (click-to-grab primary, drag secondary, disambiguated by a movement threshold — see the Decided note under M5), the visual for the item following the cursor, dimming the source block, and turning the catalog rows into grab sources instead of the current direct-add.

**Two known holes, noted where they matter:** moving a whole node with mixed sub-lots inside one inventory has not been exercised, and the source-entity weight re-evaluation in `TryMoveItemTo` skips when source and destination share an entity. Neither can bite until items actually move.

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
- [ ] 6. Equipment tab: click slot to see/manage layers (from M4). **Viewing done**: micro-button rendered in each slot corner (`position: absolute`), visible only when the slot holds more than one layer. Toggling it opens/closes a single shared popup (last child of `main-area`, so it draws above everything) anchored to the slot's top-right corner via `worldBound` + `WorldToLocal`. Popup content rebuilt on every open (no caching). Sub-slots ordered outermost→innermost, skipping the layer already shown in the main slot. Click outside closes it (`ClickEvent` bubbling to root + `StopPropagation` on button and popup). **Pending**: managing layers (equip/unequip from sub-slots) — depends on drag & drop, tasks 10-13.

Right panel (inventory):
- [x] 7. UI: render grid with item blocks sized by dimensions (w×h from BaseItemComponent). Blocks live in an `items-layer` created by `GenerateGrid` as the last child of the generated `inventory-grid` (so it matches the grid's exact size, unlike the outer `item-grid` container which stretches). Each block is `position: absolute` and sized/placed in **percentages** (`col * 100 / gridW`, etc.) instead of pixels — no cell-size constant duplicated between USS and C#, and the layout survives any change to `.inventory-grid-cell`. Data flows as `GridItemDisplayData` (composes `ItemDisplayData` + row/col) built from `TetrisGridState.GetElements()`.
- [x] 8. UI: grid is fixed size (gridW × gridH), not scrollable — what you see is what you have. `ScrollView` removed: grid dimensions are designed to fit, so a scroller would be a patch for a problem deliberately avoided — and it would fight the pointer-drag gestures coming in task 10, since `ScrollView` captures drags to pan its content. Without it, a grid that doesn't fit overflows visibly instead of hiding the problem. The old `item-grid` wrapper survives (renamed `grid-mount`): it is the mount point `GenerateGrid` can `Clear()` without destroying its `stats-bar` sibling, and it carries the `flex-grow: 1` + `align-items: center` that place the fixed-size grid top-centre in the panel. `SetInternallGrid` renamed to `MountGrid` and made private — only `GenerateGrid` ever called it.
- [x] 9. Weight bar above the inventory grid — colour-coded by threshold. Rail (fixed height, `flex-shrink: 0`) with the fill as an absolutely-positioned child whose `width` is `Length.Percent(ratio * 100)`, clamped to 100 for painting but **not** for classifying. The label is a sibling of the fill, not a child: inside it, the text spilled out of the panel whenever the fill was narrower than the text — worse the emptier the inventory.

  Thresholds and classification live in `CarryCapacity` (`EXTRA_WEIGHT` / `OVERWEIGHT` / `IMMOBILE` + `ClassifyLoad`), a pure function returning the matching `GameEventType` — those values already are the vocabulary for these bands, so a parallel enum would only need keeping in sync. Single source of truth: `InventorySystem` uses it to pick which event to post (collapsing a 20-line if/else into three, with the per-band logging split out into `LogLoad`), and `InventoryPresenter` uses it to tell the view which band to paint. Being pure it can be called on demand when opening the inventory, where no event has fired — so **the presenter does not subscribe to the EventBus yet**: `Refresh()` recomputes everything from the model, and nothing changes weight mid-session until task 10. The view receives the band already decided and only maps it to a USS class, so no domain vocabulary leaks into it.

  Colours live in USS (`.load-normal` / `.load-extra` / `.load-over` / `.load-immobile`) rather than `style.backgroundColor`, so palette tweaking benefits from the live reload built in task 0 instead of needing a recompile. Swapping is table-driven from a `Dictionary<GameEventType, string>`: remove all four, add the one — adding a fifth band is one entry, not five edited branches.

  Prerequisite discovered while doing this: **the whole event subsystem was disconnected.** `EventBus.Subscribe` was never called anywhere, and neither `InventorySystem` nor `MovementSystem` was ever instantiated, so no weight event had ever fired and the movement debuff never applied. Fixed by splitting `IGameSystem` into `IPeriodicSystem` (has `Process`, driven by tick or frame) and `IReactiveSystem` (declares `SubscribedEvents`, driven by the bus), and making `SystemManager.RegisterReactiveGameSystem` subscribe on registration — registering *is* subscribing, so a reactive system can no longer end up alive but deaf. Also added the missing `else` posting `NormalWeight` (`MovementSystem` already handled it), without which returning below 0.70 never notified anyone.

Interaction:
- [ ] 10. UI: move items within the grid to reorganize (mechanical impact — frees space for new items). Primary interaction is **click-to-grab / click-to-place** (see Decided note below); drag & drop is supported as a secondary gesture over the same "held item" state. Build alongside it a **dev creative panel**: search field + filtered list over `_itemCatalogue.GetAll()` (name + icon), amount field, click to `AddItem` + `Refresh`. Uncategorised for now. Needed to exercise the placement edge cases (full grid, no fit, stacking onto an existing lot, moving a 1x3 into a 1x2 gap) without editing `PrototypeFactory.AddTestItems` and restarting.

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

  **Bug pendiente — bloque fantasma tras una colocacion invalida.** Al soltar en una celda
  ocupada, fuera de la grid o donde el item no cabe, la mano se cancela (correcto) pero el
  bloque de origen se queda con `item-block-grabbed` puesto. Es un fallo de repintado, no de
  dominio: el `IsGrabbed` se calcula al construir los DTOs, asi que despues de cancelar hay
  que volver a renderizar **todos** los paneles — el nodo atenuado puede estar en un panel
  distinto de aquel donde se solto. Revisar que la cancelacion pase siempre por el punto que
  refresca a todos, y no solo por el panel que la origino.

- [ ] 11. UI: move items from inventory → equipment slot, and from equipment sub-slots back to inventory (from M4). Unblocks the pending half of task 6.
- [ ] 12. First-fit auto-place algorithm (for right-click pickup / quick-store): scan grid left-to-right, top-to-bottom, place in first valid position. Used as fallback, not primary flow.
- [ ] 13. Right-click context menu on inventory items: [Equip] [Consume] [Drop] [Inspect] (from M4)

Polish:
- [ ] 14. Item inspection strip (bottom, full width): left = large item icon, center-left = name + description, center-right = stats (condition, weight, durability, grid size, type). Appears/updates on item click. Must work in all panel configurations (single inventory, inventory + container, container-to-container).
- [ ] 15. Optional "auto-sort" button: best-fit algorithm to compact items and maximize free space
- [ ] 16. Update `InventoryPresenter` to handle stack inspection (sub-lot breakdown via `BatchItem.GetSubLots()`)

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

- [ ] 1. External container opens as additional panel (extra column). Support opening TWO external containers simultaneously (e.g. cart-to-cart transfer without going through personal inventory).
- [ ] 2. World item pickup: actions (chopping, mining, etc.) spawn items as world entities with position. Pickup goes to **hands** (carry buffer) → player loads into cart/chest/storage (world containers). Bulky items (logs, planks, ore) do NOT go into personal inventory — personal inventory is pocket/backpack scale only. Crafting uses **proximity**: pulls materials from ALL accessible sources — personal inventory, backpack, AND nearby world containers (cart, chest, etc.). Hands buffer details TBD: capacity, interaction with equipped tool, slot reuse vs dedicated carry state.
- [ ] 3. Drag & drop between your inventory and external container
- [ ] 4. Transfer respects both containers' grid space and weight limits
- [ ] 5. Container closes when player moves away (distance check or explicit close)
- [ ] 6. NPC/cart/wheelbarrow inventories work the same way — carts are central to logistics
- [ ] 7. Backpack/bag as equipped container: inventory panel gets tabs (pockets, backpack, shoulder bag, etc.). Clicking a tab switches the grid view to that container's grid. Each tab has its own `TetrisGridState` and `StorageComponent`.

**Decided**: Backpacks/bags add grid space but share the character's weight limit. Total carry weight = personal inventory contents + backpack item weight + backpack contents weight. The backpack's own `StorageComponent` defines its grid dimensions (extra grid space), but weight rolls up to the character's `CarryCapacity`. The value of a backpack is extra grid cells — it lets you carry more items that you couldn't fit in pockets alone.

**Refactor pendiente**: Extract `InventoryService` — move game-logic operations (Transfer, StackOnto with capacity checks, consume-for-crafting, proximity search) out of the composite tree into a service layer. The composite keeps structural operations (add/remove children, traverse, clean). The service composes them for complex flows (e.g. `InventoryService.Transfer(source, target, nodeId, amount)`). Also clean up `IInventoryElement`: remove `*Here` variants, `SetAmount`, `AddSeveralItems`, and leaf methods that throw exceptions. Consider splitting interface (structural vs query). Service orchestrators return `TransferResult` enum (`Success`, `PartialStack`, `GridFull`, `Overloaded`, `Immobile`) — qualitative result since src/dst/cursor are already updated internally.

**Refactor pendiente**: Extract `EquipmentService` — orchestrates cross-system operations between `EquipmentSystem` and `InventorySystem`. Handles: TryEquip (remove from inventory → equip in slot), TryUnequip (check inventory space via InventorySystem → remove from slot → add to inventory), drop-to-ground fallback on unequip failure. Neither system knows the other; the service composes both. (Moved from M4 T7 — equipping removes from inventory, unequipping adds back. Unequip can fail if inventory full by grid or weight. On failure: cancel or drop to ground.)

---

## Milestone 7 — Polish & integration testing

**Goal**: Edge cases, polish, and full flow testing.

**Tasks**:

- [ ] 1. Edge cases: what happens to tetris positions when items are consumed/removed? (free cells, leave gaps, or auto-compact?)
- [ ] 2. Edge cases: stack overflow — item added to full BatchItem (maxStackSize reached) but grid has space → create new BatchItem in free cells
- [ ] 3. Edge cases: item removed from middle of grid → gap handling
- [ ] 3b. Nested containers don't occupy grid cells. `InventoryObject.AddContainer` adds the child to `_inventory` but never calls `_grid.Place`, so a chest inside a backpack takes up no space and isn't rendered by `RenderGridItems` (which iterates `TetrisGridState.GetElements()`). Decide whether containers should occupy cells like any other item — they have `DimensionW/H` in `BaseItemComponent` already — and if so route `AddContainer` through the grid. Until then `InventoryObject.Clone()` copies them by list only, outside the grid.
- [ ] 4. Integration tests for full inventory flow (add, remove, transfer, equip, stack, inspect)
- [ ] 5. UI polish: drag feedback, placement preview, invalid placement indicator
- [ ] 6. Performance: stress test with large grids (cart/chest with many items)

**Note**: The old "organization bonus" concept is no longer needed — with grid-as-capacity, good organization is its own reward (more items fit). If a bonus mechanic is desired later, it can be added as a Phase 2+ feature.

---

## Design note — Composition-derived types

The `ItemType` enum currently acts as an explicit category. But with ECS composition, item type emerges naturally from which components an entity has: equippable = has `WearableComponent`, consumable = has `NutritionComponent`, weapon = has `DamageComponent`, etc. When implementing game logic, prefer querying component presence (`HasComponent<T>()`) over switching on `ItemType`. The enum can stay as UI metadata (inventory tab filters, icon badges) but should not drive mechanical decisions. This keeps the system open to new item archetypes without modifying enums or adding switch cases.

---

## Future (not Phase 1)

- [x] **`GameInteractionContext`** — fourth context alongside `GameDataContext` / `GameSessionContext` / `GameSystemContext`, holding per-player *interaction* state (as opposed to world data, session state or infrastructure). First and currently only inhabitant: `HandBuffer` (the held stack for click-to-grab / drag & drop). Expected to grow with the open external container, the currently selected node for the inspection strip, and similar UI-interaction state.

  Rationale for a context rather than a presenter field or an ECS component: it must survive the presenter rebuild on UI live reload, it is shared by every presenter that can grab or place (own inventory, chest, corpse, dev creative panel), and there is exactly one per *player*.

  **Coop/multiplayer angle (the reason it is its own box).** The eventual split is authoritative state (server) vs per-client state. `HandBuffer` never touches an inventory at all — it holds references and a count, and `NotifyPlaced` only discounts what someone else already moved — so the whole grab state is client-side by construction. The network boundary falls on the transaction that does move things (`InventoryService.PlaceAmountFromHand`, over `InventorySystem.TryMoveItemTo`), which becomes the request to the server. Note that `GameSystemContext` **already mixes both boxes today** — `SystemManager` is authoritative, `PresenterManager` is inherently per-client. `PresenterManager` is the expected second inhabitant of this context; moving it is the natural next step, not part of this task.

  **Implementation note:** build it in `GameMain.Awake`, **not** in `BuildViewsAndPresenters` — that method runs again on every live reload, which would hand back an empty hand and defeat the whole point. Inject it into `InventoryPresenter`'s constructor; the presenter receives it, never creates it.

  **Naming collision to resolve:** M6 T2 calls the bulky-item carry buffer "hands". That one *is* game state (persists with the inventory closed, counts toward weight) and will likely be an ECS component. Two different things called "hand" — consider renaming this one (`HeldStack`, `CursorHand`, `GrabState`) and leaving `Hands` to M6.

- [ ] Dependency injection via context aggregator / service layer. `GameContext` (Unity/MVC/Controller/) was written for this — groups the three Core sub-contexts (Data, Session, System) plus the Unity pieces, with a builder API, so each class receives only the sub-context it needs instead of the whole thing. It is currently **dead code**: nobody calls `new GameContext()`, and `GameMain.Awake()` builds everything with local variables and injects sub-contexts by hand. Decide whether to revive it as-is or move to a service-provider approach like the one in Stack&Go (`ServiceConsumer` + services supplied by a core controller). Until then, treat `GameContext` as inactive — it looks like live infrastructure and isn't.
- [ ] Player-facing UI scale setting. `PanelSettings-Inventory` is set to `Constant Pixel Size` (1 UI unit = 1 screen pixel), which is the sharpest option and correct while developing at the monitor's native resolution — `Scale With Screen Size` was resampling every glyph and icon by a fractional factor and made the whole panel look soft. The trade-off is that on a 4K display the UI would render at half its physical size. Fix when it matters by exposing `panelSettings.scale` as an options slider rather than reverting the scale mode; integer factors (1x, 2x) keep it pixel-perfect. Related: judge UI sharpness with the Game view maximised (Shift+Space) or in a build — at 1920x1080 the editor layout can never show the game at 1:1.
- [ ] Relocate pure rule helpers out of `ECS.Systems`. `CarryCapacity` sits in the systems namespace and is named like one, but it is a **static stateless class**: it implements neither `IPeriodicSystem` nor `IReactiveSystem`, is never registered, holds no state and processes no entities. It owns `GetMaxCarryWeight` plus the encumbrance thresholds and `ClassifyLoad`. Its own comment admits it is a placeholder ("when the real system loop is implemented, this will become a system with its own component"). Misleading as it stands — the meaningful split is *live registered object with side effects* (`InventorySystem`: posts events, mutates inventories, must be injected as an instance) versus *pure function anyone can call for free* (`CarryCapacity`). Consider a `Core/Rules/` namespace for the latter, and move it back when it genuinely becomes a system.
- [x] **Value objects for coordinates and sizes.** `GridPos` (fila, columna) en `Core/Inventory/` y `CellSize` (lado de celda en px) en `Core/MVC/View/UI/Inventory/`, ambos `readonly struct`.

  `GridPos` no es azucar sintactico: dos `int` adyacentes con significados distintos son intercambiables para el compilador, y `TryAddItemAt(..., col, row)` compilaba igual que la version correcta. Ese bug ya ocurrio una vez (el `IndexOutOfRangeException` del `PointerUp`). Con un solo parametro la inversion solo puede colarse al construirlo. De paso sustituye el centinela `(-1, -1)` de `FindFirstFit` y `PointToCoords` por `GridPos.None` / `IsNone`, y unifica el calculo de coordenadas de la vista: `PointerDown` y `PointerUp` duplicaban la division por el tamano de celda en vez de usar `PointToCoords`.

  `CellSize` existe por otra razon: `Core/` tenia `using UnityEngine` en los dos presenters de inventario para transportar un `Vector2` que ninguno de los dos leia. No se uso `System.Numerics.Vector2` porque dos tipos llamados `Vector2` en el mismo archivo obligarian a poner alias en cada frontera, y porque un tipo matematico para un dato que no se opera es ruido. La conversion ocurre en un solo sitio, la vista.

  **No** son candidatos a struct, y se decidio explicitamente dejarlos como clases: los DTO de pintado (`ItemDisplayData` tiene trece campos y es mutable — un struct mutable se copia al iterarlo y las escrituras se pierden), y `GridElement` (mutable, con identidad, referencia a un nodo).

- [ ] **`SubLot`** — nombrar la tupla `(ItemEntity item, int amount)`, que aparece en 34 sitios entre `BatchItem`, `ItemObject`, `InventoryObject` e `InventoryService`. Ya es un tipo valor, asi que no se gana rendimiento: se gana legibilidad (`List<SubLot>` frente a `List<(ItemEntity, int)>`) y un sitio donde colgar `TotalWeight`, hoy recalculado en varios puntos. Refactor mecanico, sin riesgo de cruce de parametros.

- [ ] **Revisar `EntityId` / `NameId`** (`Core/Handler/`). Ambos comparan convirtiendo a texto: `EntityId.Equals` hace `id.ToString() == another.ToString()`, y `CompareTo` ordena **alfabeticamente** un entero — el id 10 va antes que el 9. Consecuencias: `new NameId("5")` es igual a `new EntityId(5)`, dos identidades de tipos distintos que jamas deberian coincidir; y cada comparacion asigna dos cadenas, en un camino que se recorre por cada busqueda de entidad. Tampoco implementan `IEquatable<T>`, asi que usarlos como clave de diccionario boxea y pasa por el `Equals(object)` lento.

  El arreglo: comparar por el valor real (int con int, string con string), rechazar la comparacion entre tipos distintos de handler, implementar `IEquatable<T>` y ordenar numericamente en `EntityId`. Pasarlos a `struct` fue considerado y **descartado**: se usan a traves de `IHandler`, y un struct en variable de interfaz se boxea — se perderia justo la ventaja buscada, con conversiones invisibles de propina. El problema no es class-vs-struct, es la comparacion.

- [ ] 3D item preview in inventory UI
- [ ] Stack&Go full bridge (automated JSON export → item catalog)
- [ ] Save/load inventory state (serialization)
- [ ] Item tooltips with detailed stats
- [ ] Normalize `this.` usage — remove unnecessary `this.` references (underscore-prefixed fields make it redundant)
- [ ] Move `prototypes` dictionary out of `EntityManager` — entity creation should go through `PrototypeFactory`, not be managed internally by `EntityManager`
- [ ] Filtered consumption for crafting: `ConsumeFiltered(Predicate<ItemEntity> filter, int amount)` in `BatchItem` + wrapper in `InventorySystem`. Recipes need items matching not just typeId but specific state (e.g., hot iron ingot vs cold). `Equivalent()` may be too strict — evaluate whether a looser matching system is needed (partial match, predicate-based). Uses `BfsFindAll(typeId)` + filter per sub-lot. Additive, no structural refactor needed.
