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
- [x] **Erratas:** `ClearInveotryTabs` → `ClearInventoryTabs` (interfaz y vista),
  `_titelLabel` → `_titleLabel`, «transfere» → «transfer» (`InventorySystem`), «recieved» →
  «received»; el mensaje del Rigidbody de `BaseCameraStrategy`, en español. (El barrido marcó
  también «transferencia», que no es errata.)

## 3. Pendiente

### 3.1 Por decidir

Nada.

### 3.2 Por hacer

- **Mensajes de log y de excepción a español** (decidido; no hecho): hay unos 60 archivos con
  `Debug.Log`, `CoreLogger…Log` o `throw new`, muchos en inglés. Pasada aparte.
- **Restos de comentarios en inglés** en archivos donde no predominaba (el barrido solo marcó los
  23 con más inglés que español).
- **`HUDManager` y `HUDUtils`** (código muerto, con `fisiologic` en sus nombres): se resuelve al
  decidir si se borran (ver `BEM_RENOMBRES.md`, restos muertos).
