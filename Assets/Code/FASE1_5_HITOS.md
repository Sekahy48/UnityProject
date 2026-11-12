# Fase 1.5 — Mundo físico

Documento de hitos de la Fase 1.5. Se construye en modo socrático: Claude pregunta, Sergio
decide, y aquí queda lo decidido con su porqué. Las mecánicas salen de `Docs/MECANICAS.md` y
el encaje en el plan, de `Docs/ROADMAP.md`.

## Dónde estamos

> Esta sección existe para el relevo entre conversaciones: reescribirla al cerrar cada tarea.

**Definida la fase, sin hitos todavía (2026-11-12).** Rama `fase1.5`, creada desde `main`
tras fusionar la Fase 1 (PR #2). Tras la reunión con David del 2026-11-12 la fase se queda
**solo en el mundo físico** (soltar con física y movimiento del jugador por física); el
guardado de partida se aplaza a después del TFG, con su diseño conservado abajo. Siguiente:
que Sergio pida la propuesta cerrada de hitos.

## Objetivo

Lo que se suelta cae y rueda como un objeto real; el jugador y los objetos se empujan según
su peso.

---

## Decisiones de experiencia de juego (2026-11-11)

### Soltar con física

1. **Todo lo que se suelta cae y rueda,** sea un ítem suelto, un montón o un contenedor (el
   arcón incluido).
2. **Sale un poco por delante del jugador,** donde hoy aparece lo soltado, y cae desde ahí.
   **No atraviesa paredes:** si hay algo delante, aparece pegado al jugador.
3. **Los objetos interactúan con lo que hay alrededor:** se empujan entre sí y el jugador los
   mueve al chocar con ellos.
4. **Un montón cae como un solo objeto,** como es hoy en el Core. Separarlo en unidades
   queda fuera del TFG.
5. **Se puede coger algo mientras rueda,** con las mismas condiciones de alcance que hoy.

**Lanzar objetos, aplazado (fuera de esta fase).** Interesante, pero arrastra decisiones que
no tocan ahora: lanzar exige llevar antes el objeto en las manos (hoy tirar es una opción del
menú contextual del inventario), distinguir objetos de una mano (una manzana) y de dos (un
arcón), y qué pasa al impactar (un arcón lanzado contra una pared debería romperse, lo que
toca la durabilidad). «Llevar en las manos» encaja con colocar objetos (Fase 5), así que se
revisa entonces. Apuntado en `Docs/MECANICAS.md`.

### Guardar y cargar — APLAZADO A DESPUÉS DEL TFG

**Decidido el 2026-11-12, tras la reunión con David:** el guardado no entra en el TFG. Una
vertical slice se enseña con un escenario preparado, no con partidas guardadas, y mantener el
guardado al día con cada sistema nuevo cuesta sin aportar al objetivo. Lo de esta sección y las
decisiones 14–24 quedan como **diseño hecho, para retomar después del TFG**. Mientras tanto,
disciplina barata: ids de entidad estables y nada que no se pueda reconstruir.

6. **Mezcla de tres formas de guardar:**
   - **Autoguardado periódico,** con el intervalo configurable en ajustes (10 minutos por
     defecto).
   - **Guardado manual:** sobrescribir una partida o guardar como archivo nuevo.
   - **Guardado al dormir.** Dormir llega en la Fase 2; esta fase deja el guardado listo para
     que dormir lo llame.

   Que solo se pueda guardar en ciertos momentos, por realismo, queda fuera del TFG.
7. **Varias partidas:** se responde con lo anterior (guardar como archivo nuevo).
8. **Interfaz:** en esta fase basta con guardar y cargar desde controles de depuración. El
   menú de verdad (inicio, ranuras, ajustes) se hace cuando convenga; el sistema de guardado
   ya admite varios archivos desde el principio para que el menú solo tenga que llamarlo.

---

## Decisiones de sistema (2026-11-11)

### Física

9. **Todo se empuja según su masa, el jugador incluido** (no hay caso especial «el jugador
   empuja pero no es empujado»). Un solo modelo para todo: menos casos, se procesa igual.
   **Consecuencia:** el jugador tiene que moverse por física. Hoy se mueve escribiendo
   `transform.position` (`BaseCameraStrategy`, línea ~217) sobre un Rigidbody dinámico de masa
   1, que es justo la causa del tirón contra las paredes visto en octubre. Pasar a moverlo por
   el Rigidbody arregla las dos cosas.
10. **La masa física es el peso del ítem** (el del inventario). Une inventario y física a
    propósito. **La masa del jugador es la de su cuerpo más lo que lleva encima.**
11. **Forma de colisión: envoltura convexa calculada del modelo 3D.** Lo redondo rueda, lo
    cuadrado vuelca. Para el futuro, una forma elegida por ítem en Stack&Go (esfera, caja,
    cápsula), cuando se vuelva a Stack&Go.
12. **Al pararse, el objeto deja de simularse** hasta que algo lo toque, y **al cargar aparece
    exactamente donde se paró,** sin volver a caer.
13. **Si algo se sale del mundo** (atraviesa el suelo o cae por un borde), **se recoloca en el
    punto válido más cercano.** Ni vuelve a donde se soltó ni se pierde (perderlo tiene riesgo
    de «efecto portal»: tirar cosas a propósito para hacerlas desaparecer).

### Guardado (aplazado a después del TFG; ver arriba)

14. **Formato JSON legible,** con Newtonsoft (ya está en el proyecto). Binario, más adelante si
    hace falta.
15. **Qué se guarda: entidades con sus componentes, no una lista por funcionalidad.** El
    jugador es una entidad: su posición, su estado, su inventario y su equipo son
    componentes suyos. Lo mismo los objetos del mundo. Aparte, el estado que no es de ninguna
    entidad (en la 1.5, la cámara activa; en la 2, el reloj del mundo). Así cada fase añade
    componentes y el guardado los recoge sin cambiar de forma.

16. **Cada componente declara si se guarda** con la interfaz `ISaveComponent` (ver 18). Lo que no se guarda (el enlace con el GameObject, el
    volumen de interacción medido del modelo) se reconstruye al cargar. Un test recorre todos
    los tipos de componente y falla si alguno no está ni marcado para guardarse ni declarado
    como reconstruible: así no se olvida ninguno en silencio.
17. **Identidad: cada entidad se guarda una sola vez con su id, y las relaciones van como
    referencias a ids.** No es solo para el futuro: la mochila puesta ya se alcanza por dos
    caminos (su slot de equipo y el árbol de inventario del jugador), y guardar el árbol anidado
    la escribiría dos veces y al cargar habría dos mochilas. Exige ids estables guardados, que
    al cargar el generador de ids siga por encima del mayor id cargado, y cargar en dos pasos:
    primero se crean todas las entidades, después se resuelven las referencias entre ellas.
18. **Contrato único `ISaveComponent` para todos los componentes que se guardan**, al estilo del
    catálogo (`ItemComponentRegistry` + `SetFromValues`): nombre con el que viaja el
    componente, `GetValues()` y `SetFromValues(...)`, con diccionarios de valores; el Core no
    sabe de JSON, solo el cargador y el guardador. **Los simples** lo implementan sobre sí
    mismos. **Los complejos** (inventario, equipo) implementan el mismo contrato pero delegan en
    un serializador estático propio (propuesta de Sergio: así el contrato es igual para todos).
    **Referencias entre entidades:** `SetFromValues` recibe un contexto con un buscador de
    entidades. El cargador crea primero todas las entidades vacías (con sus ids) y después
    rellena sus componentes, así que cualquier referencia ya se puede resolver. Un solo método
    para todos; los simples ignoran el contexto. (Descartado: un segundo método
    `ResolveReferences` solo para los complejos.)
19. **Cada archivo lleva número de versión. Hasta que el juego tenga una versión estable, no se
    migran guardados antiguos:** uno de otra versión se rechaza con un aviso. Las migraciones
    (pasos sobre el JSON antes de leerlo) empiezan con la primera versión estable.
20. **Guardar y cargar se lanzan desde opciones del menú de desarrollo,** no con teclas.
21. **Al cargar, todo cerrado y la mano vacía.** No se pierde nada: la mano es una reserva y
    lo que llevaba sigue en su origen. Por eso el autoguardado puede saltar en cualquier
    momento, incluso arrastrando algo.
22. **Autoguardado rotando los 3 últimos,** por si uno sale mal.
23. **Guardados manuales con nombre automático por fecha y hora** (`partida_AAAA-MM-DD_HH-mm`)
    mientras no haya menú.
24. **Los guardados van a `Application.persistentDataPath`** (en Windows, dentro de `AppData`),
    fuera del proyecto y del repositorio.

## Preguntas pendientes

Ninguna de experiencia ni de diseño. Siguiente paso: propuesta cerrada de hitos (cuando
Sergio la pida).
