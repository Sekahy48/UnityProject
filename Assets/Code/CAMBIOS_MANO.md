# Cambios sobre el flujo de la mano (M5 T10)

Diagnóstico: **la vista estaba decidiendo lo que le toca decidir al presenter.**
De ahí salían los tres síntomas.

---

## Síntomas → causa

| Síntoma | Causa |
|---|---|
| Clic rápido no deja el item agarrado | `itemCard` agarraba en `PointerDown`, `items-layer` colocaba en `PointerUp` **siempre**. Un clic = down+up en el mismo punto → agarra y suelta en el mismo gesto. |
| Items que desaparecen y no vuelven | `PointerDown` sobre un bloque agarraba aunque la mano ya llevase algo → `Grab` pisaba el agarre anterior → si venía del panel dev, su `staging` quedaba sin referencias y el GC se lo llevaba con los items dentro. |
| Vista y modelo desincronizados | El clic fuera llamaba a `ClearHandBuffer()` (método de la **vista**): borraba el dibujo, el `HandBuffer` seguía agarrado. |

---

## Flujo: antes vs ahora

**Antes**

```
itemCard  PointerDown ──► OnGridItemGrabbed ──► presenter.GrabFrom
                     └──► view.RenderHandBuffer   (la vista se pinta sola)
items-layer PointerUp ──► OnPlaceGrabbedNode ──► presenter.Place   (sin preguntar nada)
root      PointerDown ──► view.ClearHandBuffer    (solo visual)
```

**Ahora**

```
itemCard              ──► (sin callbacks; burbujea a items-layer)
items-layer PointerDown ──► StopPropagation           "soy destino válido"
items-layer PointerUp   ──► OnGridCellClicked(row,col) "han clicado esta celda"
root        PointerDown ──► OnCancelRequested          "clic fuera"

presenter.OnGridCellClicked:
    mano llena  → PlaceAt(row,col)
    mano vacía  → GrabAt(row,col)
```

Regla: **ser destino válido = cortar la propagación.** Lo que no la corta llega a la raíz y cancela.

---

## Por archivo

### `InventoryView`

| Antes | Ahora |
|---|---|
| `event Action<int,int,int,ItemEntity> OnGridItemGrabbed` | *(eliminado)* |
| `event Action<int,int> OnPlaceGrabbedNode` | `event Action<int,int> OnGridCellClicked` |
| — | `event Action OnCancelRequested` |
| `itemCard.RegisterCallback<PointerDown>` (agarra + pinta + añade clase) | *(eliminado)* — los clics burbujean a `items-layer` |
| `itemsLayer` PointerDown: `StopPropagation` + TODO | `itemsLayer` PointerDown: solo `StopPropagation` |
| `itemsLayer` PointerUp → `OnPlaceGrabbedNode` | `itemsLayer` PointerUp → `OnGridCellClicked` |
| root PointerDown → `ClearHandBuffer()` | root PointerDown → `OnCancelRequested?.Invoke()` |
| `ReleaseGrabbedBlock(VisualElement)` | *(eliminado, nadie la llamaba)* |
| — | `if (item.IsGrabbed) itemCard.AddToClassList("item-block-grabbed")` |

**Consecuencia importante:** las coordenadas salen **siempre del cursor**, nunca del bloque que hubiera debajo.

### `GridItemDisplayData`

```csharp
public bool IsGrabbed;   // nodo origen de lo que lleva la mano → se dibuja atenuado
```

### `InventoryPresenter`

| Antes | Ahora |
|---|---|
| `OnGridItemGrabbed(row, col, amount, subLot)` | `GrabAt(row, col)` |
| `OnPlaceGrabbedNode(row, col)` | `PlaceAt(row, col)` |
| — | `OnGridCellClicked(row, col)` → despacha entre los dos |
| — | `OnCancelRequested()` → `EmptyHand()` + `ClearHandBuffer()` + `RenderInventory()` |

Detalles:

- `GrabAt` sale si `GetElementAt(row,col) == null` (celda vacía).
- `GrabAt` pinta la mano con **lo que `Grab` devolvió**, no con lo que mostraba el bloque (`Grab` clampa a lo disponible).
- `PlaceAt` refresca o limpia la mano según `IsHandCarrying()`.
- `RenderInventory` rellena `IsGrabbed` comparando cada nodo con `_service.GetGrabbedNode()`.

### `InventoryService`

```csharp
public int GrabFrom(...)          // antes void; devuelve lo agarrado de verdad
public ItemObject GetGrabbedNode() // nuevo, para marcar el bloque origen
```

### `HandBuffer.Grab`

```csharp
if (!IsEmpty())
    throw new InvalidOperationException("Cannot grab with a full hand. Place or clear it first.");
```

Con el flujo nuevo no debería saltar nunca (con la mano llena el clic es *colocar*). Está para que un fallo futuro sea ruidoso en vez de evaporar items.

---

## Pendiente / a revisar

1. **Drag & drop ya no funciona.** El agarre pasó a `PointerUp`, correcto para clic-para-agarrar (tu interacción primaria), pero no hay nada siguiendo al cursor con el botón pulsado. Es M5 T10 completo: umbral de ~5px en `PointerMove` + `CapturePointer`. No lo he tocado.
2. **`Close(bool absolute)`** llama a `EmptyHand()` siempre, incluso en la rama que decide *no* cerrar por llevar algo en la mano. Con `absolute = false` y la mano llena: no oculta la ventana pero sí te vacía la mano.
3. El popup de sub-slots sigue en `ClickEvent`; habrá que migrarlo a eventos de puntero cuando entre el drag (ya anotado en el doc de hitos).
