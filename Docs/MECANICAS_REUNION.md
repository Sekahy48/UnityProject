# Catálogo de mecánicas — Artisan

Todo lo que se quiere en el juego, a nivel de **mecánica** (lo que hace el jugador), no de
sistema. No es un compromiso: el compromiso lo pone `ROADMAP.md`, que coge mecánicas de aquí
y las ordena en fases.

**Versión para la reunión del 2026-11-12.** Copia de `MECANICAS.md` con el estado de
implementación de cada mecánica en lugar de su origen. La fuente de verdad sigue siendo
`MECANICAS.md`.

## Cómo se lee

Una línea por mecánica: **qué hace el jugador** · **motivación** (por qué lo hace; sin ella la
mecánica sobra) · **referente** · **etiqueta** · **estado**.

- **Etiqueta:** **N** núcleo (sin esto no es Artisan) · **D** deseable · **F** futuro (más
  allá del TFG; no compromete).
- **Estado:** Implementado · Parcial (existe una parte o la base) · No implementado.

## Narración: la primera hora (Sergio, 2026-11-08)

> Al empezar apareces con solo ropa básica, de momento en la nada a lo Minecraft porque aún no
> contemplamos pueblos. Recoges algún palo, alguna piedra, entras a un menú de crafteo básico:
> con 2 piedras puedes hacer cabezas de herramientas, idealmente en un menú de tallado a lo
> TerraFirmaCraft; haces cabezas de cuchillo, de hacha, etc. Buscas ramas largas o las sacas al
> talar árboles pequeños, te haces un vivac; contra un árbol te cuesta una rama larga menos.
> Reúnes pajas para hacerte un catre en el vivac. Talas más árboles para ir teniendo troncos,
> recoges maderitas y piedras, haces más cabezas de herramientas. Te tallas un arpón en el menú
> de crafteo, así puedes pescar peces a duras penas; recoges bayas; con una lanza intentas
> cazar algún conejo para comer. Mientras persigues al conejo aparece un venado que carga
> contra ti asustado, te hace unas heridas en el brazo, las tratas con algo: barro, hojas de
> alguna planta especial del entorno, quién sabe. Te causa dolor, lo cual te genera
> penalización de puntería o de velocidad de acciones. Duermes en el catre cuando se hace de
> noche, ayuda a curar; descansas porque te estabas quedando demasiado sopa: cuando tienes
> sueño tu velocidad de acción es ridícula, por ejemplo. Tras dormir procesas el par de conejos
> que conseguiste, sacas unos huesos pequeños, un par de tendones, carne y piel. Procesas la
> piel con curtido al sol, sin nada complejo porque no tienes para hacer más; los huesos los
> puedes aprovechar para hacer puntas o cosas así; los tendones los secas. Más adelante
> acumulas más tendones y te haces una cuerda resistente, tallas una rama grande para hacer un
> arco corto simple junto a la cuerda de tendón, puedes cazar mejor, empiezas a ahumar,
> encuentras arcilla para hacer cosas, etc.
>
> De momento tiene sentido centrar el gameplay en la supervivencia básica e inicial, aunque a
> futuro se esperan comercio, NPC, herrería, extracción más compleja de recursos, minas, etc.
> Ahora es gameplay de exiliado, o de colono en tierra virgen.

---

## 1. Cuerpo (fisiología) — detallado

La siguiente fase. Más detalle que el resto a propósito.

**Principio:** que el cuerpo se sienta como en la vida real, no como un contador al que hay que
«pincharle» algo periódicamente. Las consecuencias existen desde el principio aunque el detalle
crezca después (enfermedades, temperatura).

### 1.1 Comer
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Comer lo que consigue (bayas, pescado, conejo) | el hambre sube con el tiempo y el esfuerzo | Project Zomboid, Green Hell | N | No implementado |
| Hacer pocas comidas al día (3–4), cada una saciando según su tamaño | que comer se parezca a comer, no a picotear cada poco | — | N | No implementado |
| Obtener cantidades realistas de cada presa (un conejo no da 10 piezas de carne) | que cazar y racionar tengan peso | — | N | No implementado |
| Elegir qué comer según lo que aporta (calorías; quizá proteína, grasa, azúcar) | una dieta pobre trae penalizaciones distintas | Green Hell (macronutrientes) | D | No implementado |
| Cocinar o ahumar la comida | cruda sienta peor o dura menos | Green Hell, The Long Dark | D | No implementado |
| Vigilar el estado de la comida (fresca, rancia, estropeada) | comer algo estropeado tiene consecuencias desde el principio | Project Zomboid | N | No implementado |

### 1.2 Beber
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Beber agua (río, lago, odre) | la sed sube más rápido que el hambre; desmayo progresivo | Green Hell, DayZ | N | No implementado |
| Elegir la fuente y tratar el agua (hervir, quizá filtrar) | el agua peligrosa enferma desde el principio | Green Hell | N | No implementado |

### 1.3 Energía y sueño
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Gestionar el aguante a corto plazo (correr, esquivar) | se agota y no puede seguir | Dark Souls, Project Zomboid | N | Implementado |
| Gestionar el cansancio a largo plazo, que baja más rápido cuanto más oscurece | invita a dormir de noche | Project Zomboid | N | Parcial |
| Descansar o dormir | con sueño, sus acciones son ridículamente lentas | Project Zomboid | N | No implementado |
| Dormir en un sitio mejor (catre frente a suelo) | el sitio decide cuánto descansa y cuánto cura | The Long Dark | D | No implementado |

### 1.4 Heridas
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Recibir heridas por tipo: corte, contusión, perforación | cada fuente (animal, caída, herramienta) hiere distinto | Project Zomboid, DayZ | N | No implementado |
| Heridas en una zona concreta (brazo, pierna…) | la zona decide qué se resiente (brazo: puntería; pierna: andar) | Project Zomboid | N | No implementado |
| Cortes irregulares (desgarros: garras, colmillos, cuernos, enganchones) | se infectan más y cierran peor que un corte limpio; es la herida típica de la fauna | Project Zomboid | N | No implementado |
| Desangrarse si no se trata | la hemorragia baja la sangre hasta el desmayo o la muerte | DayZ | N | No implementado |
| Infección de una herida mal tratada | lo que no se cura bien empeora con los días | Project Zomboid, Green Hell | N (D si se complica) | No implementado |
| Fracturas (caídas, golpes fuertes; entablillar) | una fractura cambia lo que puede hacer durante días | Project Zomboid | N (D si se complica) | No implementado |
| Incapacitación parcial (no poder usar un brazo, cojear) | una herida grave quita capacidades | — | D | No implementado |

### 1.5 Dolor
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Sufrir dolor por las heridas | penaliza puntería y velocidad de las acciones | Project Zomboid | N | No implementado |
| Aliviar el dolor (plantas, reposo) | causar un mal sin poder remediarlo sería frustrante | Green Hell | N | No implementado |

### 1.6 Tratar y curar
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Tratar una herida con lo que tiene (venda, barro, hojas de una planta) | sin tratar, sangra o se infecta | Green Hell | N | No implementado |
| Curar con el tiempo, más rápido durmiendo | las heridas necesitan días | Project Zomboid | N | No implementado |
| Conocer plantas medicinales del entorno | cada planta sirve para una cosa | Green Hell | D | No implementado |

### 1.7 Ánimo
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Notar en el ánimo lo que come y cómo vive: comida rancia, comer siempre lo mismo, ciertas comidas (salazón) | que la monotonía y la miseria se noten, no solo el hambre | Project Zomboid (felicidad, aburrimiento, estrés) | N | No implementado |
| Ganar bonificaciones por hacer las cosas bien (dieta variada, pequeños placeres) | recompensar el buen cuidado, no solo castigar | Project Zomboid | N | No implementado |

**Tensión de diseño (Sergio):** en Artisan el jugador *es* el personaje (inmersión), no lo
maneja desde fuera como en Project Zomboid; eso resta legitimidad a este tipo de mecánicas.
Pero sin ellas el jugador queda como un ser «extracósmico» respecto al mundo. Hay que
equilibrarlas para que no rompan la inmersión ni sean un incordio, sin renunciar a ellas.

### 1.8 Consecuencias globales
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Ver su estado en iconos cualitativos, sin números | un campesino no tiene reloj inteligente | Project Zomboid | N | Parcial |
| Morir de forma coherente (sangre, sed, hambre, frío) | la muerte es la consecuencia última | DayZ | N | No implementado |
| Temperatura corporal: frío y calor según la ropa y el tiempo | en invierno no se puede ir desnudo sin consecuencias | The Long Dark | N (detalle creciente) | No implementado |
| Enfermar (comida estropeada, agua peligrosa) | las malas decisiones tienen consecuencia desde el principio | Green Hell | N (detalle creciente) | No implementado |

---

## 2. Carga
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Organizar el inventario en rejilla por tamaño | el espacio es limitado | Escape from Tarkov, Resident Evil | N | Implementado |
| Cargar con peso y sufrir sobrecarga | el peso frena e inmoviliza | Project Zomboid | N | Implementado |
| Usar contenedores (mochila, zurrón, arcón) | más capacidad a cambio de equiparlos | Escape from Tarkov | N | Implementado |
| Soltar objetos: caen y ruedan con física y se quedan donde se paran | que el mundo responda como un mundo real | Valheim, Skyrim | N | Parcial |
| Colocar objetos con precisión en vez de soltarlos (previsualizar, girar) | dejar las cosas donde uno quiere; la misma mecánica sirve para estructuras | Medieval Dynasty, Valheim | N | No implementado |
| Que la ropa puesta pese | decidido «el equipo pesa, con coeficiente» | — | D | No implementado |

## 3. Recolección y entorno
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Recoger del suelo (palos, piedras, bayas, paja, maderitas) | es la materia prima de todo | Minecraft, TerraFirmaCraft | N | Parcial |
| Talar árboles (pequeños: ramas largas; grandes: troncos) | hace falta madera de varios tamaños | Valheim, TerraFirmaCraft | N | No implementado |
| Pescar: a lanzazo con la lanza, o mejor con arpón | comida temprana; fácil en ríos con peces remontando, difícil en lagos | Medieval Dynasty, Green Hell | N | No implementado |
| Encontrar arcilla | desbloquea la cerámica | TerraFirmaCraft | D | No implementado |
| Minería y extracción compleja | metales para la herrería | TerraFirmaCraft, Vintage Story | F | No implementado |

## 4. Transformación (crafteo)
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Craftear desde un menú que reúne todos los modos de crafteo (lista o botón, tallado…) | convertir lo recogido en útiles; cada modo es una pantalla intercambiable | Minecraft | N | No implementado |
| Tallar piedra en una pantalla interactiva sencilla (cuchillo, hacha…) | la forma la decide el jugador | TerraFirmaCraft (tallado de piedra) | N | No implementado |
| Tallado más profundo (sustituye la pantalla sencilla) | más oficio, más control | TerraFirmaCraft, Vintage Story | F | No implementado |
| Montar herramientas: cabeza + mango (+ atadura) | las herramientas son piezas | TerraFirmaCraft | N | No implementado |
| Hacer una lanza rudimentaria (madera, como mucho punta de piedra) y un arpón de pesca, objetos distintos | el arpón pesca mejor, pero la lanza también sirve | Medieval Dynasty | N | No implementado |
| Despiezar presas: carne, piel, huesos, tendones | el animal es materia prima, no solo comida | Green Hell | N | No implementado |
| Despiezar y trabajar mejor en una estructura de trabajo que en el suelo | se aprovecha más | — | D | No implementado |
| Procesos con tiempo: curtir al sol, secar tendones, ahumar | lo bueno tarda; el proceso es la gracia | — | N | No implementado |
| Hacer cuerda con tendones, arco corto, puntas de hueso | la cadena de mejoras | Green Hell | N | No implementado |
| Cerámica con arcilla | recipientes y cocina | TerraFirmaCraft | D (intención: TFG) | No implementado |
| Herrería | herramientas de metal | Medieval Dynasty, TerraFirmaCraft | F | No implementado |
| Atajos delegables a aldeanos | con colonia, el proceso se reparte | — | F | No implementado |

## 5. Construcción
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Hacer un vivac con ramas largas (contra un árbol cuesta menos) | refugio para dormir; el entorno abarata | Green Hell | N | No implementado |
| Hacer un catre de paja | dormir mejor | Green Hell | N | No implementado |
| Construcción modular (cimientos, paredes, techos) | crucial a futuro; inviable para el TFG con todo lo demás | Valheim, ARK | F | No implementado |

## 6. Fauna y caza
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Cazar presas que huyen (conejo) | carne y materiales; cuesta | Green Hell, Red Dead Redemption 2 | N | No implementado |
| Enfrentarse a animales que cargan si se asustan (venado, jabalí) | la fauna también hiere | The Long Dark | N | No implementado |
| Cazar con lanza; luego con arco | mejores armas, mejor caza | Green Hell | N | No implementado |
| Depredadores y territorios | presión del entorno | — | D | No implementado |
| Ganadería y cría | recursos estables | Medieval Dynasty | F | No implementado |

## 7. Tiempo y clima
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Vivir un ciclo de día y noche | de noche se duerme y se ve peor | casi todos | N | No implementado |
| Notar la temperatura del mundo (día y noche; estaciones) | va de la mano de la temperatura corporal | The Long Dark | N (detalle creciente) | No implementado |
| Aprovechar el sol para procesos (curtir, secar) | algunos procesos dependen del tiempo | — | D | No implementado |
| Clima con efecto real (lluvia, nieve) y cosechas por estación | el entorno cambia lo que se puede hacer | — | F | No implementado |

## 8. Personas
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Hablar con NPC básicos (diálogo) | información, comercio | — | D | No implementado |
| NPC con supervivencia y estado propio (hambre, miedo) | el mundo vive sin el jugador | — | F | No implementado |
| Rasgos de personalidad (p. ej. nocturno: invierte o anula la penalización de la noche) | individualidad; encaja más en NPC que en el jugador | — | F | No implementado |
| Reputación y memoria de los NPC | lo que haces tiene consecuencias | — | F | No implementado |
| Reclutar aldeanos por confianza | base de la colonia | — | F | No implementado |
| Identidad y apodos emergentes | reconocimiento social | — | F | No implementado |

## 9. Economía
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Comerciar | conseguir lo que no puede hacer | Medieval Dynasty | F | No implementado |
| Mercado emergente (órdenes de compra y venta) | precios por oferta y demanda | Albion Online | F | No implementado |

## 10. Conflicto
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Combate cuerpo a cuerpo ligado a las heridas | el daño es una herida concreta, no puntos de vida | Mordhau, Kingdom Come | F | No implementado |
| Facciones con objetivos propios | política emergente | Mount & Blade II: Bannerlord | F | No implementado |

## 11. Colonia
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Asignar tareas a aldeanos y poseer un NPC | el jugador no puede hacerlo todo | Medieval Dynasty, RimWorld | F | No implementado |
| Agricultura | comida estable | Medieval Dynasty | F | No implementado |
| Vista RTS | comandar y gestionar desde arriba | Manor Lords, 0 A.D. | F | Parcial |

## 12. Meta
| Hace | Motivación | Referente | Et. | Estado |
|---|---|---|---|---|
| Empezar en un mundo vacío con mapa fijo | aún no hay pueblos | Minecraft | N | No implementado |
| Guardar y cargar la partida | sesiones de juego reales; sin esto no se puede jugar más de una vez seguida | — | N | No implementado |
| Mundo procedural | rejugabilidad | — | F | No implementado |
| Muerte permanente con legado | consecuencias reales | — | F | No implementado |
| Cooperativo de 4 a 8 jugadores y más allá | jugar con otros | — | F | No implementado |

---

## Investigación pendiente

- **El agua en la sociedad feudal y otras.** Qué fuentes se consideraban seguras (agua
  corriente frente a lago, pozo, lluvia), si bastaba con hervirla o se filtraba también (sabor
  terroso), qué se consideraba normal. Objetivo: que las fuentes de agua sean fidedignas y
  decidir si el agua de mala calidad, además de enfermar, afecta al ánimo.

## Ideas sueltas (sin área ni etiqueta todavía)

- **Pequeños placeres de época** como fuente de ánimo (el equivalente medieval a leer o fumar
  en Project Zomboid). Fumar en pipa al estilo de la Comarca de Tolkien no encaja en una
  ambientación histórica; buscar equivalentes de época.

