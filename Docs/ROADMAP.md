# Roadmap — Artisan

Qué se hace y en qué orden, a nivel de **mecánicas**. Las mecánicas salen de `MECANICAS.md`;
los **sistemas** de cada fase se definen al empezarla, en su propio documento de hitos (como
`Assets/Code/FASE1_HITOS.md` para la Fase 1).

**mk1 (2026-11-11).** Elegida la alternativa B de las tres valoradas (ver «Por qué este
orden»). La siguiente fase está detallada; las demás, en lo esencial. Se reajustarán al
llegar a cada una.

## Principios

1. **Cada fase termina siendo jugable** y le da al jugador verbos nuevos. Su cierre tiene
   criterios comprobables, no «cuando esté».
2. **Se respetan las dependencias reales:** una mecánica se hace cuando existe lo que la
   alimenta (las necesidades, con algo que consumir; las heridas, con algo que las cause).
3. **La fisiología, en profundidad; lo demás, en versión vertical slice**, salvo lo que Sergio
   marque para profundizar.
4. **Sin fechas fijas por fase.** Las estimaciones orientan, no obligan; el único hito de
   calendario es la presentación del TFG (julio de 2027).
5. **El guardado es progresivo.** La base se hace en la Fase 1.5; desde ahí, **«su estado se
   guarda y se carga» es un criterio de cierre de cada fase**, en vez de una deuda acumulada
   para el final.
6. **Los hitos de cada fase se construyen en modo socrático:** Claude pregunta, Sergio
   decide.

## Fases cerradas

| Fase | Qué fue | Documento |
|---|---|---|
| 0 | Revisión del código heredado; separación en `Core/` (C# puro) y `Unity/`. | `PR_FASE_0.md` |
| 1 | Inventario: catálogo de ítems, sublotes, peso y rejilla, equipo, interfaz tetris, interacción con el mundo, pulido y tests. Cerrada el 2026-10-29. | `Assets/Code/FASE1_HITOS.md` |

---

## Fase 1.5 — Mundo físico y guardado base (siguiente)

Fase corta y transversal, antes de la Fase 2 (decidido el 2026-11-11).

**Experiencia:** lo que sueltas cae y rueda como un objeto real, y la partida se puede guardar
y retomar.

**Verbos nuevos:** guardar · cargar (soltar ya existía; ahora con física).

**Mecánicas:**

- **Soltar con física:** lo que se suelta cae, rueda y se queda donde se para.
- **Guardar y cargar la base:** el jugador (posición, estado), su inventario y equipo, y los
  objetos del mundo (lo soltado, los arcones…).

**Decisiones previstas para sus hitos:** quién manda en la posición de un objeto mientras se
mueve (con física manda Unity; quieto, el Core), y cómo se avisa al Core cuando se para; formato
del guardado, identificadores estables de las entidades y versión del archivo; qué se guarda y
qué se reconstruye al cargar.

**Fuera, a propósito:** **colocar** objetos con precisión en vez de soltarlos va en la Fase 5,
junto al vivac y el catre: colocar un objeto y colocar una estructura son la misma mecánica
(previsualizar, comprobar el terreno, girar), y hacerla ahora solo para objetos obligaría a
rehacerla.

**Cierre:** lo soltado cae con física sin tirones y queda donde se para; guardar, salir y
cargar devuelve al jugador, su inventario y los objetos del mundo exactamente como estaban;
tests del Core del guardado.

---

## Fase 2 — Cuerpo I: necesidades

**Experiencia de juego:** el personaje tiene un cuerpo que pide cosas. Pasa hambre, sed, frío
y sueño, y se nota: en lo que puede hacer y en su ánimo. Lo resuelve comiendo, bebiendo,
abrigándose y durmiendo, con comida y agua que ya están en el mundo (todavía no se consiguen
jugando: eso llega en la Fase 3).

**Verbos nuevos:** comer · beber · tratar el agua · dormir · abrigarse.

**Principio de diseño:** que el cuerpo se sienta real, no un contador que hay que «pinchar» cada
poco. Las consecuencias existen desde el principio aunque el detalle crezca después.

### Mecánicas incluidas (de `MECANICAS.md`)

| Área | Mecánica | Et. |
|---|---|---|
| 1.1 Comer | Comer lo que hay; pocas comidas al día (3–4) que sacian según su tamaño; cantidades realistas | N |
| 1.1 Comer | Estado de la comida (fresca, rancia, estropeada) y su consecuencia | N |
| 1.2 Beber | Beber; sed que sube más rápido que el hambre; desmayo progresivo | N |
| 1.2 Beber | Elegir la fuente y tratar el agua (hervir; quizá filtrar) | N |
| 1.3 Energía | Aguante a corto plazo (ya existe) y cansancio a largo plazo, que baja más rápido cuanto más oscurece | N |
| 1.3 Energía | Dormir; con sueño, acciones ridículamente lentas | N |
| 1.7 Ánimo | Ánimo afectado por lo que come y cómo vive (rancio, monotonía); bonificaciones por hacerlo bien | N |
| 1.8 Consecuencias | Iconos cualitativos de estado, sin números (se generaliza lo hecho para el peso) | N |
| 1.8 Consecuencias | Temperatura corporal según ropa y tiempo (detalle creciente) | N |
| 1.8 Consecuencias | Enfermar por comida estropeada o agua peligrosa (detalle creciente) | N |
| 1.8 Consecuencias | Morir de sed, hambre o frío | N |
| 7 Tiempo | Ciclo de día y noche; temperatura del mundo | N |
| 1.1 Comer | Elegir qué comer por lo que aporta (calorías; quizá proteína, grasa, azúcar) | D |

### Fuera de esta fase (a propósito)

- **Heridas, dolor, tratamiento, curación:** Fase 4, junto a la fauna que las causa.
- **Cocinar y ahumar:** Fase 5 (procesos con tiempo). En la Fase 2, la comida cruda o
  preparada ya viene colocada en el mundo para poder probar.
- **Dormir mejor en catre o vivac:** Fase 5. En la Fase 2 se duerme en el suelo.
- **Conseguir la comida y el agua jugando** (bayas, pesca, recoger): Fase 3.

### Criterios de cierre

1. En una partida, el personaje pasa hambre, sed, frío y sueño a un ritmo creíble, y cada
   necesidad se resuelve con algo colocado en el mundo.
2. Cada estado se ve con su icono cualitativo y su descripción, sin números.
3. Comer algo estropeado o beber agua sin tratar tiene una consecuencia visible.
4. La noche se nota: oscurece, refresca, el cansancio baja más rápido y dormir lo recupera.
5. Descuidar el cuerpo lleva hasta la muerte por sed, hambre o frío.
6. El ánimo cambia con la monotonía y la calidad de la comida, sin volverse un incordio.
7. Tests del Core para cada necesidad y sus umbrales.
8. El estado del cuerpo y del reloj del mundo se guarda y se carga.

### Antes o al principio de la fase

- **Investigación del agua** (`MECANICAS.md`, «Investigación pendiente»): fuentes seguras en
  la Edad Media, hervir o filtrar, qué era normal. Condiciona cómo se tratan las fuentes.
- **Documento de hitos de la Fase 2** con sus sistemas, siguiendo la forma de trabajo de
  `CLAUDE.md` (primero la lista de decisiones, luego la propuesta cerrada).

---

## Fase 3 — Recolección y crafteo

**Experiencia:** sobrevivir con lo que da el entorno y hacerse las primeras herramientas.

**Verbos nuevos:** recoger · talar · tallar · montar · pescar.

**Mecánicas:** recoger del suelo (palos, piedras, bayas, paja, maderitas); talar árboles
(ramas largas, troncos); menú de crafteo que reúne todos los modos, con el tallado de piedra
interactivo y sencillo; montar herramientas (cabeza + mango + atadura); lanza rudimentaria y
arpón de pesca como objetos distintos; pescar (fácil en ríos con peces remontando, difícil en
lagos). Bayas y pesca son la primera comida que se consigue jugando.

**Cierre:** se sobrevive a base de recolectar y pescar; se hacen cuchillo, hacha, lanza y
arpón desde cero.

## Fase 4 — Cuerpo II: heridas y fauna

**Experiencia:** el entorno también hace daño, y hay que curarse.

**Verbos nuevos:** cazar · despiezar · tratar · curar.

**Mecánicas:** heridas por tipo (corte, con la marca «irregular» para desgarros; contusión;
perforación) y por zona; hemorragia; infección; fracturas; dolor y su alivio; tratar con lo
que hay (venda, barro, hojas) y curar con el tiempo; fauna: conejo que huye, venado o jabalí
que cargan si se asustan; cazar con lanza; despiezar (carne, piel, huesos, tendones, en
cantidades realistas).

**Cierre:** cazar puede salir mal; una carga deja una herida concreta en una zona concreta,
con sus efectos; se trata y se cura.

**Riesgo:** es la fase más grande. Si crece, se parte en 4a (heridas y tratamiento, con
causas de prueba) y 4b (fauna y caza).

## Fase 5 — Refugio y vertical slice

**Experiencia:** la primera hora de la narración, entera y pulida.

**Verbos nuevos:** colocar · construir el refugio · procesar.

**Mecánicas:** colocar objetos y estructuras (una sola mecánica: previsualizar, comprobar el
terreno, girar); vivac con ramas largas (más barato contra un árbol) y catre de paja (mejor
sueño, mejor curación); procesos con tiempo (curtir al sol, secar tendones, ahumar);
cocinar; cuerda de tendón, arco corto y puntas de hueso; contenido, equilibrado y pulido de
la primera hora.

**Cierre:** la narración de la primera hora se puede jugar de principio a fin.

---

## Líneas de corte del TFG

| Nivel | Hasta | Qué se presenta |
|---|---|---|
| **Mínimo entregable** | Fase 3 | Inventario + mundo físico y guardado + necesidades del cuerpo + recolección y crafteo: se sobrevive jugando. |
| **Objetivo** | Fase 5 | La vertical slice: la primera hora completa. |
| **Ampliación** | — | Cerámica, estructuras de trabajo (despiece más eficiente), plantas medicinales, depredadores, NPC básicos con diálogo (enlazando con Fuero). |

## Estimación orientativa

En semanas de trabajo efectivo al ritmo de verano (el de la Fase 1). Fiabilidad baja: son
estimaciones por analogía, no un cálculo.

| Fase | Semanas |
|---|---|
| 1.5 | 2–3 |
| 2 | 4–6 |
| 3 | 5–8 |
| 4 | 6–10 |
| 5 | 3–5 |
| Memoria del TFG | 4–6 |

Las fases 1.5–5 suman unas 20–32 semanas. Durante el curso el ritmo efectivo es menor, así que
el **objetivo va justo** para julio; el **mínimo** es alcanzable con margen.

## Por qué este orden

Se valoraron tres alternativas (2026-11-11):

- **A. Fisiología entera primero, luego el ciclo de juego.** Descartada: las heridas y el
  hambre se diseñarían sin sus causas reales (fauna, comida que se consigue jugando) y habría
  que reequilibrarlas después; nada jugable de verdad hasta muy tarde.
- **B. Fisiología partida e intercalada con lo que la alimenta.** **Elegida:** cada fase
  termina siendo jugable, respeta las dependencias, la vertical slice crece fase a fase y la
  fisiología sigue siendo profunda (en dos partes).
- **C. Primero toda la slice en superficie, luego profundizar.** Descartada: obliga a rehacer
  casi todo lo superficial y choca con la forma de trabajar del proyecto (sistemas terminados,
  nada al tuntún).

## Después del TFG

Todo lo etiquetado F en `MECANICAS.md`, sin orden comprometido: construcción modular,
herrería y minería, NPC con vida propia (Fuero), comercio y economía, colonia, conflicto,
clima completo, mundo procedural y cooperativo.
