# Phase 1 — Inventory: Implementation Milestones

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

**Decided**: Equipment grid is 3×4. Layout:
```
(L.Shoulder)  (Head)    (R.Shoulder)
(L.Hand)      (Torso)   (R.Hand)
(reserved)    (Legs)    (reserved)
(reserved)    (Feet)    (reserved)
```
Shoulders: bags, backpacks (1 shoulder = satchel, both = backpack). 4 reserved slots use the same `enabled=false` mechanism as amputation — unlocked when gameplay needs them (belt, cloak, etc). `EquipmentSlotType` enum needs updating to add shoulders and reserved.

**Decided**: Layer order is validated. Two rules:
1. **topLayer** (component data): `WearableComponent` has a `topLayer` bool. Rigid/structured items (armor, chestplate) are topLayer — nothing can be equipped on top of them. Covers: no shirt over chestplate, no armor over armor.
2. **No duplicate garment category** (system logic, NOT component data): `InventorySystem` enforces that you can't equip two items with the same `garmentCategory` in the same slot. Iron plate armor and studded leather armor are both `Plate` → can't stack. A shirt and a camisole are both `Shirt` → can't stack. One Shirt + one Vest + one Plate = OK. System-level validation, can be relaxed in the future if needed.
- `maxLayers` on `EquipmentSlot` remains as a hard safety cap.
- Equip order: new items always go on top (outermost). To change order, unequip and re-equip.

---

## Milestone 5 — Tetris grid UI & split view

**Goal**: Inventory displayed as tetris grid where grid IS the capacity system. Split layout with personal panel.

**Tasks**:

Foundation:
- [ ] 1. `TetrisGridState` already exists from M3. This milestone adds the UI rendering and interaction on top of it.
- [ ] 2. Split view layout: left panel (personal) + right panel (inventory). Replaces current tab-based UI.

Left panel (personal):
- [ ] 3. Health placeholder in personal panel
- [ ] 4. UI: render 3×4 equipment grid with layer indicators (from M4)
- [ ] 5. UI: click slot to see/manage layers (from M4)

Right panel (inventory):
- [ ] 6. UI: render grid with item blocks sized by dimensions (w×h from BaseItemComponent)
- [ ] 7. UI: grid is fixed size (gridW × gridH), not scrollable — what you see is what you have
- [ ] 8. Weight stats bar below inventory grid — color-coded by threshold (ExtraWeight, Overweight, Immobile). Grid visual shows free/occupied cells in real time.

Interaction:
- [ ] 9. UI: drag items within grid to reorganize (mechanical impact — frees space for new items)
- [ ] 10. UI: drag from inventory → equipment slot (from M4)
- [ ] 11. First-fit auto-place algorithm (for right-click pickup / quick-store): scan grid left-to-right, top-to-bottom, place in first valid position. Used as fallback, not primary flow.
- [ ] 12. Right-click context menu on inventory items: [Equip] [Consume] [Drop] [Inspect] (from M4)

Polish:
- [ ] 13. Item inspection strip (bottom, full width): left = large item icon, center-left = name + description, center-right = stats (condition, weight, durability, grid size, type). Appears/updates on item click. Must work in all panel configurations (single inventory, inventory + container, container-to-container).
- [ ] 14. Optional "auto-sort" button: best-fit algorithm to compact items and maximize free space
- [ ] 15. Update `InventoryPresenter` to handle stack inspection (sub-lot breakdown via `BatchItem.GetSubLots()`)

**Decided**: No auto-placement as primary flow. Items enter the player's inventory by manual drag from world containers. The player decides where each item goes. Auto-sort and first-fit exist as convenience tools, not as the default path. This reinforces the realistic logistics theme.

**Decided**: Click-to-grab, click-to-place interaction (not drag & drop). Left click picks up a stack into the cursor. Clicking again places it. Overflow stays in cursor. Three placement cases:
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
- [ ] 4. Integration tests for full inventory flow (add, remove, transfer, equip, stack, inspect)
- [ ] 5. UI polish: drag feedback, placement preview, invalid placement indicator
- [ ] 6. Performance: stress test with large grids (cart/chest with many items)

**Note**: The old "organization bonus" concept is no longer needed — with grid-as-capacity, good organization is its own reward (more items fit). If a bonus mechanic is desired later, it can be added as a Phase 2+ feature.

---

## Design note — Composition-derived types

The `ItemType` enum currently acts as an explicit category. But with ECS composition, item type emerges naturally from which components an entity has: equippable = has `WearableComponent`, consumable = has `NutritionComponent`, weapon = has `DamageComponent`, etc. When implementing game logic, prefer querying component presence (`HasComponent<T>()`) over switching on `ItemType`. The enum can stay as UI metadata (inventory tab filters, icon badges) but should not drive mechanical decisions. This keeps the system open to new item archetypes without modifying enums or adding switch cases.

---

## Future (not Phase 1)

- [ ] 3D item preview in inventory UI
- [ ] Stack&Go full bridge (automated JSON export → item catalog)
- [ ] Save/load inventory state (serialization)
- [ ] Item tooltips with detailed stats
- [ ] Normalize `this.` usage — remove unnecessary `this.` references (underscore-prefixed fields make it redundant)
- [ ] Move `prototypes` dictionary out of `EntityManager` — entity creation should go through `PrototypeFactory`, not be managed internally by `EntityManager`
- [ ] Filtered consumption for crafting: `ConsumeFiltered(Predicate<ItemEntity> filter, int amount)` in `BatchItem` + wrapper in `InventorySystem`. Recipes need items matching not just typeId but specific state (e.g., hot iron ingot vs cold). `Equivalent()` may be too strict — evaluate whether a looser matching system is needed (partial match, predicate-based). Uses `BfsFindAll(typeId)` + filter per sub-lot. Additive, no structural refactor needed.
