# Refactor a BEM — tabla de renombres

Hecho el 2026-10-28 en la rama `bem` (Claude). Convención: `bloque`, `bloque__elemento`, `bloque--modificador`, todo en kebab-case. Los prefijos cortos ya asentados se mantienen (`wi-` interacción con el mundo, `ip-` panel de inspección, `ctx-menu`). Las clases utilitarias, que no pertenecen a ningún componente, llevan el prefijo `u-`.

Los `name=` de los UXML **no cambian** salvo los del HUD (que no estaban en kebab-case): `name` sirve para encontrar desde C# (`Q("…")`) y la clase para dar estilo, así que un elemento puede llamarse `weight-label` y tener la clase `weight-rail__label`.

## Clases

### Ventana de inventario

| Antes | Después |
|---|---|
| `left-tabs-bar` | `left-tabs` |
| `left-tab` | `left-tabs__tab` |
| `title-label` | `title-bar__label` |
| `dev-button` | `title-bar__dev-button` |
| `close-button` | `title-bar__close` |
| `content-area` | `main-area__content` |
| `left-panel` | `player-panels__left` |
| `rightside-container` | `player-panels__right` |
| `internal-personal-panel` | `player-panels__section` |
| `equipment-cross` | `equipment-panel__cross` |
| `equip-row` | `equipment-panel__row` |
| `equip-spacer` | `equipment-panel__spacer` |
| `equip-side` | `equipment-panel__side` |
| `panel-slot-tabs-label` | `panel-slot-tabs__label` |
| `inspect-icon` | `inspection-strip__icon` |
| `inspect-info` | `inspection-strip__info` |
| `inspect-name` | `inspection-strip__name` |
| `inspect-description` | `inspection-strip__description` |
| `inspect-stats` | `inspection-strip__stats` |
| `inspect-stat` | `inspection-strip__stat` |
| `layers-button` | `equip-slot__layers-button` |
| `side-panel` | `side-panels__panel` |
| `panel-content` | `side-panels__content` |
| `catalog-search` | `catalog__search` |
| `catalog-scroll` | `catalog__scroll` |
| `catalog-row` | `catalog__row` |
| `catalog-row-label` | `catalog__row-label` |
| `row-amount` | `catalog__amount` |
| `add-button` | `catalog__add-button` |
| `tooltip-text` | `tooltip__text` |
| `hand-buffer-icon` | `hand-buffer__icon` |
| `hand-buffer-instant` | `hand-buffer--instant` |
| `hand-buffer-fits` | `hand-buffer--fits` |
| `hand-buffer-partial` | `hand-buffer--partial` |
| `hand-buffer-swap` | `hand-buffer--swap` |
| `hand-buffer-collision` | `hand-buffer--collision` |
| `item-block-grabbed` | `item-block--grabbed` |
| `icon-fit` | `u-icon-fit` |
| `alegreyaSansSC` | `u-font-alegreya` |
| `beigeColor` | `u-color-beige` |
| `weight-bar` | `weight-rail__bar` |
| `weight-label` | `weight-rail__label` |
| `weight-label-note` | `weight-rail__label--note` |
| `load-normal` | `weight-rail__bar--normal` |
| `load-extra` | `weight-rail__bar--extra` |
| `load-over` | `weight-rail__bar--over` |
| `load-immobile` | `weight-rail__bar--immobile` |
| `load-normal-label` | `weight-rail__label--normal` |
| `load-extra-label` | `weight-rail__label--extra` |
| `load-over-label` | `weight-rail__label--over` |
| `load-immobile-label` | `weight-rail__label--immobile` |
| `grid-mount` | `inventory-panel__grid-mount` |
| `inventory-grid-row` | `inventory-grid__row` |
| `inventory-grid-cell` | `inventory-grid__cell` |
| `ctx-menu-option` | `ctx-menu__option` |
| `ctx-menu-option-selected` | `ctx-menu__option--selected` |
| `ctx-menu-option-label` | `ctx-menu__label` |
| `ctx-menu-widget` | `ctx-menu__widget` |
| `ctx-submenu` | `ctx-menu--submenu` |
| `ctx-submenu--flipped` | `ctx-menu--flipped` |
| `sublots-popup-row` | `sublots-popup__row` |
| `sublots-popup-row-label` | `sublots-popup__label` |

### Panel de inspección (`ip`)

| Antes | Después |
|---|---|
| `ip-title-bar` | `ip__title-bar` |
| `ip-close` | `ip__close` |
| `ip-body` | `ip__body` |
| `ip-header` | `ip__header` |
| `ip-icon` | `ip__icon` |
| `ip-identity` | `ip__identity` |
| `ip-name` | `ip__name` |
| `ip-type` | `ip__type` |
| `ip-description` | `ip__description` |
| `ip-divider` | `ip__divider` |
| `ip-stats` | `ip__stats` |
| `ip-stat-row` | `ip-stat` |
| `ip-stat-key` | `ip-stat__key` |
| `ip-stat-value` | `ip-stat__value` |
| `ip-lots-section` | `ip__lots-section` |
| `ip-section-label` | `ip__section-label` |
| `ip-lots` | `ip__lots` |
| `ip-footer` | `ip__footer` |
| `ip-lot-row` | `ip-lot` |
| `ip-lot-icon` | `ip-lot__icon` |
| `ip-lot-name` | `ip-lot__name` |
| `ip-lot-amount` | `ip-lot__amount` |
| `ip-lot-weight` | `ip-lot__weight` |

### Interacción con el mundo (`wi`)

| Antes | Después |
|---|---|
| `wi-row` | `wi-prompt__row` |
| `wi-key` | `wi-prompt__key` |
| `wi-action` | `wi-prompt__action` |
| `wi-hint` | `wi-prompt__hint` |
| `wi-target` | `wi-prompt__target` |
| `wi-more` | `wi-prompt__more` |
| `wi-reticle-core` | `wi-reticle__core` |

### HUD

| Antes | Después |
|---|---|
| `hudContainer` | `hud` |
| `barsContainer` | `hud__bars` |
| `statesBar` | `hud__states` |
| `fatigueBar` | `hud-bar--fatigue` |
| `staminaBar` | `hud-bar--stamina` |
| `fill` | `hud-bar__fill` |

### Clases nuevas (sin equivalente anterior)

| Clase | Dónde | Por qué |
|---|---|---|
| `hud-bar` | las dos barras del HUD | bloque base: el modificador (`hud-bar--fatigue`) no va nunca solo |
| `catalog` | `item-catalog` | bloque de los elementos `catalog__*` |
| `ip__title` | título del panel de inspección | sustituye al anidado `.ip-title-bar .title-label` (ver excepciones) |
| `tooltip__text` en el HUD | textos del tooltip de estados (`HUDView.TooltipText`) | sustituye al selector por tipo `.tooltip Label` de los temas |

### Sin cambios

Ya eran bloques válidos o ya estaban en BEM: `ui-root`, `inventory-root`, `main-area`, `title-bar`, `player-panels`, `equipment-panel`, `equip-slot`, `panel-slot`, `panel-slot-tabs`, `inspection-strip`, `layers-popup`, `side-panels`, `tooltip`, `hand-buffer`, `items-layer`, `item-block`, `item-icon`, `amount-label`, `section-label`, `inventory-panel`, `weight-rail`, `inventory-grid`, `ctx-menu`, `sublots-popup`, `inspect-panel`, `ip-root`, `wi-root`, `wi-prompt`, `wi-prompt--disabled`, `wi-reticle` (y sus `--active`/`--hidden`), todo el radial (`radial-*`), `status` y `status--weight-N`.

Marcadores sin estilo que enlazan pestaña y panel (`InventoryView.GetRelationClass`): `equipment-related`, `health-related`. Se quedan como están.

## Names (solo HUD)

| Antes | Después |
|---|---|
| `Container` | `hud-root` |
| `BarsContainer` | `hud-bars` |
| `FatigueBar` | `fatigue-bar` |
| `StaminaBar` | `stamina-bar` |
| `Fill` | `fill` |
| `StatesBar` | `states-bar` |

## Selectores anidados que se quedan (excepciones)

1. `.wi-prompt--disabled .wi-prompt__action`: modificador del bloque que afecta a su elemento (BEM lo permite).
2. `.hud-bar--fatigue .hud-bar__fill` y `.hud-bar--stamina .hud-bar__fill`: mismo caso.
3. `.ctx-menu__widget .unity-…` y `#unity-text-input`: clases internas de Unity, no se pueden renombrar.
4. `.side-panels__panel .inventory-panel__grid-mount` (tema medieval): la rejilla no sabe en qué hueco está montada. Aplanarlo pediría que la vista del panel pusiera un modificador según su hueco.

`.title-bar__label.ip__title` y `.hand-buffer.hand-buffer--fits` (y demás veredictos) son **dobles clases en el mismo elemento**, no anidados: se usan para ganar especificidad sin depender del orden de las hojas.

## Bug corregido de paso

`InventoryPanelView` buscaba `Q<Label>("title-bar-panel")`, pero el UXML lo llama `title-label-panel`: devolvía null y `_titelLabel.text = …` (línea ~513) habría lanzado una excepción si se ejecutaba. Corregido. **Puede que ahora aparezca el título en paneles donde antes no salía.**

## Restos muertos sin tocar (para decidir)

- `UI/MainMenu.uxml` + `UI/main-menu.uss`: nada los referencia. `UI Toolkit/MainMenuVisualTree.uxml` + `MainMenuStyle.uss`: los usa el GameObject `MainMenu` de `MainScene`, que está **desactivado**.
- `UI Toolkit/Inventory/uss/EquipmentPanel.uss`: no lo carga ningún UXML ni código. Además usa `z-index` y `cursor: pointer`, que USS no admite.
- `HUDManager.cs` y `HUDUtils.cs`: nadie los usa desde el refactor del HUD. `HUDUtils` aún busca `"Fill"`, que ya no existe.
- Clases del orbe en `PlayerHUDStyle.uss` (`orbfill`, `bloodOrb`, `emptyOrb`): se guardaron a propósito para cuando vuelva con las heridas. Si vuelven, pasarlas a BEM entonces.
- `.sublot-icon` en `InventoryWindow.uss`: ninguna vista la usa.

## Verificación

Script de comprobación cruzada antes y después: cada `Q("…")` tiene su `name`, cada clase de UXML/C# tiene regla USS y cada regla USS se usa. Diferencias: desaparece `title-bar-panel` (el bug), aparece `Fill` (solo en `HUDUtils`, muerto) y aparecen `catalog` y `hud-bar` sin regla (bloques sin estilo propio, a propósito).

**No compilado ni probado en Unity.** Pantallas a revisar: inventario (rejilla, mano y sus colores, menú contextual con submenú, popup de sublotes, capas, pestañas izquierdas, tira de inspección, barra de peso con sus cuatro colores y la nota), catálogo de desarrollo y su tooltip, paneles laterales, panel de inspección, marca de la E (activa, desactivada con motivo), radial y HUD (barras y tooltip de estados). Los dos temas.
