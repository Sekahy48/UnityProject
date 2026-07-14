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
- [ ] 8. Refactor `IInventoryElement` — `GetId()` returns int (typeId) instead of string name
- [ ] 9. Update `InventoryObject` BFS methods to use typeId
- ~~10. Update `ItemObject` accordingly~~ (unnecessary — ItemObject is replaced by BatchItem in M2)
- [ ] 11. Update `PrototypeFactory` to create items from catalog
- [ ] 12. Create test JSON with sample items

**Decided**: No separate `ItemDefinition` class. `ItemCatalog` stores `ItemEntity` prototypes with all their components pre-assembled. Creating a new item = `catalog.CreateItem(typeId)` which clones the prototype. Data is duplicated per instance (acceptable trade-off for simplicity).

---

## Milestone 2 — Sub-lot stacking

**Goal**: Items stack visually but maintain internal sub-lots for different states.

**Tasks**:

- [ ] 1. Replace `ItemObject` with `BatchItem` — ALL leaf nodes are now `BatchItem`. Contains `List<(ItemEntity, int)>` sub-lots. A unique item (key, letter) is a BatchItem with one sub-lot of amount 1. `ItemObject` is deleted.
- [ ] 2. Tree simplification: only 2 node types remain — `InventoryObject` (branch/container) and `BatchItem` (leaf, always). `IInventoryElement` interface unchanged.
- [ ] 3. Equivalence-based grouping: items with same typeId live in the same BatchItem, sub-lots split by property differences (durability, condition, enchants)
- [ ] 4. `AddItem` by default adds to the first compatible BatchItem found. Creates new sub-lot if same type but different state, new BatchItem if different type. Multiple BatchItems of the same type are allowed (player can manually split stacks or keep separate piles). Total amount in a BatchItem cannot exceed `BaseItemComponent.maxStackSize`.
- [ ] 5. `Consume(n)` picks random sub-lot (not FIFO, not alphabetical)
- [ ] 6. `InspectStack()` returns the sub-lot breakdown for UI
- [ ] 7. `GetTotalAmount()` sums across sub-lots
- [ ] 8. `GetTotalWeight` sums across sub-lots using per-entity weights (volume calculation removed — grid handles capacity)
- [ ] 9. Create `ItemStateComponent` in Core — tracks item state (fresh, rotten, damaged, etc.) and decay rate. This is what differentiates sub-lots within a BatchItem (a rotten apple vs a fresh one). Decay logic itself is Phase 3, but the component and its data must exist now for sub-lot equivalence checks to work.
- [ ] 10. Update `InventoryPresenter` to handle stack inspection

**Decided**: Unified leaf node — no distinction between "single item" and "stack". Everything is a `BatchItem`. Eliminates special cases in composite tree logic.

**Decided**: Pulling items out of a stack via inspect creates a new BatchItem in the same inventory. It's a real data operation, not just visual.

---

## Milestone 3 — Weight & grid space enforcement

**Goal**: AddItem checks capacity limits and rejects/penalizes when exceeded.

**Tasks**:

- [ ] 1. Refactor `StorageComponent` — replace `maxVolume` (float) with `gridW` and `gridH` (int). Grid dimensions define capacity. `maxWeight` stays as float.
- [ ] 2. Create `TetrisGridState` in Core — data structure that tracks which cells are occupied and by which BatchItem. Grid dimensions come from `StorageComponent.gridW × gridH`. Core only, no UI — rendering is M5.
- [ ] 3. `AddItem` / `StackOnto` check grid space (via TetrisGridState: can the item's dimensions fit in remaining free cells?) and `StorageComponent.maxWeight` before adding
- [ ] 4. `CarryCapacity` enforcement: total inventory weight vs character carry capacity
- [ ] 5. Two capacity systems:
   - **Grid space** (hard limit): No free cells that fit the item's dimensions → transfer rejected. Partial if stackable and an existing BatchItem has room under maxStackSize. Notify player: inventory/container full.
   - **Weight** (two thresholds): Soft = transfer allowed but debuff (movement speed reduction, notify overloaded, health consequences stub for Phase 2). Hard (immobile) = transfer rejected entirely, too heavy to move.
- [ ] 6. Return enum `TransferResult { Success, PartialStack, GridFull, Overloaded, Immobile }` — Success: all moved. PartialStack: some stacked onto existing BatchItem, rest didn't fit. GridFull: no space at all. Overloaded: moved but soft weight exceeded. Immobile: rejected, hard weight limit.
- [ ] 7. `InventorySystem` fires events: `INVENTORY_FULL`, `OVERWEIGHT`, `IMMOBILE`
- [ ] 8. Weight debuff integration: overweight → speed multiplier in `MovementComponent` (stub health effects for Phase 2)
- [ ] 9. UI: weight bar + grid visual (free/occupied cells) update in real time, color-coded by threshold

**Decided**: Grid space is a hard limit (reject/partial). Weight has two thresholds — soft (debuff, allowed) and hard (reject, immobile). Health consequences of overload are stubbed as interface for Phase 2.

---

## Milestone 4 — Equipment system overhaul

**Goal**: Equipment panel with ordered layers, drag & drop, slot disabling.

**Tasks**:

- [ ] 1. `EquipmentSlot` — ensure List<ItemEntity> is explicitly ordered (index 0 = outermost layer)
- [ ] 2. Add `enabled` flag to `EquipmentSlot` (default true, false = amputated/injured) and `maxLayers` int (hard cap on stacked items per slot)
- [ ] 3. Create `ClothingComponent` (rename TBD: `WearComponent` or `WearableComponent`) in Core — determines which `EquipmentSlotType` a wearable item targets. An item is equippable if and only if it has this component (ECS-idiomatic: `HasComponent<ClothingComponent>()`). Fields: targetSlot (EquipmentSlotType), topLayer (bool — if true, nothing can be equipped on top of this item in that slot), garmentCategory (enum: Shirt, Vest, Plate, Robe, Glove, Boot, Helmet, Hood, Satchel... — extensible), layerOrder (int, for Phase 2 damage penetration ordering).
- [ ] 4. Add layer validation: can this item go in this slot? (check ClothingComponent.targetSlot)
- [ ] 5. Equipment changes fire `EQUIPMENT_CHANGED` event
- [ ] 6. Equipping an item removes it from inventory, unequipping adds it back
- [ ] 7. UI: render 3×4 equipment grid with layer indicators
- [ ] 8. UI: click slot to see/manage layers
- [ ] 9. UI: drag from inventory → equipment slot
- [ ] 10. Right-click context menu on inventory items: [Equip] [Consume] [Drop] [Inspect]

**Decided**: Equipment grid is 3×4. Layout:
```
(L.Shoulder)  (Head)    (R.Shoulder)
(L.Hand)      (Torso)   (R.Hand)
(reserved)    (Legs)    (reserved)
(reserved)    (Feet)    (reserved)
```
Shoulders: bags, backpacks (1 shoulder = satchel, both = backpack). 4 reserved slots use the same `enabled=false` mechanism as amputation — unlocked when gameplay needs them (belt, cloak, etc). `EquipmentSlotType` enum needs updating to add shoulders and reserved.

**Decided**: Layer order is validated. Two rules:
1. **topLayer** (component data): `ClothingComponent` has a `topLayer` bool. Rigid/structured items (armor, chestplate) are topLayer — nothing can be equipped on top of them. Covers: no shirt over chestplate, no armor over armor.
2. **No duplicate garment category** (system logic, NOT component data): `InventorySystem` enforces that you can't equip two items with the same `garmentCategory` in the same slot. Iron plate armor and studded leather armor are both `Plate` → can't stack. A shirt and a camisole are both `Shirt` → can't stack. One Shirt + one Vest + one Plate = OK. System-level validation, can be relaxed in the future if needed.
- `maxLayers` on `EquipmentSlot` remains as a hard safety cap.
- Equip order: new items always go on top (outermost). To change order, unequip and re-equip.

---

## Milestone 5 — Tetris grid UI & split view

**Goal**: Inventory displayed as tetris grid where grid IS the capacity system. Split layout with personal panel.

**Tasks**:

- [ ] 1. `TetrisGridState` already exists from M3. This milestone adds the UI rendering and interaction on top of it.
- [ ] 2. First-fit auto-place algorithm (for right-click pickup / quick-store): scan grid left-to-right, top-to-bottom, place in first valid position. Used as fallback, not primary flow.
- [ ] 3. UI: render grid with item blocks sized by dimensions (w×h from BaseItemComponent)
- [ ] 4. UI: drag items within grid to reorganize (mechanical impact — frees space for new items)
- [ ] 5. UI: grid is fixed size (gridW × gridH), not scrollable — what you see is what you have
- [ ] 6. Split view layout: left panel (personal) + right panel (inventory)
- [ ] 7. Health placeholder in personal panel
- [ ] 8. Weight stats bar below inventory grid
- [ ] 9. Item inspection strip (bottom, full width): left = large item icon, center-left = name + description, center-right = stats (condition, weight, durability, grid size, type). Appears/updates on item click. Must work in all panel configurations (single inventory, inventory + container, container-to-container).
- [ ] 10. Optional "auto-sort" button: best-fit algorithm to compact items and maximize free space

**Decided**: No auto-placement as primary flow. Items enter the player's inventory by manual drag from world containers. The player decides where each item goes. Auto-sort and first-fit exist as convenience tools, not as the default path. This reinforces the realistic logistics theme.

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

The `ItemType` enum currently acts as an explicit category. But with ECS composition, item type emerges naturally from which components an entity has: equippable = has `ClothingComponent`, consumable = has `NutritionComponent`, weapon = has `DamageComponent`, etc. When implementing game logic, prefer querying component presence (`HasComponent<T>()`) over switching on `ItemType`. The enum can stay as UI metadata (inventory tab filters, icon badges) but should not drive mechanical decisions. This keeps the system open to new item archetypes without modifying enums or adding switch cases.

---

## Future (not Phase 1)

- [ ] 3D item preview in inventory UI
- [ ] Stack&Go full bridge (automated JSON export → item catalog)
- [ ] Save/load inventory state (serialization)
- [ ] Item tooltips with detailed stats
