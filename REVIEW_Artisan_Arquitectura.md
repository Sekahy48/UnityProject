# Revisión Arquitectónica — Artisan (Iltur)

**Fecha:** 5 de julio de 2026  
**Alcance:** Arquitectura general, calidad del código, diseño de sistemas  
**Archivos analizados:** 89 archivos .cs bajo `Assets/Code/`

---

## 1. VEREDICTO GENERAL

La base arquitectónica es **sólida para un proyecto de un solo desarrollador en fase temprana**. Hay separación clara de responsabilidades, uso coherente de patrones (ECS propio, Observer, Strategy, MVP), y el objetivo declarado de desacoplarse de Unity se cumple parcialmente. Sin embargo, hay problemas concretos que conviene abordar antes de que el proyecto escale.

**Puntuación por área:**

| Área | Nota | Comentario |
|------|------|-----------|
| Estructura de carpetas | 8/10 | Clara y bien organizada |
| Desacoplamiento de Unity | 6/10 | Buen intento, pero con filtraciones importantes |
| Coherencia de patrones | 6/10 | Patrones bien elegidos, aplicación irregular |
| Calidad de código | 5/10 | Inconsistencias de estilo, dead code, bugs latentes |
| Diseño de sistemas | 7/10 | Inventario muy bien pensado; resto incompleto pero con base |

---

## 2. ARQUITECTURA GENERAL

### 2.1 Estructura de carpetas

```
Code/
├── ECS/           ← Núcleo: componentes, entidades, sistemas
│   ├── Component/ ← Datos puros (mayoritariamente)
│   ├── Entity/    ← Contenedores de componentes
│   └── Systems/   ← Lógica que opera sobre entidades
├── MVC/           ← Capa de presentación y control
│   ├── Controller/← GameMain, GameContext, InputManager
│   ├── Model/     ← Logic (puente ECS↔MVC)
│   ├── Presenter/ ← MVP para UI (inventario)
│   └── View/      ← Vistas, HUD, UIRegistry
├── Events/        ← Sistema de eventos (embrionario)
├── Factories/     ← PrototypeFactory
├── Handler/       ← IHandler, EntityId, NameId
├── Inventory/     ← Sistema de inventario árbol (Composite)
├── Observer/      ← Interfaces Observer/Subject
├── Strategy/      ← Estrategias de cámara
└── Utils/         ← ArgumentChecker
```

**Positivo:** La separación en módulos es clara. Cada carpeta tiene una responsabilidad identificable. El código no está monolítico.

**Problema:** No hay una separación física real entre "código puro C#" y "código que depende de Unity". Todo convive bajo el mismo directorio. Para el TFG y para el objetivo de desacoplamiento, convendría una estructura tipo:

```
Code/
├── Core/          ← C# puro, 0 dependencias Unity
│   ├── ECS/
│   ├── Inventory/
│   ├── Events/
│   └── Observer/
├── Unity/         ← Todo lo que toca UnityEngine
│   ├── Components/ (UnityEntityComponent, PositionComponent)
│   ├── MVC/
│   └── Strategy/
```

### 2.2 Desacoplamiento de Unity — Análisis honesto

El objetivo declarado es "usar Unity solo como base, con sistemas propios". Esto se logra **parcialmente**:

**Bien desacoplado (C# puro):**
- `IComponent`, `IEntity`, `InGameEntity` — no dependen de Unity
- `InventoryObject`, `ItemObject`, `IInventoryElement` — 0 dependencias Unity
- `Observer/` — interfaces puras
- `Handler/` — puro C#
- `HealthComponent`, `HealComponent`, `FisiologicComponent` (parcialmente)
- `FluidComponent`, `StorageComponent`, `BaseItemComponent`

**Filtraciones de Unity que rompen el desacoplamiento:**

1. **`PositionComponent`** depende directamente de `UnityEngine.Transform`. Este es el componente más acoplado. Si migras a otro motor, hay que reescribirlo entero.

2. **`MovementComponent`** usa `UnityEngine.Vector2` — la dependencia es más suave (es solo un struct), pero rompe la compilación fuera de Unity.

3. **`FisiologicComponent`** importa `UnityEngine` aunque no lo usa directamente — eliminar el using.

4. **`Logic`** hace `GameObject.FindWithTag("MainPlayer")` en la **inicialización de campo**. Esto es un acoplamiento fuerte Y un bug potencial (se ejecuta antes del Awake del MonoBehaviour).

5. **`EntityManager`** recibe `GameObject` en constructor — debería recibir una abstracción.

6. **`BaseCameraStrategy`** hace `GameObject.FindWithTag("MainPlayer")` — acoplamiento directo al scene graph.

7. **`CameraRegister`** accede a `GameObject.FindWithTag("MainCamera")` en constructor.

**Recomendación:** Crear una interfaz `IUnityBridge` o similar que encapsule las llamadas a `GameObject.Find*`, `Transform`, etc. Inyectarla desde `GameMain`. Así el ECS puro ni sabe que Unity existe.

---

## 3. CALIDAD DEL CÓDIGO

### 3.1 Bugs concretos

**CRÍTICO — `PrototypeFactory.CreatePlayerEntityPrototype()`:**
```csharp
e.AddComponent(new UnityEntityComponent(player)); // línea 50
// ... null check redundante ...
e.AddComponent(new UnityEntityComponent(player)); // línea 57 — DUPLICADO
```
Se añade `UnityEntityComponent` **dos veces**. La segunda sobrescribe la primera (por el `Dictionary<Type, IComponent>`), así que no crashea, pero es dead code y evidencia de un copy-paste.

**CRÍTICO — `HealthComponent` usa `Thread.Sleep()`:**
Los métodos `ReceiveDamageOverTime`, `HealOverTime`, `ReceiveDamagePercentageOverTime`, `HealPercentageOverTime` bloquean el hilo con `Thread.Sleep(1000)`. En Unity esto **congelaría el juego entero**. Necesitan reescribirse como corrutinas o integrarse con el `ClockSystem`.

**MEDIO — `Logic` inicializa campo con `GameObject.FindWithTag`:**
```csharp
private readonly EntityManager entityManager = new EntityManager(GameObject.FindWithTag("MainPlayer"));
```
Se ejecuta al construir `Logic`, que puede ocurrir antes de que el objeto "MainPlayer" exista en escena. Es una bomba de tiempo.

**MEDIO — `FluidComponent.AddFluid()` tiene bug lógico:**
```csharp
if (fluids.ContainsKey(fluid) && left > 0)
```
Solo añade fluido si la key **ya existe**. Nunca permite añadir un tipo de fluido nuevo. Debería ser `||` o manejar el caso de key nueva.

**BAJO — `EquipmentComponent` no inicializa `equipmentSlots`:**
El campo `equipmentSlots` se declara pero nunca se inicializa con `new Dictionary<>()`. Cualquier llamada a `AddSlot()` o `EquipItem()` lanzará `NullReferenceException`.

**BAJO — `GameMain.Awake()` crea `InputManager` dos veces:**
```csharp
gameContext.SetInputManager(inputManager = new InputManager(gameContext)); // línea 27
inputManager = new InputManager(gameContext); // línea 43 — sobrescribe
```

### 3.2 Inconsistencias de estilo

**Naming:**
- Mezcla de convenciones: `getInventory()` (Java-style) junto a `GetWeight()` (C#-style). Componentes como `StorageComponent` usan `getMaxVolume()` en minúscula.
- `MaterialComponen.cs` — falta la "t" en el nombre del archivo.
- `FisiologicComponent` — typo: debería ser "PhysiologicComponent" o "FisiologicoComponent" (si prefieres español).
- `VIewManager.cs` — mayúscula incorrecta en "I".
- `Piorities.cs` — typo, debería ser "Priorities".
- `GameEventType.INVETORY_CHANGED` — typo, debería ser "INVENTORY_CHANGED".
- `FPCStraegy.cs` — typo en deprecated.
- `fattigueDrain` — typo en variable local.

**Getters/Setters vs Properties:**
- `HealthComponent` usa properties C# (`CurrentHealth =>`)
- `FisiologicComponent` usa getters Java-style (`GetHeight()`)
- `BaseItemComponent` mezcla ambos estilos
- **Recomendación:** Estandarizar a properties C# (`public float Height { get; set; }`) excepto cuando necesites lógica de validación.

**Comentarios:**
- Mezcla español/inglés sin criterio: XML docs en español, comentarios inline en inglés, y viceversa. Elegir un idioma y mantenerlo.
- `/// </summary>` sin cierre en `MaterialComponen.cs` — XML doc roto.

### 3.3 Dead code y archivos deprecated

Existen carpetas `.deprecated/` con archivos viejos (`DamageComponent.cs`, `InventoryComponent.cs`, `HealthManager.cs`, `ItemEntity.cs`, `FPCStraegy.cs`, `TPCameraStrategy.cs`). Están correctamente separados, pero recomiendo eliminarlos del proyecto y confiar en git para el historial.

---

## 4. DISEÑO DE SISTEMAS

### 4.1 ECS propio — Evaluación

**Lo que funciona bien:**
- Las entidades son contenedores genéricos de componentes (`Dictionary<Type, IComponent>`)
- El patrón Prototype para crear entidades es correcto
- `Clone()` y `Equivalent()` están bien implementados en la mayoría de componentes
- La interfaz `IComponent` es mínima y suficiente

**Problemas fundamentales:**

1. **No es realmente un ECS.** Es un Entity-Component system sin la "S" de Systems correctamente implementada. En un ECS real, los sistemas iteran sobre TODAS las entidades con ciertos componentes cada frame. Aquí, los "systems" (`FatigueStaminaSystem`, `MovementSystem`) operan sobre entidades individuales de forma ad-hoc. `FatigueStaminaSystem.ProcessEntity()` se llama manualmente desde `InputManager` y `Logic` — no hay un loop de sistemas centralizado.

2. **`GetComponent<T>(Type target)` es redundante.** La firma pide el tipo genérico Y el Type como parámetro. Con genéricos basta: `GetComponent<T>() where T : IComponent` usando `typeof(T)` internamente. La firma actual obliga a escribir `entity.GetComponent<FisiologicComponent>(typeof(FisiologicComponent))` — verboso y propenso a errores si T y target no coinciden.

3. **Las entidades concretas (`AliveEntity`, `UnaliveEntity`, `ItemEntity`) están vacías.** Son clases sin ningún campo ni método propio. Su única función es diferenciación de tipos, pero al heredar de `InGameEntity` no aportan nada. Si la diferenciación importa, podrían ser un enum `EntityKind` dentro de `InGameEntity`, o un componente tag (`AliveTag`).

4. **`IdGenerator` no es thread-safe y se resetea.** Un `static int` con `currentId++` sin `Interlocked.Increment`. Para singleplayer es tolerable, pero el método `Reset()` puede causar colisiones de IDs si se llama incorrectamente.

### 4.2 Inventario — El mejor sistema del proyecto

El sistema de inventario es **notablemente bueno** para el nivel del proyecto:

- Patrón **Composite** bien aplicado (`InventoryObject` como nodo, `ItemObject` como hoja)
- **BFS** para búsqueda en profundidad — correcto para la estructura de árbol
- Separación entre operaciones **globales** (recursivas) y **locales** (nivel inmediato)
- `StackOnto` para apilar items equivalentes
- `CleanTree` para eliminar nodos con cantidad 0
- `FlattenInventory` para la UI
- Peso y volumen calculados recursivamente

**Mejoras posibles:**
- Los IDs de items son strings basados en `GetName()`, lo cual puede causar colisiones si dos items distintos tienen el mismo nombre. Usar el ID numérico de la entidad sería más robusto.
- No hay límites de capacidad (peso/volumen máximo del contenedor) aplicados al añadir items — `StorageComponent` tiene `maxVolume` y `maxWeight` pero nadie los consulta en `AddItem()`.

### 4.3 Fisiología

`FisiologicComponent` es ambicioso y bien fundamentado (fórmulas BMI, Mifflin-St Jeor para metabolismo basal, porcentaje de grasa). El problema es que **es un God Component**: ~300 líneas, 20+ campos, mezcla stamina, fatiga, hambre, sed, nutrientes y capacidad de carga. Es un componente que debería ser 3-4 componentes:

- `PhysicalStatsComponent` (altura, peso, edad, sexo, grasa)
- `EnergyComponent` (stamina, fatiga, metabolismo)
- `NutritionComponent` (hambre, sed, macronutrientes, agua)
- La capacidad de carga debería calcularse como un servicio/sistema que consulta estos datos, no como método dentro del componente

### 4.4 Observer

Bien implementado con dos variantes: `IObserver` (simple, sin datos) e `IEventObserver` (con `GameEvent`). `GenericSubject` proporciona una implementación base. El problema es que `FatigueStaminaSystem` reimplementa la lista de observers con `ArrayList` en vez de usar `GenericSubject` — inconsistencia.

### 4.5 Cámaras (Strategy)

El patrón Strategy está bien aplicado para las 3 cámaras (FPS, TPS, RTS). `BaseCameraStrategy` centraliza la lógica común. Problema: **duplicación de código entre `FirstPersonCamera` y `ThirdPersonCamera`** — el `HandleMovement` es prácticamente idéntico (WASD + shift + space). Debería estar en `BaseCameraStrategy`.

### 4.6 Eventos

El sistema de eventos es **embrionario**. Solo existe `GameEventType.INVETORY_CHANGED` (con typo). `GameEvent` transporta entidad + componente, pero no hay un bus de eventos centralizado. Los eventos se disparan y consumen de forma ad-hoc. Para el futuro del proyecto (NPCs con memoria, reputación, colonia) necesitarás un EventBus robusto.

### 4.7 MVC/MVP

`GameContext` funciona como Service Locator — centraliza acceso a todos los managers. Está bien para el tamaño actual. `InventoryPresenter` implementa MVP correctamente con un mecanismo de view-readiness (`OnReady` + `_pendingTabIndex`) que resuelve una race condition real de UI Toolkit.

Problema: `GameMain.Awake()` es un método de inicialización de 25 líneas donde el orden importa. Cuando crezca, será frágil. Considerar un sistema de inicialización por fases.

---

## 5. RESUMEN DE ACCIONES PRIORITARIAS

### Urgente (bugs que crashean o congelan)

1. Eliminar los `Thread.Sleep()` de `HealthComponent` — reemplazar con integración al `ClockSystem`
2. Inicializar `equipmentSlots` en `EquipmentComponent`
3. Mover la inicialización de `EntityManager` en `Logic` al constructor o a un método `Initialize()`
4. Arreglar `FluidComponent.AddFluid()` para permitir tipos nuevos
5. Eliminar el duplicado de `UnityEntityComponent` en `PrototypeFactory`

### Importante (deuda técnica que frena el desarrollo)

6. Simplificar firma de `GetComponent<T>()` — eliminar el parámetro `Type target`
7. Crear loop centralizado de systems en vez de llamadas manuales ad-hoc
8. Extraer `HandleMovement` duplicado de FPS/TPS a `BaseCameraStrategy`
9. Dividir `FisiologicComponent` en 3-4 componentes menores
10. Estandarizar naming a convenciones C# (properties, PascalCase en métodos)

### Recomendable (para el TFG y escalabilidad)

11. Separar físicamente código C# puro de código Unity-dependiente
12. Corregir todos los typos en nombres de archivos y tipos
13. Eliminar archivos deprecated del proyecto
14. Implementar un EventBus centralizado
15. Añadir unit tests para el inventario y la fisiología (son C# puro, fácilmente testables)

---

*Revisión generada sobre el estado actual del repositorio. Refleja lo que hay en código, no lo planificado.*
