# Phase 0: Codebase review, cleanup and Core/Unity separation

## Summary

Complete review and systematic fix of all inherited technical debt. The codebase is now cleanly separated into `Core/` (pure C#, no Unity dependencies) and `Unity/` (engine-specific implementations), connected via bridge interfaces.

## What changed

### Bug fixes
- Fixed `EntityManager` initialization order in `Logic`
- Simplified `GetComponent<T>()` signature (removed redundant overloads)
- Initialized `equipmentSlots` dictionary in `EquipmentComponent` constructor
- Fixed `FluidComponent.AddFluid()` logic error
- Removed duplicate `InputManager` creation in `GameMain.Awake()`
- Removed deprecated camera code in Strategy classes

### Architecture: Observer pattern
- `FatigueStaminaSystem` now extends `GenericSubject` instead of maintaining its own `ArrayList` — unified Observer usage across the project

### Architecture: Entity hierarchy
- Removed empty subclasses `AliveEntity` and `UnaliveEntity` (added no behavior)
- Kept `ItemEntity` (semantic type used by the entire inventory system)

### Architecture: Component split
- Split monolithic `FisiologicComponent` into three focused components:
  - `BodyComponent` — physical attributes (height, weight, age, sex)
  - `EnergyComponent` — stamina, fatigue, metabolic rate
  - `NutritionComponent` — hunger, thirst, macronutrients
- Extracted `CarryCapacity` as a static utility reading from multiple components

### Architecture: System loop + EventBus
- Created `EventBus` (pub/sub by `GameEventType`)
- Created `IGameSystem` interface + `SystemManager` with two tracks:
  - **Game systems**: tick-driven via `ClockSystem` (supports pause/timeSpeed)
  - **Engine systems**: frame-driven with real deltaTime
- Migrated `FatigueStaminaSystem` to the system loop
- `Logic` no longer manages systems; `InputManager` no longer invokes them manually

### Architecture: Core/Unity separation
- **Bridge interfaces** in `Core/`: `ILogger`, `IEntityLinker`
- **Unity implementations**: `UnityLogger`, `UnityEntityLinker`, `TransformSyncSystem`
- `PositionComponent` rewritten to pure C# (float storage + quaternion math for Forward/Right, dirty flag for sync)
- `MovementComponent` rewritten to pure C# (float primitives instead of Vector2)
- `PrototypeFactory`, `EntityManager`, `Logic` decoupled from `GameObject`
- `MovementSystem` deprecated, replaced by `TransformSyncSystem` (bidirectional Position ↔ Transform sync)

### Architecture: God Object elimination
- Split `GameContext` into three thematic sub-contexts:
  - `GameDataContext` — EntityManager
  - `GameSessionContext` — current player, clock
  - `GameSystemContext` — SystemManager, PresenterManager
- `GameController` and `InputManager` now receive only the dependencies they need via constructor injection (no more passing the entire context)

### Code quality
- `IdGenerator` made thread-safe (`Interlocked.Increment`)
- Removed `Thread.Sleep()` from `HealthComponent` (4 dangerous `*OverTime` methods — dead code)
- Extracted duplicated `HandleMovement` from FPS/TPS into `BaseCameraStrategy` (fixed TPS bug ignoring `CanRun()`)
- Migrated all `Debug.Log` in Core classes to `ILogger` injection
- Standardized all comments to English
- **Physical folder reorganization**: 65+ files moved into `Core/` and `Unity/` with .meta preservation

### New files
- `Core/ILogger.cs`, `Core/IEntityLinker.cs`
- `Core/Contexts/GameDataContext.cs`, `GameSessionContext.cs`, `GameSystemContext.cs`
- `Unity/UnityLogger.cs`, `Unity/UnityEntityLinker.cs`, `Unity/TransformSyncSystem.cs`

### Deleted
- `AliveEntity.cs`, `UnaliveEntity.cs` (empty subclasses)
- `FisiologicComponent.cs` (replaced by Body/Energy/Nutrition)
- `MovementSystem.cs` (replaced by TransformSyncSystem)
- Deprecated camera code and folder remnants

## File count
- **Core/**: 65 .cs files (pure C#, 0 UnityEngine references)
- **Unity/**: 21 .cs files (engine-dependent)

## Testing
- Verified in-game: player movement, camera switching, HUD, stamina system — all functional
- Grep verification: zero `using UnityEngine` in `Core/`, all internal `using` namespaces resolve correctly
