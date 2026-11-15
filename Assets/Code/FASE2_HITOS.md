# Fase 2 — Fisiología

Documento de hitos de la Fase 2. Se construye en modo socrático: Claude pregunta, Sergio
decide, y aquí queda lo decidido con su porqué. Las mecánicas salen de `Docs/MECANICAS.md`, el
encaje en el plan, de `Docs/ROADMAP.md` y los números de referencia, de
`Docs/FISIOLOGIA_REFERENCIA.md`.

## Dónde estamos

> Esta sección existe para el relevo entre conversaciones: reescribirla al cerrar cada tarea.

**Fase empezada (2026-11-14).** Rama `fase2`, desde `main` con la Fase 1.5 ya fusionada.
- **Hecho:** investigación de fisiología de referencia, con la del agua dentro
  (`Docs/FISIOLOGIA_REFERENCIA.md`).
- **Investigación ampliada** (2026-11-14 y 15): hambre y saciedad (§5 bis), músculo y entrenamiento
  (§2), ganancia con déficit y adaptación nerviosa (§2), fórmulas del código verificadas.
- **Cerrado en el tronco (capa 0):** piezas del cuerpo (23 con músculo + 7 solo herida), músculo
  guardado y ganancia con puerta energética y proteica, reservas de nutrientes, contenido digestivo
  con vaciado lineal, hambre con dos hechos (llenado derivado + acumulador de ayuno) y hábito por
  ventana de X días, agua (solo hervir), micronutrientes en un saco, sueño sin muerte.
- **Aplazado:** vómito (ver «Aplazado a propósito»); alucinaciones, a capa 2.
- **Capa 0: todas las decisiones cerradas (2026-11-15, 20:32).** Ver «Lista de decisiones de la
  capa 0» y las secciones «Decididas». Quedan solo valores de calibración, que se fijan al
  implementar.
- **En curso:** escribir los hitos de la fase.
- **Siguiente:** cuando la investigación y las decisiones estén cerradas, escribir los hitos. El
  primero es la base del menú de depuración, no una mecánica.
- **Aviso para quien retome esto:** las secciones «Decididas por Sergio (fecha, hora)» están en
  orden cronológico y algunas decisiones posteriores sustituyen a las anteriores (se indica en
  cada una). Las secciones «Decididas» mandan sobre el estado de la lista. La lista original de 25 preguntas está en «Lista de decisiones de la capa 0», con su estado.

## Punto de partida

Decidido fuera de esta conversación (reunión con David, 2026-11-12; `Docs/ROADMAP.md`, mk2):

- Toda la fisiología va en esta fase, ordenada por capas (0 tronco, 1, 2).
- Se prueba con un **menú de depuración** y **escenarios preparados**. Nada de fuentes reales
  de daño (animales, etc.): la fauna, cuando llegue, solo llamará al sistema de heridas.
- **Mínimo entregable del TFG:** Fase 1.5 + Fase 2 hasta la capa 1. El tronco y la capa 1 no
  se recortan.
- Cada hito cierra con algo jugable y probado en juego.

Lo que ya existe y sirve de base (leído en el código el 2026-11-14):

- `ClockSystem`: singleton con observadores, tick de 0,016 s, `SetTimeSpeed`, pausa. Sin hora
  del día.
- `EnergyComponent` + `FatigueStaminaSystem`: aguante y cansancio; `CalculateBasalMetabolism`
  (Mifflin-St Jeor) sin usar. Campos de balance y metabolismo sin consumidor.
- `NutritionComponent`: hambre, sed, reservas de kcal y agua, macronutrientes; aún sin sistema.
- `HealthComponent`: vida en puntos enteros. `BodyComponent`: talla, peso, edad, sexo, grasa.
- Precedente de estados con umbrales: `CarryCapacityCalculator` (bandas), `MovementSystem`
  (efecto sobre la velocidad), `WeightLevel` y `HUDPresenter` (icono cualitativo).

## Decisiones

### Cerradas

- **El sueño no mata** (2026-11-14, pregunta 23). Sin dormir, el personaje sufre microsueños y
  acaba **desmayándose**; no hay muerte directa. *Por qué:* no hay ninguna muerte humana
  documentada solo por privación de sueño; lo que mata es la consecuencia
  (`Docs/FISIOLOGIA_REFERENCIA.md` §8). *Efecto en el cierre de la fase:* el criterio 5
  («descuidar el cuerpo lleva al desmayo y a la muerte») vale para sed, hambre, sangre y
  temperatura, no para el sueño.

### Respuestas parciales, sin cerrar (2026-11-14)

Recogidas del mensaje de Sergio; se confirman al cerrar la lista.

- **Preguntas 2, 6 y 8 (inclinación):** el cuerpo se simula **por periodos**, con valores reales
  (agua corporal, calorías) que bajan según reglas de la referencia y multiplicadores por
  actividad; la sed, el hambre y demás se **derivan** de esos valores. No hace falta que cada
  evaluación sea por tick: «cada varios segundos» vale.
- **Comidas e hambre (preguntas 17 y 6, diseño buscado):** el límite máximo de saciedad debe
  permitir que **3–4 comidas decentes** al día, repartidas en una jornada normal, mantengan el
  hambre a raya hasta que «toque» la siguiente. Nada de hambre a los 15 minutos de un plato de
  cocido. No se impone un número de comidas: quien pique cada 5 minutos también puede. El hambre
  empieza a entrar cuando, según un horario normal, tocaría la siguiente comida o un poco antes.
- **Etapas de hambre (por decidir con la investigación §5 bis):** hambriento, muy hambriento y un
  tramo extremo que no es morirse de hambre. Sergio pide proponer otro nombre si «famélico» no es
  exacto.
- **Idea de mecánica:** tras mucho tiempo en hambre extrema, un estado tope de **indiferencia al
  hambre** (embotamiento), algo más suave que el extremo, que se rompe al comer. La referencia
  lo apoya con matices (§5 bis).
- **Electrolitos:** Sergio ve razonable colgarlos de los micronutrientes de alimentos y cuerpo.
  La referencia dice que el sodio se comporta distinto de las vitaminas (§3): pregunta nueva
  abajo.

### Decididas por Sergio (2026-11-14, tarde)

- **Micronutrientes como saco único.** No hay un campo por nutriente (sodio, fósforo, vitamina C…).
  Si hace falta comprobar sodio o escorbuto, se comprueba `micronutrients`. *Por qué:* ese nivel de
  simulación no compensa en un juego. *Coste asumido:* no se podrá representar hiponatremia con
  precisión propia (ver `FISIOLOGIA_REFERENCIA.md` §3).
- **Etapas de hambre:** hambriento → muy hambriento → desfallecido → **indiferente** (el
  embotamiento tras mucho tiempo en hambre; algo más suave que desfallecido). Los nombres están
  aceptados; los **tiempos de cada tramo** siguen por decidir y son calibración.
- **El hábito de comer genera un patrón de hambre** (el hambre aparece cuando «toca»). Más trabajo
  de modelado, asumido.
- **Forma de trabajar la fase:** definir cada hito e implementarlo antes de definir el siguiente,
  no todos de golpe.

- **Músculo guardado, no deducido** (Sergio, 2026-11-15): en cuanto el entrenamiento lo cambia,
  deja de poder calcularse. El valor inicial sale de una ecuación poblacional
  (`FISIOLOGIA_REFERENCIA.md` §2). **Entrenamiento continuo:** no hay «terminar una acción»; mientras
  dura la acción (golpear un árbol) se acumulan milésimas de progreso en las partes implicadas.
  *Por qué desaparece mi reserva de «dos ritmos»:* heridas y entrenamiento actúan ambos de forma
  continua mientras hay causa; el argumento de la frecuencia no sostiene separarlos.
- **Agua:** solo se hierve. No se añade otro tratamiento (2026-11-14, noche).

### Dudas nuevas, para la lista (2026-11-14, noche)

- **Fuerza y entrenamiento.** Idea de Sergio: el entrenamiento se guarda **por parte del cuerpo**
  (la misma partición que las heridas); brazos entrenados dan más fuerza de tala, piernas más
  capacidad de carga. Pendiente: no está en `ROADMAP.md` ni `MECANICAS.md` (alcance nuevo, en
  qué capa); cuántas zonas; si el entrenamiento y las heridas viven en el mismo componente o en
  dos (ritmos de cambio muy distintos); cómo se combina con la masa muscular base.
- **`_fatPercentage` obsoleto.** Verificado: `SetWeight` no recalcula la grasa y nadie fuera de
  `BodyComponent` llama a `SetWeight` ni a `EstimateFatPercentage`, así que hoy es latente, no
  activo. Sergio lo arreglará; falta elegir cómo (recalcular en el setter, no guardarlo, o guardar
  composición como dato primario). Depende de la pregunta anterior.
- **Alucinaciones por falta de sueño:** **capa 2** (decidido por Sergio, 2026-11-15). Solo capa de
  vista, sin tocar Core.

### Propuesta de Sergio sobre piezas anatómicas (2026-11-15), sin cerrar

Se recoge tal cual la dijo; las dudas están en la sección de arriba. Aún no hay decisión.

- **Pieza anatómica** = cada músculo individual, con lado (bíceps izquierdo, pecho derecho…).
  Es a la vez la unidad que recibe heridas y la que se entrena. Los **grupos musculares** son
  conjuntos de piezas.
- **Interfaz** tipo Project Zomboid, con dos paneles (frente y espalda; juntos o con botón).
- **Actividades:** cada una declara, por diseño, qué grupos entrena. Asimetría aceptada: una acción
  a una mano entrena solo ese lado (es lo realista).
- **Consolidación al dormir:** mientras se hace la actividad se acumula un número pequeño por pieza
  (entero en milésimas, como el reloj). Al dormir se gasta para añadir músculo y para subir un
  índice de **adaptación nerviosa** por pieza. *Por qué:* sin adaptación nerviosa, ser muy fuerte
  exigiría estar hipertrofiado.

### Decididas por Sergio (2026-11-15, mediodía)

- **Reparto inicial del músculo:** se hace con lo que hay en la literatura y no más (fracciones
  constantes por pieza; calibración después).
- **Mejora muscular = una parte por dormir + una parte por existir, con retraso.** No inmediata.
  *Por qué:* es más realista y deja el camino abierto al cooperativo, donde el sueño es difícil de
  implementar (como en Project Zomboid multijugador): bastaría poner el porcentaje de «dormir» a 0
  y el de «existir» al 100 %. Con esto el acumulador deja de gastarse solo al dormir.
- **Piezas sin entrenamiento:** cabeza, cara, cuello… son piezas con herida y sin músculo
  entrenable, igual que el pectoral izquierdo y el derecho son piezas distintas.
- **El acumulador necesita tope.**
- **Requisitos para construir músculo** (planteamiento de Sergio): acumulación de entrenamiento +
  superávit energético + superávit de proteína, o al menos reservas. *Pendiente de ajustar*, porque
  la evidencia (`FISIOLOGIA_REFERENCIA.md` §2, «Ganar músculo con déficit») dice que es un gradiente
  y no un interruptor.
- **Interfaz:** el coste de dibujar un cuerpo con regiones no le preocupa.
- **Datos de las actividades:** en código por ahora; no van a Stack&Go.

### Decididas por Sergio (2026-11-15, 13:21)

- **Piezas, versión de Sergio** (sustituye a mi lista de 40, que era demasiado anatómica):
  por lado, **hombro, pectoral, espalda alta, espalda baja, bíceps, tríceps, antebrazo entero y
  mano** (×2); **abdominal** (una sola zona, sin oblicuos); **piernas por separado**
  (gemelo, cuádriceps, bíceps femoral…). *Por qué:* la fuerza de agarre no se modela y casi toda
  acción la lleva, así que flexores y extensores del antebrazo no se distinguen. Más piezas no
  aportan decisiones de juego y sí calibración. Pendiente: lista exacta de la pierna, si la mano
  es entrenable y las piezas sin músculo (ver dudas).
- **Adaptación nerviosa: por grupo y lado, con transferencia parcial al otro lado**
  (recomendación de Claude, aceptada). Los kg de músculo van por pieza y no se transfieren.
  *Por qué:* es una adaptación de control del movimiento, y el tamaño casi no cruza de lado
  (`FISIOLOGIA_REFERENCIA.md` §2).
- **Los cuatro grupos** (empuje, tirón, tronco, piernas) quedan aceptados.
- **Campo de proteína:** `NutritionComponent` ya tiene `_protein` (y `_carbohydrates`, `_fats`,
  `_micronutrients`, `_fiber`, `_storedKcal`). Verificado en el fichero. Falta decidir si `_protein`
  es reserva o ingesta (ver dudas).
- **Puerta de construcción muscular** (idea de Sergio): hasta ~300 kcal/día de déficit se sigue
  construyendo; por debajo del umbral de proteína de mantenimiento no se construye e incluso se
  puede perder. Los umbrales concretos los propone Claude y se contrastan; se aceptan datos
  compuestos cuando no haya medidos.

### Decididas por Sergio (2026-11-15, 13:32)

- **Piernas, por pierna:** glúteo, cuádriceps, **isquiotibiales** (nombre amplio, en lugar de
  «bíceps femoral») y gemelo. Aductores y tibial anterior quedan fuera.
- **Mano:** solo pieza de herida, no se entrena. **Sin músculo (no se entrenan):** cabeza, cuello,
  pies y tronco con órganos. Cuenta resultante: **23 piezas con músculo** (7 por lado ×2, abdominal
  y 4 por pierna ×2) y **7 solo de herida** (2 manos, 2 pies, cabeza, cuello, tronco).
- **Reservas de nutrientes:** todo lo que hay en `NutritionComponent` (proteína, grasas,
  carbohidratos, micronutrientes, fibra) es **reserva**; no es ingesta del día.
- **Comentario «Relevant in Phase 3» de `NutritionComponent`:** retirado (retoque trivial pedido
  por Sergio).
- **Umbrales de la puerta de construcción:** tabla de Claude aceptada como punto de partida;
  se calibra.
- **Idea de Sergio, sin cerrar:** en lugar de pasar lo comido directamente a la reserva, un
  «contenido digestivo» (macros y micronutrientes ingeridos) que se va procesando y se añade a la
  reserva poco a poco. Ver dudas.

### Decididas por Sergio (2026-11-15, 13:56)

- **Contenido digestivo como componente propio** (`DigestiveComponent`, nombre provisional): al
  comer, los macros y micronutrientes entran ahí; a cierta tasa salen y se suman a
  `NutritionComponent` (la reserva). *Por qué:* hace que comer no sea instantáneo y da la saciedad
  sin campo extra.
- **La puerta de construcción muscular solo cuenta la proteína de la reserva**, no la del contenido
  digestivo: aún no está disponible.
- **Etapas de hambre deducidas del contenido digestivo:** estómago vacío = hambriento; después,
  un contador que crece mientras está vacío (equivale a guardar tiempo) y sus umbrales dan muy
  hambriento, desfallecido (hambre extrema) e indiferente. Pendiente de Claude: revisar si el
  planteamiento aguanta (ver dudas).
- **Pregunta de Sergio sin cerrar:** tasa de vaciado única para todo o una por nutriente.

### Decididas por Sergio (2026-11-15, 14:17)

- **Drenado común del contenido digestivo:** cada tick sale una cantidad **del total**, repartida
  en proporción a la composición, no una cantidad por nutriente. *Por qué:* con una tasa por
  nutriente, 10 g de proteína + 10 g de hidratos se vaciarían antes que 20 g de hidratos, y no hay
  base para eso (ver `FISIOLOGIA_REFERENCIA.md` §5 bis y la reseña de vaciado gástrico). Los
  números del ejemplo eran ilustrativos, no de diseño.
- **Contador de hambre** (propuesta de Sergio): vale 0 con el estómago lleno, sube según se vacía;
  al pasar un umbral hay hambre; el **hábito de comidas también lo modifica**; con ayuno largo
  sigue creciendo y, pasado otro umbral, el estado es **indiferente**. Sergio reconoce la
  tensión con «un hecho, un dueño» (la parte de relleno se duplicaría con el contenido digestivo).
- **Salir de la indiferencia:** hace falta una comida con **potencia saciante** por encima de un
  umbral (un cacahuete no basta). Al superarlo se **recalcula** el contador teniendo en cuenta solo
  el llenado del estómago, y los **patrones de ingesta se consideran obsoletos**: se parte del
  punto base, como al empezar a jugar.
- **`DigestiveComponent`:** nombre y campos se dejan para cuando se asigne hito. **Umbrales y
  tasa de vaciado:** se proponen al implementar, no ahora.
- **Orden de trabajo:** primero acabar la investigación y las decisiones, después construir los
  hitos de esta fase, después implementar.

### Decididas por Sergio (2026-11-15, 14:29)

- **Vaciado lineal** (una cantidad fija del total por tick). *Por qué:* así el estómago llega a
  cero de verdad y no hace falta un umbral de «casi vacío».
- **Potencia saciante de una comida = cuánto llena el estómago** (porcentaje de llenado tras
  comer, con umbral de diseño). Sin magnitud nueva.
- **Hábito de comidas = registro por franjas horarias de las últimas N días** (N de diseño, p. ej.
  15). Cada comida que supere el umbral saciante marca su franja; al consultar, la probabilidad de
  comer a esa hora suma al contador de hambre. Idea de Sergio; sustituye al boceto de Claude
  (lista de horas con fuerza y promedio móvil). Notas de Claude: 24 franjas es poco (mejor 48–96
  con suavizado entre vecinas); no cuentan los picoteos bajo el umbral; con N mayor que un ayuno,
  los datos anteriores siguen dentro de la ventana, así que al salir de la indiferencia el
  registro se vacía igualmente (decidido el 14:17).
- **Sin cerrar:** contador único (incremental) o dos hechos (llenado derivado + acumulador de
  ayuno). Sergio pidió dos ejemplos con números para decidir.

### Decididas por Sergio (2026-11-15, 14:38)

- **Hambre con dos hechos (versión B):** el llenado se **deriva** del contenido digestivo al
  consultar y solo se guarda el **acumulador de ayuno** (a cero con una comida que supere el umbral
  saciante). *Por qué:* evita acoplar sistemas: si otro sistema cambia el contenido del estómago,
  el hambre lo refleja sin que nadie tenga que actualizar un número copiado. Se descarta la
  versión A (un número guardado y actualizado de forma incremental).
- **Hábito: ya no se vacía el registro al salir de la indiferencia** (sustituye a la decisión del
  14:17 y a la nota del 14:29). *Por qué:* si el ayuno dura Y días y la ventana es de X días con
  Y < X, aún quedan datos de referencia; con Y ≥ X la ventana se queda vacía y se vuelve sola al
  estado base. Es lo que Sergio cree realista: sin un ayuno muy largo, se sigue «sabiendo» el
  horario normal.
- **Detalle detectado por Sergio, sin resolver:** tras vaciar el estómago a la fuerza (vómito) no
  hay hambre, sino pereza de comer. Con B, estómago vacío = llenado a 100 = hambre, que es lo
  contrario. Hace falta un mecanismo. (Verificado: ni `ROADMAP.md` ni `MECANICAS.md` ni este
  documento mencionan el vómito: es alcance nuevo.)

### Decididas por Sergio (2026-11-15, 14:42)

- **Probabilidad del hábito = comidas en esa franja / X días completos de la ventana**, no solo los
  días con datos. *Por qué:* así el hábito se debilita solo según avanza un ayuno largo.
- **Vómito:** fuera de esta fase (ver «Aplazado a propósito»).

### Dudas nuevas, para la lista (2026-11-15)

- **Cuántas piezas y cuáles.** Con músculos individuales y lado salen unas 25–30; cada pieza
  llevaría herida, kg de músculo, índice nervioso y acumulador. Además faltan piezas sin músculo
  que sí reciben heridas (cabeza, cuello, manos, pies) y el tronco con órganos.
- **Dónde viven los datos de cada actividad** (pesos por grupo y modo de lado) y si van a Stack&Go.
- **Modo de lado por actividad:** ambos, mano activa, mano dominante…
- **¿La consolidación exige comer** (energía y proteína)? Con inanición, ¿no hay ganancia?
- **Calibración:** tope o rendimientos decrecientes del acumulador; plazos del índice nervioso
  (rápido) frente al músculo (lento).
- **Adaptación nerviosa por pieza o por grupo**, y si se transfiere de un lado al otro.
- **Reparto inicial:** fracciones constantes por pieza sacadas de la literatura
  (`FISIOLOGIA_REFERENCIA.md` §2).

### Respuestas parciales, sin cerrar (2026-11-14, tarde)

- **Temperatura corporal:** se simula; cae o sube según las condiciones exteriores, y la
  calibración se hace contra los tiempos que se quieran obtener. Umbrales de la referencia.
  Aislamiento de la ropa: se decide cuando se llegue a ella.
- **Sangrado:** el cuerpo se divide en partes; cada parte tiene una **curva de severidad** (tirada
  aleatoria 0–1 sobre la curva → caudal de sangrado en mL/min). Brazo: techo bajo. Pierna: curva
  pronunciada (del gemelo a la femoral). Cuello: siempre alto. La **causa** de la herida modifica
  la curva. Umbral de desmayo por desangrado y, algo después, umbral de muerte; el desmayado aún
  puede ser rescatado (vendarlo, subirlo a una carreta). Rechazado: golpear zonas concretas del
  modelo 3D (excesivo para esta fase). Pendiente: catálogo de armadura/tipo de daño (la cota
  frena el cortante, menos el perforante; ya está en documentos anteriores).
- **Sueño:** el jugador elige cuántas horas dormir; la luz que entra, el cansancio acumulado y la
  hora influyen en si se despierta antes. Todo calibración.
- **Fuentes de agua:** estancada, corriente (incluye canales y conductos), manantial, acuífero.
  Según la fuente, hay que hervirla o no. **Turbia** (terrosa, sin riesgo): da un pequeño debuff
  de ánimo; filtrada es agua normal. Destilar no se ofrece (no es potable en exceso y hervir ya
  sirve). Abierto: otro tratamiento que no sea un coñazo y encaje en la época.
- **Alucinaciones por falta de sueño:** sin decidir si se hace algo.
- **Umbrales intermedios de deshidratación, hambre, etc.:** calibración, no decisión de diseño.

### Decididas por Sergio (2026-11-15, 16:44)

- **(1) Día de juego = 1 hora real.** Todo depende del reloj, así que `SetTimeSpeed` sirve para
  probar más rápido. Consecuencia anotada por Claude: 1 min de juego = 2,5 s reales; dormir 8 h
  son 20 min reales; la ventana de hábito de 15 días son 15 h de juego real; morir de hambre
  (30–70 días) son 30–70 h reales.
- **(3) Día fijo, sin estaciones** (estaciones: más trabajo, no hacen falta para el vertical slice).
  Empieza a las **6:00**. Referencia: un día de **mediados de primavera** (cálculo de Claude,
  abajo).
- **(4) Temperatura ambiente: un valor global por hora**, hecho de forma que añadir zonas (sombra,
  interior, agua) o clima no obligue a rehacerlo. El clima no es núcleo.
- **(5) Hora, luz y temperatura las guarda Core**; la capa de Unity solo aplica la luz que Core
  le dice. Dónde exactamente (entidad mundo o servicio): Sergio pide propuesta.
- **(7) Umbrales y ritmos no se editan desde el menú de depuración.** Histéresis donde tenga
  sentido; Sergio decide caso a caso sobre una lista de candidatos.
- **(8) Un sistema por necesidad** (hambre, sed, sueño, temperatura…).
- **(10) Un icono por estado que se sustituye al cambiar de nivel**, como el peso.
- **(12) Componentes:** `EnergyComponent` y `HealthComponent` se reutilizan (aunque sea vaciando
  y reimplementando); `FluidComponent` **no** es para el agua del cuerpo (es para contenedores y
  mezclas). El agua entra por `DigestiveComponent` con **tasa propia** (excepción al drenado común:
  se absorbe más rápido). En `NutritionComponent`, `_thirst` y `_storedWater` se sustituyen por la
  **cantidad de agua del cuerpo** (preferencia de Sergio: en `NutritionComponent`, y que
  `BodyComponent` deduzca el porcentaje); la sed se deriva. Sin cerrar del todo (ver 38).
- **(16) Efectos no numéricos fuera de esta fase.** Se apuntan.
- **(17) Sin barras ni números** para hambre, llenado, etc.: solo el icono de estado, como Project
  Zomboid vanilla. El peso sigue mostrando su número en el inventario. Niveles y bonificaciones:
  Sergio responde sobre una lista.
- **(18) Iconos placeholder** (paquete gratuito); texto en código o tabla, más adelante.
- **(19) Sin tope de iconos.** Si se llena el lateral en juego, se verá.
- **(21) La sangre total es del cuerpo**, en `BodyComponent`, en litros. El contrato de herida se
  define en los hitos. Pendiente: cómo convive con el agua del cuerpo (ver 38).
- **(22) Desmayo:** pantalla negra o muy oscura y borrosa; sin control; dura **mientras se cumpla
  la condición que lo causa**, y despierta lo que la revierte: sueño → recuperar energía;
  desangrado → subir la sangre (cortar la hemorragia y producir, o transfusión); frío → subir la
  temperatura; inanición → casi nada (no se puede alimentar a un inconsciente).
- **(23) Muerte por umbrales en constantes vitales**: sangre, temperatura corporal, energía
  almacenada… Cruzar el límite mata, con o sin desmayo. No hay vida en puntos como causa.
- **(24) Se puede morir desmayado.** Sin estado «crítico» aparte: bastan los umbrales.
- **(25) Fisiología genérica por entidad**; lo que `FatigueStaminaSystem` guarda a nivel de
  sistema hay que generalizarlo.

**Día de referencia (cálculo propio de Claude, fórmula solar estándar, latitud 42,5° N, la de
Jaca/León; 5 de mayo):** ~14,2 h de luz; crepúsculo civil ~31 min; náutico ~69 min; altura máxima
del sol ~64°. En 15 de mayo: 14,6 h. No es una fuente externa; es aritmética.

### Decididas por Sergio (2026-11-15, 19:25)

- **(2) Acelerar el tiempo solo acelera los sistemas de juego**, no la física de Unity.
  (Verificado: `SetTimeSpeed` no lo llama nadie y no se usa `Time.timeScale`.)
- **(5) Hora, luz y temperatura no se guardan:** se deducen del tiempo del reloj con funciones puras
  en una clase de Core (nombre provisional `DayCycle`); Unity pregunta y aplica la luz. La consulta
  de temperatura recibe ya la posición aunque hoy la ignore, para poder añadir zonas y clima sin
  cambiar a quien llama. Si algún día hay clima con estado propio, ese estado irá en un componente.
- **(9) Influencias cruzadas por lectura de componentes**, no por eventos: cada sistema lee los
  componentes que le afectan (p. ej. temperatura lee nutrición). Los eventos, solo para cambios
  discretos (nivel, icono, desmayo). Verificado que hay precedente: `FatigueStaminaSystem` e
  `InventorySystem` leen `MovementComponent`; `WorldInteractionSystem` lee `BodyComponent`.
- **(13) Capacidades de esta fase:** velocidad al moverse, tiempo de las acciones y capacidad de
  carga. Fuerza de trabajo cuando haya algo que talar (Fase 3); puntería, pospuesta.
- **(14) Efectos multiplicativos con suelo** (orden de 0,2; calibración). La inmovilidad total la
  dan estados concretos (desmayo, peso inmóvil), no la acumulación.
- **(15) Calculador por capacidad que pregunta a cada estado** («tirar»), como `WeightCalculator`.
- **(18) Tooltip sí.**
- **(20/39) Piezas y slots de equipo son conceptos distintos, unidos por una tabla de cobertura**
  (slot → piezas que protege), que va con el catálogo de armadura.
- **(7) Histéresis:** no en hambre, sed, peso ni sueño (Sergio: se rellenan a saltos grandes y no
  oscilan). Pendiente de confirmar: temperatura, aguante y desmayo/despertar.
- **(17) Hambre con dos niveles por arriba:** **lleno** (aviso de no comer más, sin efecto negativo)
  y uno de exceso con malestar digestivo y penalización (nombre propuesto por Claude:
  **empachado**). Ambos salen del llenado del contenido digestivo.

### Decididas por Sergio (2026-11-15, 20:15)

- **(11) Estados:** un enum de niveles por estado; **un evento genérico** de cambio de nivel
  (categoría + nivel) en lugar de uno por banda; el peso migra a él; los enums de nivel salen de
  `MVC/View/UI/HUD/…` a una carpeta de dominio de Core.
- **(7) Histéresis sí** en temperatura, desmayo/despertar y aguante (este ya la tiene:
  `FatigueStaminaSystem` restringe correr al vaciarse y solo la levanta con el aguante lleno). **No**
  en hambre, sed, peso ni sueño.
- **(17) Niveles aceptados**, con dos cambios: (1) los textos (tooltip) deben dejar claro que el
  último nivel visible es ya peligroso aunque no haya icono «crítico» (p. ej. hipotermia moderada →
  «te estás congelando vivo»), porque el nivel siguiente es desmayo y el jugador no lo vería venir;
  (2) **sueño sin «descansado»** ni somnolencia al despertar (incómodo de jugar): — · cansado ·
  muy cansado · agotado.
- **(38) Agua y sangre:** agua del cuerpo en mL en `NutritionComponent` (reserva; se quitan
  `_thirst`, `_maxThirst`, `_storedWater`); `BodyComponent` deduce el porcentaje. Sangre en litros
  en `BodyComponent`. Al sangrar se resta también agua: **0,5 L de agua por litro de sangre**
  (2 L de sangre → 1 L de agua). Beber no sube la sangre directamente; la sangre se regenera con el
  tiempo por su cuenta. Los umbrales de desmayo y muerte miran la **sangre efectiva**, deducida:
  sangre × (1 − ~2,8 × % de deshidratación).
- **(26/27) Reserva de energía: opción A o B** (Sergio descarta C: dos reservas solo compensan si
  hay mecánicas de glucógeno, y es otro berenjenal). Claude recomienda A (ver respuesta). El arreglo
  de `_fatPercentage` sale de la 26.
- **(28) Heridas y músculo en un solo componente** por piezas.
- **(29) Fuerza por capas:** piezas en capa 0, músculo y carga en capa 1, entrenamiento en capa 2.
  Sergio ve viable llegar a la capa 1.
- **(30) El entrenamiento sale de golpear**, no de «actividades»: golpear un árbol, una roca o un
  enemigo. Entrena el lado del brazo con el que se golpea (lo que tenga equipado esa mano); una
  herramienta a dos manos (p. ej. caña de pescar, futura) entrena los dos. Cocinar o confeccionar no
  entrenan.
- **(31) Tabla actividad → grupos:** se añade **golpear** a andar, correr, cargar e interactuar.
- **(32) El hábito suma al acumulador** (probabilidad de la franja × constante).
- **(33) Comer por debajo del umbral saciante no toca el acumulador de ayuno.**
- **(34) Sed = déficit de agua (% de masa) compensado por el agua que hay en el estómago.** Tramos:
  sediento entre bien hidratado y deshidratación leve; muy sediento en deshidratación leve;
  deshidratado desde la moderada; en la crítica, desmayo.
- **(35) Una sola temperatura central.** Sin congelación por extremidades. Influye en desmayo,
  muerte y quizá en velocidad o fuerza.
- **(36) El catálogo de daño y armadura entra en esta fase. El rescate de desmayados, solo desde
  el menú de depuración** (no hay NPC hasta Fuero).
- **(37) Despertar:** se elige cuánto dormir; con luz y cansancio bajo cierto umbral te despiertas
  antes; si a la hora elegida sigues muy cansado y está oscuro, sigues durmiendo. El umbral con luz
  pide menos descanso que sin ella. Sergio pide un modelo de parámetros (propuesta de Claude en la
  respuesta; sin cerrar).

### Decididas por Sergio (2026-11-15, 20:28)

- **(41) Regenerar sangre consume agua del cuerpo**, simétrico a sangrar.
- **(26/27) Opción A:** la grasa en kg es el dato guardado (dentro de la composición: grasa, músculo
  por pieza y un resto fijo); peso, % de grasa y kcal de reserva se deducen. `_fatPercentage` y
  `_storedKcal` dejan de guardarse. **Constante: 9 300 kcal/kg (grasa pura).** *Por qué:* el agua
  del tejido adiposo ya la lleva el campo de agua del cuerpo; usar 7 700 la contaría dos veces.
- **(40) `_fats` fuera** de la reserva. `_carbohydrates`: sin cerrar (ver respuesta de Claude).
- **(37) Modelo de despertar aceptado**: deuda de sueño en horas; despierto suma 1 h/h (más
  cuanto más oscuro), durmiendo resta 2 h/h; despertar antes con luz si deuda ≤ U_luz (1 h), a
  oscuras si deuda ≤ U_oscuro (0 h); pasada la hora elegida, si está oscuro y deuda > U_alargar
  (3 h), se sigue durmiendo hasta cumplir una regla de despertar, con tope de +3 h; luz suficiente
  desde el inicio del crepúsculo civil. Valores provisionales. *Intención de Sergio:* la luz hace
  más fácil despertarse; dormir con luz no descansa menos (eso no se modela).

### Decididas por Sergio (2026-11-15, 20:32)

- **(40) `_carbohydrates` fuera** de la reserva, como `_fats`. La energía absorbida va directa al
  balance de la grasa. *Por qué:* con la opción A el balance neto ya decide si se gana o pierde
  grasa; un búfer de glucógeno solo retrasaría un día el uso de la grasa y abriría mecánicas
  (agotamiento muscular) que no hay. Si llegan, se añade entonces. En la reserva de
  `NutritionComponent` quedan proteína, micronutrientes, fibra y agua.
- **(42) Déficit diario para la puerta muscular: acumulador que suma lo que entra y resta lo que se
  gasta, y se reinicia cada día de juego.** *Por qué:* más sencillo que una ventana móvil de 24 h.

## Lista de decisiones de la capa 0 (estado a 2026-11-15, 14:52)

Lista original (2026-11-14), recuperada por Sergio del chat, cruzada con lo decidido. Estado:
**cerrada**, **parcial** (hay respuesta pero falta una parte) o **abierta**. Las numeradas 26 en
adelante surgieron después.

**Reloj del mundo**
1. Minutos reales por día de juego. — cerrada (1 h)
2. Necesidades por tick o por minuto de juego; si acelerar toca la física. — cerrada
3. Ciclo solar: día fijo o estaciones, hora de inicio, crepúsculo. — cerrada
4. Temperatura ambiente: global por hora o por zona; qué la modifica. — cerrada (ver «Decididas»)
5. Quién guarda hora y temperatura (entidad mundo o servicio); qué es «oscurece» en Unity. — cerrada

**Modelo de estado común**
6. Unidades físicas con bandas derivadas o escala abstracta. — cerrada: valores reales, estados
   derivados.
7. Umbrales y ritmos: código, datos o Stack&Go; editables en depuración; histéresis. — cerrada (ver «Decididas»)
8. Sistema genérico con tasas o uno por necesidad. — cerrada (uno por necesidad)
9. Influencias cruzadas: eventos o modificadores. — cerrada
10. Aviso al cruzar banda: evento por banda o genérico. — cerrada
11. Migrar `WeightLevel` al modelo común ahora o después. — cerrada
12. Componentes existentes: reutilizar, reemplazar, retirar. — cerrada (ver «Decididas»)

**Efectos sobre capacidades**
13. Qué capacidades hay; puntería ahora o después. — cerrada
14. Combinar efectos: multiplicar, sumar con tope o el peor; suelo mínimo. — cerrada
15. Agregador único o cada estado empuja su multiplicador. — cerrada
16. Efectos no numéricos (tambaleo, visión, cojera): en esta fase o solo iconos. — cerrada (fuera)

**Iconos cualitativos**
17. Siempre visibles o con desviación; niveles; iconos de bonificación. — cerrada (ver «Decididas»)
18. Placeholders; tooltip; texto en código o tabla. — cerrada (ver «Decididas»)
19. Tope de iconos visibles. — cerrada (sin tope)

**Cuerpo por zonas**
20. Zonas y granularidad; si reutilizan los slots de equipo. — cerrada (ver «Decididas»)
21. Solo zonas vacías o también el contrato de herida; sangre total del cuerpo. — cerrada (ver «Decididas»)

**Desmayo y muerte**
22. Desmayo: pantalla, control, duración, qué despierta, qué lo causa. — cerrada (ver «Decididas»)
23. Muerte: causas; vida en puntos; qué pasa al morir. — cerrada (ver «Decididas»)
24. Morir desmayado; estado crítico intermedio. — cerrada

**Transversal**
25. Fisiología solo del jugador o por entidad. — cerrada (por entidad)

**Surgidas después**
26. Dueño de la reserva de energía y agua (`_storedKcal`/`_storedWater` frente a peso y grasa). — cerrada (A)
27. Arreglo de `_fatPercentage` (depende de 26). — cerrada (no se guarda)
28. Heridas y entrenamiento en un componente o dos. — cerrada (uno)
29. Capa del roadmap para fuerza y entrenamiento. — cerrada
30. Modo de lado por actividad; diestro o zurdo como dato. — cerrada (lado del golpe)
31. Quién rellena la tabla actividad → grupos y cuándo. — cerrada
32. Forma en que el hábito suma al hambre. — cerrada
33. Qué hace una comida bajo el umbral saciante con el acumulador de ayuno. — cerrada
34. Forma de la sed respecto al déficit de agua. — cerrada
35. Una temperatura central o varias. — cerrada
36. Catálogo de daño y armadura; rescate de desmayados en esta fase o con compañeros. — cerrada
37. Regla de despertar antes (luz, cansancio, hora). — cerrada
38. Agua del cuerpo y sangre: dónde vive cada una y cómo conviven (la sangre es en buena parte agua). — cerrada
40. Macros de reserva (`_fats`, `_carbohydrates`) frente a la grasa corporal: con la 26, ¿siguen haciendo falta? — cerrada (fuera las dos)
42. Cómo medir el déficit diario de la puerta muscular. — cerrada (acumulador reiniciado cada día)
41. Si regenerar sangre consume agua del cuerpo (simétrico a sangrar). — cerrada (sí)
39. Relación entre piezas de herida y slots de equipo (cobertura de armadura). — cerrada

## Aplazado a propósito (no se hace en esta fase)

- **Vómito** (decidido por Sergio, 2026-11-15: fuera por ahora, «da mucha pereza»). Catalogado en
  `Docs/MECANICAS.md` §1.1 con etiqueta F. **Problema que habrá que resolver:** con el hambre
  derivada del contenido digestivo, vaciar el estómago a la fuerza deja el llenado a 100 y daría
  hambre, cuando en la realidad hay pereza de comer. **Mecanismo propuesto por Claude, sin
  decidir:** un campo guardado en `DigestiveComponent` con los minutos de aversión restantes, que
  escribe quien vacía el estómago y que decae linealmente; el cálculo del hambre lo lee y la reduce
  mientras sea mayor que cero. No se ha buscado literatura sobre cuánto dura esa pérdida de
  apetito; sería un número de calibración.

## Para la memoria, si da tiempo

- **Simulación del propio juego** con las reglas de la Fase 2 para ver qué consumo calórico medio
  sale de un personaje que vive (agua, comida, actividad), y compararlo con la referencia de la
  FAO. Solo si el juego está integrado y sobra tiempo; encaja con Fuero (NPC con vida propia).
  Para después del TFG, o dentro si cabe.

## Hitos

*(Se escriben cuando termine la capa 0.)*
