# Artisan — guía para trabajar en este repositorio

Juego de supervivencia y logística en Unity 6 (6000.1.17f1), URP. Es además el TFG de Sergio.

Antes de proponer nada, lee **`Assets/Code/FASE1_HITOS.md`**. Es la memoria de diseño del
proyecto: qué está hecho, qué falta, y —sobre todo— **por qué** cada decisión se tomó así y
qué alternativas se descartaron. Su sección «Where we are right now» dice en qué punto está
el trabajo; está escrita para el relevo entre conversaciones.

---

## La regla dura

**`Assets/Code/Core/` no puede importar `UnityEngine` ni `UnityEditor`.** Ni una línea, ni
para un `Vector3`, ni para un `Debug.Log`.

Core contiene el dominio completo: ECS, inventario, equipo, sistemas, presentadores. La capa
de Unity (`Assets/Code/Unity/`) contiene todo lo que necesita el motor: vistas, MonoBehaviours,
carga de recursos, sincronización de transforms.

Cuando Core necesita algo del motor, se define una **interfaz en Core** y la implementación
vive al otro lado. Ejemplos vivos: `IEntityLinker`, `ILogger`, `IReachFilter`.

Cuando Core necesita matemáticas que parecen de motor, se escriben en Core.
`PositionComponent.Forward()` y `WorldToLocal()` salen de un cuaternión con aritmética a
mano, precisamente para no importar nada.

---

## Arquitectura

**ECS propio, no DOTS.** `IEntity` con un `Dictionary<Type, IComponent>`. Los componentes son
clases con `Clone()` y `Equivalent()`. Las entidades se crean clonando prototipos de
`PrototypeFactory`, registrados en `EntityManager` por `EntityType` (enum, no cadenas).

**Sistemas.** Tres familias, registradas en `SystemManager` desde `GameMain`:
- `IPeriodicSystem` de juego: corren por tick del reloj.
- `IPeriodicSystem` de motor: corren por fotograma con deltaTime real.
- `IReactiveSystem`: reaccionan a `GameEventType` por el `EventBus`.

**Los sistemas no se conocen entre sí.** Cuando una operación necesita dos, la compone un
**servicio** (`InventoryService` es el precedente: `TryEquipItem` coordina `EquipmentSystem`
e `InventorySystem` sin que ninguno importe al otro).

**El inventario es un Composite.** `InventoryObject` (rama) / `ItemObject` (hoja, envuelve un
`BatchItem` de `SubLot`s). La rejilla tetris (`TetrisGridState`) **es** el sistema de
capacidad, no un adorno visual.

**MVC en `Core/MVC/`.** Los presentadores viven en Core y hablan con vistas a través de
interfaces; las vistas de UI Toolkit están en `Unity/MVC/View/`.

**Catálogo de items desde Stack&Go.** `data.json` en `StreamingAssets`, exportado por la app
Java de autoría (repo aparte). `JsonItemCatalogLoader` lo lee y `ItemComponentRegistry`
traduce el nombre de cada componente al tipo de Core.

---

## Principios que el código ya sigue

No son aspiraciones: están aplicados y hay cicatrices de cuando no lo estaban. Romperlos
produce fallos silenciosos, no errores de compilación.

**Paridad consulta/ejecución.** Si algo pinta un veredicto antes de actuar, la acción real
tiene que **empezar llamando a esa misma consulta**. `EquipItem` llama a `CanEquip`;
`PlaceAt` decide con el mismo `EvaluatePlacement` que colorea el fantasma. Las cuatro veces
que este proyecto ha pintado verde sin mover nada fue por preguntar una parte de la decisión,
o preguntarla en otro orden.

**Un hecho, un dueño.** Si un dato se puede deducir, no se guarda. Si se guarda en dos
sitios, algún día discreparán. Ejemplos de cómo se ha aplicado: `NumericFields<T>` declara
cada campo una vez y de ahí salen la lectura y la escritura; el volumen de interacción se
mide del modelo en vez de declararse en el catálogo; el arquetipo de entidad dejó de guardarse
como campo porque quien lo necesita ya lo tiene en la mano.

**Nombres, no banderas.** Una interfaz con nombre antes que un `Func` suelto (`IReachFilter`,
no `Func<IEntity, IEntity, bool>`). Un campo por hecho antes que un campo que cambia de
significado según el momento.

**Una acción que no puede cumplirse no se ofrece** — salvo que ocultarla deje al jugador sin
saber por qué no pasa nada. En el menú del inventario se oculta; en el mundo se ofrece y se
ejecuta moviendo cero con un aviso. La diferencia está razonada en `FASE1_HITOS.md`.

**Lo que decide un diseñador va a Stack&Go; lo que se deriva del asset se mide; lo que es
constante de ajuste vive en el código.** Peso y modelo son lo primero; el volumen de un objeto
es lo segundo; alcance de interacción y ángulo del cono son lo tercero.

---

## Estilo

**Comentarios en español, como documentación, no como consejo.** `<summary>` en los miembros
públicos que lo merezcan. Se documenta **por qué** algo es así y qué se descartó, no qué hace
la línea siguiente. Si un comentario se puede sustituir leyendo el código, sobra.

Los comentarios del repositorio **no llevan tildes** (el código ya existente es así; respétalo).
Los documentos `.md` sí las llevan.

**Nomenclatura.** Campos privados `_camelCase`; propiedades y métodos `PascalCase`. Hay
excepciones antiguas —algunos campos sin guion bajo— y están apuntadas como deuda; no las
propagues.

**Sin `this.` innecesario.** El guion bajo ya distingue el campo. Hay código antiguo que lo
usa; está apuntado como limpieza pendiente.

**Regiones.** `#region` para agrupar bloques largos dentro de una clase, como ya se hace en
`InventoryService` y `WorldInteractionSystem`.

---

## Cómo se trabaja aquí

**Sergio conduce.** Los comandos de git los lanza él: no ejecutes `git commit`, `git push` ni
nada que modifique el repositorio; si hace falta un mensaje de commit, escríbelo para que lo
copie.

**Sergio compila; tú no puedes.** No hay Unity en el entorno de trabajo, así que ningún
cambio está compilado ni probado cuando lo entregas. No lo intentes ni lo simules: al
terminar, di explícitamente qué no has podido verificar (tipos que asumes, firmas que no
has abierto, avisos esperables) para que él sepa dónde mirar cuando compile.

**Diseñar antes que escribir.** Cuando una tarea tenga decisiones dentro, plantéalas y espera
respuesta en vez de elegir por tu cuenta. Empújalo a pensar en lugar de darle la solución
hecha. Si una decisión te parece equivocada, dilo: sinceridad sin dorar y sin castigar.

**Con cada implementación de algo diseñado en conjunto, acompaña un pseudo diagrama** de flujo
o de secuencia, en ASCII, sin refinar pero legible.

**Verifica antes de afirmar.** Este proyecto ha encontrado varios fallos porque se comprobó el
fichero en vez de razonar sobre lo que debería contener. Antes de añadir un método a una clase,
haz grep del nombre: ya ha pasado duplicar un `Forward()` que existía cincuenta líneas más
abajo.

**Cuando tomes una decisión de diseño con su razonamiento, apúntala en
`Assets/Code/FASE1_HITOS.md`.** Ese documento es lo que permite que una conversación nueva
empiece sabiendo por qué las cosas son como son.

---

## Estructura

```
Assets/Code/
  Core/                     ← sin UnityEngine, jamás
    ECS/          Component, Entity, Systems
    Inventory/    Composite, rejilla, HandBuffer, SubLot
    Services/     InventoryService (coreógrafo)
    MVC/          Presenters y modelos de vista
    Item/         Catálogo, carga de JSON, registro de componentes
    Factories/    PrototypeFactory
  Unity/                    ← todo lo que toca el motor
    MVC/          Vistas de UI Toolkit, GameMain
    Services/     TextureCache, ModelCache
    UnityEntityLinker, TransformSyncSystem
  FASE1_HITOS.md            ← memoria de diseño. Empieza por aquí.

Assets/StreamingAssets/
  data.json, images/, models/   ← exportado desde Stack&Go, se lee en ejecución
```

---

## Cosas que sorprenden si no las sabes

- **La mano es una reserva, no una posesión.** Agarrar no mueve nada; la extracción y su
  vuelta atrás ocurren dentro de la misma llamada a `InventoryService.RunTransfer`.
- **`ItemEntity` es inmutable mientras está en un sub-lote.** Una misma instancia puede estar
  compartida por varias pilas; para cambiar el estado de unas unidades se clona.
- **Un contenedor no se cuenta a sí mismo** en el peso: su barra mide lo que le has metido, y
  su propio peso lo suma quien lo lleva.
- **Recorrer los slots de equipo no es recorrer las prendas puestas.** Una prenda de ocupación
  completa está registrada en varios slots; para contar se usa `EquipmentComponent.EquippedItems()`.
- **El catálogo se lee en ejecución desde `StreamingAssets`**, así que los modelos e iconos no
  son assets de Unity y no pasan por su importador.

La lista completa, con el razonamiento de cada una, está en la sección «Invariantes del
inventario» de `FASE1_HITOS.md`.
