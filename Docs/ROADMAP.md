# Roadmap — Artisan

Qué se hace y en qué orden, a nivel de **mecánicas**. Las mecánicas salen de `MECANICAS.md`;
los **sistemas** de cada fase se definen al empezarla, en su propio documento de hitos (como
`Assets/Code/FASE1_HITOS.md` para la Fase 1).

**mk2 (2026-11-12).** Revisado tras la segunda reunión con David: la fisiología entera pasa a
la Fase 2, ordenada por su «tronco»; el guardado de partida se aplaza a después del TFG; las
líneas de corte son más defensivas. La historia de los cambios, en «Por qué este orden».

## Principios

1. **Cada fase termina siendo jugable** y le da al jugador verbos nuevos. Su cierre tiene
   criterios comprobables, no «cuando esté».
2. **Se respetan las dependencias reales:** una mecánica se hace cuando existe lo que la
   alimenta. Cuando lo que falta es solo la causa (un animal que hiera, una planta que
   recoger), se sustituye por **herramientas de depuración** y la mecánica no espera.
3. **Primero el tronco, luego las ramas.** Dentro de cada fase se hace antes lo que no
   depende de nada y de lo que todo depende. Si aprieta el tiempo, se recortan ramas, nunca
   tronco.
4. **La fisiología, en profundidad; lo demás, en versión vertical slice**, salvo lo que Sergio
   marque para profundizar.
5. **Sin fechas fijas por fase.** Las estimaciones orientan, no obligan; el único hito de
   calendario es la presentación del TFG (julio de 2027).
6. **Sin guardado de partida durante el TFG.** La demo se monta con **escenarios preparados**
   (posición, inventario, objetos cercanos), que además sirven para probar. El diseño del
   guardado ya está hecho y se conserva para después (`FASE1_5_HITOS.md`).
7. **Los hitos de cada fase se construyen en modo socrático:** Claude pregunta, Sergio
   decide.

## Fases cerradas

| Fase | Qué fue | Documento |
|---|---|---|
| 0 | Revisión del código heredado; separación en `Core/` (C# puro) y `Unity/`. | `PR_FASE_0.md` |
| 1 | Inventario: catálogo de ítems, sublotes, peso y rejilla, equipo, interfaz tetris, interacción con el mundo, pulido y tests. Cerrada el 2026-10-29; fusionada en `main` (PR #2). | `Assets/Code/FASE1_HITOS.md` |
| 1.5 | Mundo físico: el jugador se mueve por física; lo soltado cae, rueda y vuelca con masa = peso (el jugador, cuerpo + carga); sale desde el centro del jugador con un impulso según su masa, así que nunca atraviesa una pared; colisión con la envoltura convexa del modelo; lo que se sale del mundo vuelve. Del 2026-11-11 al 14 (estimado: 1–2 semanas). | `Assets/Code/FASE1_5_HITOS.md` |

---

## Fase 2 — Fisiología (siguiente)

**Experiencia:** el personaje tiene un cuerpo que pide cosas y que se hace daño. Pasa hambre,
sed, frío y sueño; se hiere, le duele, se cura o empeora. Todo se nota en lo que puede hacer y
en su ánimo.

**Principio de diseño:** que el cuerpo se sienta real, no un contador que hay que «pinchar» cada
poco. Las consecuencias existen desde el principio aunque el detalle crezca después.

**Cómo se prueba sin las fases siguientes:** un **menú de depuración** (causar una herida de un
tipo en una zona, poner ítems, ajustar necesidades) y **escenarios de prueba** preparados.
Comida, agua, vendas, barro u hojas son ítems del catálogo; todavía no se consiguen jugando.

### Capa 0 — Tronco (no depende de nada; todo depende de ella)

1. **Reloj del mundo:** tiempo de juego, día y noche, temperatura ambiente.
2. **Modelo de estado común:** valores que cambian con el tiempo, umbrales, niveles y
   efectos. Hambre, sed, sueño, dolor, temperatura y ánimo siguen el mismo patrón.
3. **Efectos sobre capacidades:** velocidad al moverse, velocidad de las acciones, puntería.
   Generaliza lo que ya hace el peso con el movimiento.
4. **Iconos de estado cualitativos,** generalizando los de peso.
5. **Cuerpo por zonas** (brazo, pierna…), base de las heridas.
6. **Desmayo y muerte.**

### Capa 1 — Depende solo del tronco

| Mecánica | Et. |
|---|---|
| Hambre y comer: pocas comidas al día que sacian según su tamaño; cantidades realistas | N |
| Sed y beber (odre, fuente de agua colocada); desmayo progresivo | N |
| Aguante a corto plazo (existe), cansancio a largo plazo que baja más rápido cuanto más oscurece, sueño y dormir | N |
| Temperatura corporal según la ropa y el tiempo | N |
| Heridas por tipo (corte, con la marca «irregular» para desgarros; contusión; perforación), por zona, con hemorragia; causadas desde el menú de depuración | N |

### Capa 2 — Depende de la capa 1, dentro de la fisiología

| Mecánica | Et. |
|---|---|
| Dolor y aliviarlo | N |
| Tratar heridas (venda, barro, hojas como ítems) y curar con el tiempo, más rápido durmiendo | N |
| Estado de la comida (fresca, rancia, estropeada) y enfermar por comida estropeada o agua peligrosa | N |
| Ánimo: monotonía, comida rancia; bonificaciones por hacerlo bien | N |
| Infección y fracturas | N (D si se complica) |
| Incapacitación parcial | D |
| Elegir qué comer según lo que aporta (nutrientes) | D |

### Fuera de esta fase, con sustituto provisional

| Depende de | Mecánica | Mientras tanto |
|---|---|---|
| Fuego y procesos (Fase 5) | Cocinar, ahumar, hervir el agua | Agua tratada y comida preparada como ítems; las consecuencias del agua mala sí existen. |
| Recolección (Fase 3) | Recoger plantas medicinales del entorno | Ítems puestos con el menú de depuración. |
| Estructuras (Fase 5) | Dormir mejor en catre | Se duerme en el suelo. |
| Fauna (Fase 4) | Heridas causadas por animales | Menú de depuración; la fauna solo tendrá que llamar al sistema de heridas. |

### Orden de recorte si aprieta el tiempo

De las ramas hacia el tronco: nutrientes → incapacitación parcial → fracturas → infección →
detalle del ánimo → detalle de la enfermedad. El tronco y la capa 1 no se recortan.

### Antes de empezar

- **Investigación del agua** (`MECANICAS.md`, «Investigación pendiente»).
- **Documento de hitos de la Fase 2,** en modo socrático.

### Cierre

1. En una partida preparada, el personaje pasa hambre, sed, frío y sueño a un ritmo creíble, y
   cada necesidad se resuelve con algo del mundo o del inventario.
2. Una herida causada desde el menú de depuración tiene tipo, zona, sangrado, dolor y efectos
   en las capacidades; se trata y se cura, o empeora si no.
3. Cada estado se ve con su icono cualitativo y su descripción, sin números.
4. La noche se nota: oscurece, refresca, el cansancio baja más rápido y dormir lo recupera.
5. Descuidar el cuerpo lleva al desmayo y a la muerte.
6. Tests del Core para cada necesidad, cada tipo de herida y sus umbrales.

---

## Fase 3 — Recolección y crafteo

**Experiencia:** sobrevivir con lo que da el entorno y hacerse las primeras herramientas.

**Mecánicas:** recoger del suelo (palos, piedras, bayas, paja, maderitas, plantas
medicinales); talar árboles; menú de crafteo que reúne todos los modos, con el tallado de
piedra interactivo y sencillo; montar herramientas; lanza rudimentaria y arpón de pesca;
pescar. Bayas, plantas y pesca sustituyen a los ítems de depuración de la Fase 2.

**Cierre:** se sobrevive a base de recolectar y pescar; se hacen cuchillo, hacha, lanza y arpón
desde cero.

## Fase 4 — Fauna y caza

**Experiencia:** el entorno está vivo y puede hacer daño.

**Mecánicas:** conejo que huye; venado o jabalí que cargan si se asustan y hieren (llamando al
sistema de heridas de la Fase 2); cazar con lanza; despiezar (carne, piel, huesos, tendones, en
cantidades realistas).

**Cierre:** cazar puede salir mal; una carga deja una herida concreta en una zona concreta.

## Fase 5 — Refugio y vertical slice

**Experiencia:** la primera hora de la narración, entera y pulida.

**Mecánicas:** colocar objetos y estructuras (una sola mecánica); vivac y catre; procesos con
tiempo (curtir, secar, ahumar); cocinar y hervir agua con fuego; cuerda de tendón, arco y
puntas de hueso; escenario de demo, equilibrado y pulido.

**Cierre:** la narración de la primera hora se puede jugar de principio a fin desde un
escenario de inicio.

---

## Líneas de corte del TFG

| Nivel | Hasta | Qué se presenta |
|---|---|---|
| **Mínimo entregable** | Fase 1.5 + Fase 2 hasta la capa 1 | El tronco de la fisiología con todas las necesidades y las heridas básicas, sobre el inventario y el mundo físico. |
| **Objetivo** | Fase 2 completa + Fase 3 | Fisiología completa y se sobrevive recolectando y crafteando. |
| **Ampliación** | Fases 4 y 5 | Fauna y caza; refugio y la primera hora completa. |

## Estimación orientativa

En semanas de trabajo efectivo al ritmo de verano (el de la Fase 1). Fiabilidad baja: son
estimaciones por analogía, no un cálculo.

| Fase | Semanas |
|---|---|
| 1.5 | 1–2 |
| 2 | 9–14 |
| 3 | 5–8 |
| 4 | 4–6 |
| 5 | 3–5 |
| Memoria del TFG | 4–6 |

La Fase 2 es la más grande: más o menos la mitad del tiempo que queda hasta julio. Encaja con
lo que pide David (fisiología primero y bien hecha), y por eso las Fases 4 y 5 son ampliación.

## Por qué este orden

**mk1 (2026-11-11).** Se valoraron tres alternativas:

- **A. Fisiología entera primero, luego el ciclo de juego.** Descartada entonces porque las
  heridas y el hambre se diseñarían sin sus causas reales.
- **B. Fisiología partida e intercalada con lo que la alimenta.** Elegida en mk1.
- **C. Primero toda la slice en superficie, luego profundizar.** Descartada: obliga a rehacer
  y choca con la forma de trabajar del proyecto.

**mk2 (2026-11-12), tras la reunión con David.** David prioriza la fisiología y teme por el
alcance; pide identificar el tronco y centrarse en él. Al ordenar la fisiología por capas de
dependencia se vio que las causas que faltaban (animales que hieren, plantas que recoger) se
pueden sustituir con depuración, así que la fisiología entera cabe en una fase y la objeción a
A desaparece. El resultado es A con el principio del tronco: fisiología completa en la Fase 2,
por capas, y las demás fases conectan con ella. El guardado de partida sale del TFG (principio
6) y las líneas de corte pasan a ser defensivas.

## Después del TFG

Guardado de partida (diseño en `FASE1_5_HITOS.md`), lanzar objetos, y todo lo etiquetado F en
`MECANICAS.md`: construcción modular, herrería y minería, NPC con vida propia (Fuero), comercio
y economía, colonia, conflicto, clima completo, mundo procedural y cooperativo.
