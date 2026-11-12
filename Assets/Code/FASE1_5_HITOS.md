# Fase 1.5 — Mundo físico

Documento de hitos de la Fase 1.5. Se construye en modo socrático: Claude pregunta, Sergio
decide, y aquí queda lo decidido con su porqué. Las mecánicas salen de `Docs/MECANICAS.md` y
el encaje en el plan, de `Docs/ROADMAP.md`.

## Dónde estamos

> Esta sección existe para el relevo entre conversaciones: reescribirla al cerrar cada tarea.

**M1 cerrado (2026-11-12).** Rama `fase1.5`. La fase se queda solo en el mundo físico (soltar
con física y jugador por física); el guardado se aplaza a después del TFG, con su diseño
conservado abajo. El jugador ya se mueve por física (Sergio lo escribió, con guía): rapidez y
dirección separadas (33), sin rozamiento al moverse y agarre al estar quieto (34, 37), sin
Apply Root Motion (35) y cámaras FPS/TPS descolgadas del cuerpo (36). Probado en juego: paredes,
FPS, TPS, caídas, cambio a RTS, rampas y paredes en el aire. Siguiente: **M2, lo soltado cae y
rueda, con el sistema de masa** (quién escribe cada parte, se decide al empezarlo).

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

### Implementación (2026-11-12)

25. **No atravesar paredes al soltar: el Core pregunta a Unity por una interfaz,** igual que
    con `IReachFilter` / `LineOfSightFilter`. El Core decide dónde aparece lo soltado; Unity
    solo responde si el camino está libre.
26. **Masa del jugador con su carga: Unity la actualiza al recibir el aviso de que el inventario
    ha cambiado** (ya existe), no en cada fotograma.
27. **Mientras el modelo 3D carga, el objeto cae ya con la esfera provisional** y la cambia por
    la envoltura convexa cuando llega el modelo.
28. **Objetos pequeños bajo los pies: el umbral se decide al probar** (por masa o tamaño, por
    debajo del cual el objeto no choca con el jugador).
29. **Salirse del mundo: se busca suelo en vertical en esa misma posición horizontal; si no
    hay, vuelve al último sitio donde estuvo quieto.**
30. **Quién escribe cada cosa se decide al llegar a cada hito.** Lo de Core, por la regla de
    `CLAUDE.md`, es de Sergio.
31. **La masa de los Rigidbody la pone un sistema propio de Unity** suscrito al aviso de
    inventario cambiado (`InventoryChanged`; más adelante también al de equipo cambiado). No
    es «cada entidad comprueba si el aviso es suyo»: un solo sistema atiende a todas, toma la
    entidad que viene en el aviso y, si tiene cuerpo físico en Unity, le pone `masa = peso`.
    Vale igual para el jugador y para los objetos (un arcón del suelo al que le metes cosas).
    Como el aviso no llega cuando algo aparece en el mundo, la masa inicial se pone también
    al enlazar la entidad con su GameObject. Las estrategias de cámara siguen guardando el
    GameObject del jugador y sacan de él el Rigidbody una vez; no saben nada de pesos.
32. **El peso de cualquier entidad sale de una sola función del Core:** un ítem, `ItemWeight.Of`
    (con su contenido); un cuerpo, su peso más el total de su inventario (que ya incluye la
    mochila puesta). La ropa puesta aún no pesa (decidido, sin aplicar). Es Core: la escribe
    Sergio.
33. **El movimiento separa rapidez y dirección** (M1, 2026-11-12). La rapidez sube con
    `Acceleration` (8 m/s²) y baja con `Deceleration` (16 m/s²); la dirección gira hacia la de
    las teclas a `TurnSpeed` (720 °/s) sin gastar rapidez. Motivo: con piernas se cambia de
    dirección casi sin frenar; acercar el vector entero (`MoveTowards`) hacía perder mucha
    velocidad en cada giro. Media vuelta sin frenar: aceptado tras probarlo; si resultara raro,
    se añadiría un freno según lo cerrado del giro.
34. **El jugador no tiene rozamiento con el suelo:** material físico `PlayerSinFriccion`
    (fricción 0, combinación Minimum) en `Assets/Settings/Physics/`. El frenado lo hace el
    código (33); con el rozamiento por defecto el suelo se comía la aceleración. Evita también
    quedarse pegado a las paredes en el aire.
35. **Animator del jugador sin Apply Root Motion:** con él activo, el Animator pisaba cada paso
    de física la velocidad del Rigidbody con la de la animación (0, van en el sitio) y el
    jugador temblaba sin avanzar.
36. **Las cámaras de primera y tercera persona no cuelgan del cuerpo** (M1, 2026-11-12). El
    cuerpo gira con `MoveRotation`, al ritmo de la física; si la cámara colgara de él, el ratón
    iría a tirones. FPS: la cámara se coloca cada fotograma en la posición del jugador más la
    altura de los ojos y aplica los dos giros directamente. TPS: sigue al jugador sin retraso y
    solo suaviza su desplazamiento alrededor de él (la órbita del ratón); antes suavizaba
    también el seguimiento y al caer se quedaba atrás. La dirección de W/A/S/D sale del giro
    del ratón, no del cuerpo, que va un paso de física por detrás.
37. **Quieto, el jugador se agarra al suelo** (M1, 2026-11-12). Sin rozamiento (34) se
    deslizaba despacio por las rampas al pararse, y el rozamiento estático del material no
    bastó. `HandleMovement` cambia el material del collider: sin teclas, uno de agarre total
    (`PlayerQuieto`, creado por código, combinación Maximum); con teclas, `PlayerSinFriccion`.
    **Límite conocido, para Fase 2:** el agarre representa estar de pie y consciente, y hoy lo
    decide «no hay teclas». Un cuerpo desmayado o muerto en una cuesta debe poder resbalar
    (solo en las empinadas: un cuerpo inerte sobre hierba aguanta las suaves). Cuando exista
    el desmayo, el material saldrá del estado del cuerpo (moviéndose / de pie / inerte), no
    de las teclas, y el movimiento por teclas quedará bloqueado mientras no pueda moverse.

---

## Hitos

Orden pensado para que cada hito se pruebe solo y no estropee lo anterior. El jugador va
primero: si los objetos tuvieran física mientras el jugador sigue moviéndose a base de
escribir su posición, al tocarlos los «teletransportaría» dentro de ellos y saldrían
disparados.

### M1 — El jugador se mueve por física ✅ (2026-11-12)
- El movimiento deja de escribir `transform.position` (`BaseCameraStrategy`) y pasa a darle
  velocidad al Rigidbody dinámico, con interpolación para que la cámara no tiemble.
- Revisar las pendientes y los escalones. La voltereta y el mortal son solo animaciones (no
  hay código que mueva al jugador); sin Apply Root Motion (35) ya no desplazan el cuerpo.
- La masa del jugador (cuerpo + carga) pasa a M2, con el sistema de masa (31, 32).
- **Cierre:** se mueve igual que antes al jugar, sin el tirón contra las paredes; la cámara
  no tiembla.

### M2 — Lo soltado cae y rueda
- Sistema de masa (31, 32): la del jugador = cuerpo + carga, actualizada con el aviso de
  inventario cambiado (26); la de lo suelto, la de abajo.
- Lo que está en el mundo (ítems sueltos, montones, arcones) tiene Rigidbody dinámico con masa
  = peso del ítem con su contenido (`ItemWeight.Of`) y colisión convexa del modelo (11), con la
  esfera provisional mientras carga (27).
- La posición: el Core la marca al soltar; desde ahí manda la física y `TransformSyncSystem`
  ya la copia al Core. Al pararse, deja de simularse (12).
- Se puede coger mientras rueda (5).
- **Cierre:** soltar una manzana y un arcón: caen, ruedan o vuelcan según su forma, se paran y
  se pueden coger, también en movimiento. Empujar la manzana y empujar el arcón se nota
  distinto.

### M3 — No se tira nada a través de una pared
- Interfaz del Core para preguntar a Unity si el camino del jugador al punto de soltar está
  libre; si no, aparece pegado al jugador (2, 25).
- **Cierre:** soltar mirando a una pared deja el objeto en el lado del jugador. Test del Core
  con un filtro de prueba.

### M4 — Objetos pequeños bajo los pies
- Probar a pisar objetos pequeños y fijar el umbral (28).
- **Cierre:** andar sobre manzanas no hace botar al jugador.

### M5 — Lo que se sale del mundo vuelve
- Detección por debajo de un límite y recolocación (29).
- **Cierre:** un objeto que atraviesa el suelo o cae por un borde vuelve a un sitio válido.

### M6 — Cierre de la fase
- Repaso de los criterios de `Docs/ROADMAP.md` (Fase 1.5), tests del Core en verde y «Dónde
  estamos» reescrito.
