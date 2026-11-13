# Estándares de código — Artisan

Qué convención sigue el código y qué no la cumple todavía. La primera parte es la norma; la
segunda, lo que queda por cambiar.

**Revisión y limpieza: Claude, 2026-11-12**, con un barrido automático de `Assets/Code` y
`Assets/Tests` (expresiones regulares) repasado a mano. Puede quedar algún caso suelto que el
barrido no vea.

## 1. La norma (decidida el 2026-11-12)

| Qué | Convención | Ejemplo |
|---|---|---|
| Campo privado, también `static` o `readonly` | `_camelCase` | `_speed`, `_fields`, `_placements` |
| Lo visible fuera de la clase (`public`, `protected`, `internal`) | **propiedad** en PascalCase, no campo | `protected Rigidbody PlayerBody { get; }` |
| Valor `public static readonly` con nombre | PascalCase | `GridPos.None`, `MenuInputs.Empty` |
| Constante (`const`) | `MAYUSCULAS_CON_GUIONES` | `INTERACT_HOLD_TIME` |
| Métodos, clases, interfaces, enums | PascalCase; interfaces con `I` | `GetSpeed()`, `IReachFilter` |
| Variables locales y parámetros | camelCase | `deltaTime` |
| `this.` | solo cuando hace falta para desambiguar | — |
| Nombres de código | inglés | — |
| Comentarios | español, sin tildes (regla de `CLAUDE.md`) | — |
| Mensajes de log y de excepción | español (en el texto de un mensaje se pueden poner tildes) | — |

**La regla, en corto:** la visibilidad decide la forma; solo `const` tiene la suya.

**Excepciones aceptadas** (no son fallos):

1. **Campos `public readonly` en tipos de valor inmutables** (`GridPos`, `SubLot`, `CellSize`,
   `PanelPoint`, `MenuField`, `MenuOption`, `MenuContext`…): idiomático en C#.
2. **DTO de JSON cuyos nombres siguen al JSON** (`CatalogData`, `ItemData`… en
   `JsonItemCatalogLoader`, en minúscula): cambiarlos rompería la lectura del catálogo. Para
   pasarlos a PascalCase haría falta `[JsonProperty("nombre")]` en cada uno.
3. **Un struct que se modifica por partes** (`BaseCameraStrategy.Rotation`): sobre una propiedad
   `Rotation.x += …` no compila; se queda como campo `protected` en PascalCase, con comentario.
4. **Tipos privados anidados que solo son un paquete de datos** (`ItemComponentRegistry.Entry`,
   `NumericFields.Field`, `WorldInteractionSystem.Candidate`, `RadialMenu.Ring`,
   `UIElementUtils.Placement`): sus campos públicos no salen de la clase que los contiene.
5. **Valores fijos que no pueden ser `const`, en MAYÚSCULAS** (decidido el 2026-11-12):
   `static readonly` que se comportan como constantes (`WorldPresence.COMPONENTS`,
   `WorldInteractionPresenter.RING_SLOTS` y `SIDE_PANELS`, las propiedades de estilo de
   `RadialMenu`: `FILL`, `STROKE`, `INNER_RADIUS`…).

**Cuidado al renombrar** campos de un `MonoBehaviour` serializados en escena (`[SerializeField]`
en `GameMain`, `UIRegistry`): Unity pierde la referencia del Inspector. Los actuales ya cumplen;
si algún día hay que renombrar uno, con `[FormerlySerializedAs("viejo")]`.

### 1.1 El nombre dice el género de la clase (decidido el 2026-11-12)

Igual que `…Component`, `…System`, `…Service`, `…Presenter` o `…View`, **una clase de utilidad
(estática, sin estado propio) lleva en el nombre un sufijo que diga qué clase de cosa es.** Los
tipos del dominio (`InventoryObject`, `SubLot`, `ItemEntity`) no: su nombre ya es la cosa.

| Sufijo | Qué hace la clase | Ejemplos |
|---|---|---|
| `Calculator` | calcula un valor a partir de datos, sin efectos | `WeightCalculator` |
| `Resolver` | decide qué corresponde a algo (una ruta, una etapa) | `ItemModelResolver` |
| `Reader` | lee un valor nombrado de una entidad | — |
| `Registry` | tabla de consulta registrada al arrancar | `ItemComponentRegistry` |
| `Builder` | construye objetos o estructuras | `DisplayDTOsBuilder` |
| `Generator` | produce valores nuevos (ids) | `IdGenerator`, `NodeIdGenerator` |
| `Binder` | engancha y desengancha relaciones entre objetos | — |
| `Rules` | escribe una regla del dominio en un solo sitio y la aplica o comprueba | — |
| `Paths` | catálogo de rutas constantes | — |
| `Extensions` | métodos de extensión | `EnumExtensions` |
| `Checker`, `Config`, `Logger` | lo que dicen | `ArgumentChecker`, `CoreConfig`, `CoreLogger` |
| `Utils` | **último recurso**, si ninguno encaja | `UIElementUtils` |

Una clase que hoy es estática pero «se volverá un sistema» lleva el sufijo de lo que es hoy; al
volverse sistema, se renombra.

### 1.2 Avisos a los que escucha un sistema reactivo (decidido el 2026-11-13)

```csharp
public IEnumerable<GameEventType> SubscribedEvents { get; } = new[] { GameEventType.WeightChanged };
public IEnumerable<GameEventType> SubscribedEvents { get; } = Array.Empty<GameEventType>();   // ninguno
```

Con `{ get; } =` la lista se crea una vez. **No** `=> new[] { ... }`: la flecha se vuelve a
ejecutar en cada lectura y crea un array cada vez. Sobra el campo `static readonly` aparte
(cada sistema tiene una sola instancia).

### 1.3 Qué hacer cuando llega null (decidido el 2026-11-13)

**La regla:** si null es un fallo de quien llama, se lanza con `AC.CheckNotNull(x, nameof(x))`
(`using AC = Core.Utils.ArgumentChecker;`); si null significa algo, se maneja con `return` o
`?.` y se dice en el comentario qué significa.

| Caso | Qué se hace |
|---|---|
| Parámetro obligatorio de un método público | `AC.CheckNotNull` al principio |
| Dependencias de un constructor (contextos, servicios, sistemas) | `AC.CheckNotNull`: falla al arrancar, no a mitad de partida |
| `GameEvent` en `UpdateOnEvent` | `AC.CheckNotNull`: el bus nunca publica null |
| Entidad del aviso, si ese aviso siempre la lleva | `AC.CheckNotNull` |
| Componente opcional (`GetComponent` que puede no estar) | `return`: «no lo tiene» |
| Parámetro declarado opcional (`= null`) | nada que comprobar: está documentado |
| Objeto de Unity destruido tras un `await` | `return`: es un estado real |
| Métodos privados | **sin comprobación**: la clase ya filtró en su entrada pública |
| Datos de fuera (catálogo) | **lanzar al cargar**, con todos los fallos juntos. Excepción: los ficheros de modelo que faltan solo avisan (despiste al copiar datos, no catálogo mal escrito) |

---

## 2. Hecho el 2026-11-12

Core compilado y 56 tests en verde con .NET fuera de Unity. **Lo de `Unity/` no se ha podido
compilar aquí:** hay que abrir Unity y comprobar que compila.

- [x] **Campos privados a `_camelCase`** en todo el repositorio (componentes del cuerpo, energía,
  nutrición, fluidos, curación, vida, recursos y tareas; reloj, gestor de sistemas, bus de
  eventos, eventos, ids, presentadores; cámaras, registro de cámaras, gestor de vistas, mapas).
  Hecho con un renombrado que respeta los parámetros que se llaman igual que el campo; se
  comprobó que ningún método tenía una variable local con el mismo nombre.
- [x] **`private static readonly` a `_camelCase`:** `_fields` (componentes con campos numéricos),
  `_entries`, `_loadLevels`, `_loadClasses`, `_loadLabelClasses`, `_placements`, `_shakes`,
  `_random`, `_loadBands` (test).
- [x] **Campos `protected` a propiedades o privados:** `BaseCameraStrategy` (`Camera`,
  `PlayerObject`, `PlayerBody`, `Player`, `Animator`), `BasicComponent._name` → propiedad `Name`,
  `InGameEntity.id` / `components` y `GenericSubject.observers` → privados (ninguna subclase
  los usaba).
- [x] **Datos de vista con campos públicos a propiedades `{ get; set; }`:** `GridItemDisplayData`,
  `ItemDisplayData`, `TabDisplayData`, `RadialOption`, `InspectPanelData`, `WorldPromptData`.
- [x] **`this.` innecesario** quitado en todo el repositorio (estaba en Future de
  `FASE1_HITOS.md`).
- [x] **Comentarios en inglés traducidos** en los 23 archivos donde predominaba el inglés (estaba en
  Future de `FASE1_HITOS.md`).
- [x] **Constantes:** `BaseItemComponent._maxDurability` / `_maxCondition` → `MAX_DURABILITY` /
  `MAX_CONDITION`; `HUDView.DEFAULT_STATUS_CLASS` pasa a `const`.
- [x] **Renombres por la norma 1.1:** `ItemWeight` → `WeightCalculator`, `CarryCapacity` →
  `CarryCapacityCalculator`, `ItemMagnitudes` → `ItemMagnitudeReader`, `WornContainers` →
  `WornContainerBinder`, `WorldPresence` → `WorldPresenceRules`, `RadialGeometry` →
  `RadialGeometryCalculator`, `UIImages` → `UIImagePaths` (archivos y `.meta` incluidos).
- [x] **Cada clase a su carpeta** (el espacio de nombres sigue a la carpeta; `using` ajustados):
  `CarryCapacityCalculator` y `PlacementVerdict` → `Core/Inventory`; `WorldPresenceRules`,
  `WorldAction` e `IReachFilter` → `Core/World` (nueva); `EntityManager` y
  `EntityComponentException` → `Core/ECS`; `EnumExtensions` → `Core/Utils`, con
  `namespace Core.Utils` (estaba en el global). Pueden quedar `using Core.ECS.Systems;` sin uso
  en archivos que solo lo tenían por `EntityManager`: no molestan.
- [x] **`SubscribedEvents` a la forma 1.2** (2026-11-13) en `EquipmentSystem`,
  `InventorySystem`, `MovementSystem` y `WorldInteractionSystem` (`MassSyncSystem` ya nació así).
- [x] **Norma 1.3, primera tanda** (2026-11-13): `AC.CheckNotNull(gameEvent)` en todos los
  `UpdateOnEvent` con cuerpo; dependencias de los constructores de contextos, `EntityManager`,
  `SystemManager` (y sus `Register…`), `TaskSystem`, `InventoryService` y presentadores de
  inventario; `EventBus.Subscribe/Unsubscribe/Post`. Catálogo (`JsonItemCatalogLoader`) y mapa
  de ids (`TypeIdMapper`) lanzan al cargar. `ServiceRig` (tests) dejó de pasar null a
  `SystemManager`.
- [x] **Fallo de la limpieza, corregido el 2026-11-13:** el renombrado convirtió el constructor
  de `EntityId` en `EntityId(int _id) { _id = _id; }`. Todas las entidades tenían id 0 y el
  registro de `EntityManager` (indexado por id) se quedaba solo con la última: el jugador
  dejaba de sincronizar su posición en el Core al soltar el primer objeto. Test
  `EntityIdTests`. Buscadas otras autoasignaciones (`x = x;`) en todo el repositorio: ninguna.
- [x] **Erratas:** `ClearInveotryTabs` → `ClearInventoryTabs` (interfaz y vista),
  `_titelLabel` → `_titleLabel`, «transfere» → «transfer» (`InventorySystem`), «recieved» →
  «received»; el mensaje del Rigidbody de `BaseCameraStrategy`, en español. (El barrido marcó
  también «transferencia», que no es errata.)

## 3. Pendiente

### 3.1 Por decidir

Nada.

### 3.2 Por hacer

- **Norma 1.3, segunda tanda:** métodos públicos de juego (`InventoryService`,
  `WorldInteractionService`, `InventorySystem`, `EquipmentSystem`, `InventoryObject`,
  componentes…). Un escáner (`~/bem/nullscan.py` en la VM de Claude) cuenta unos 230
  parámetros sin comprobar en el Core y 67 en Unity, con falsos positivos (enums, structs).
  **No se meten a ciegas:** desde la interfaz puede llegar null a propósito (no hay destino bajo
  el ratón, por ejemplo), y lanzar ahí rompería el juego en caminos que los tests no cubren.
  Hay que mirar quién llama a cada uno y hacerlo por archivos, con Unity abierto para probar.
- **`EquipmentSlot`** comprueba con `AC.CheckNotNull` un parámetro que es un enum (nunca es
  null): sobra.


- **Mensajes de log y de excepción a español** (decidido; no hecho): hay unos 60 archivos con
  `Debug.Log`, `CoreLogger…Log` o `throw new`, muchos en inglés. Pasada aparte.
- **Restos de comentarios en inglés** en archivos donde no predominaba (el barrido solo marcó los
  23 con más inglés que español).
- **`HUDManager` y `HUDUtils`** (código muerto, con `fisiologic` en sus nombres): se resuelve al
  decidir si se borran (ver `BEM_RENOMBRES.md`, restos muertos).
