# Catálogo de mecánicas — Artisan

Todo lo que se quiere en el juego, a nivel de **mecánica** (lo que hace el jugador), no de
sistema. No es un compromiso: el compromiso lo pone `ROADMAP.md`, que coge mecánicas de aquí
y las ordena en fases.

**Versión mk1, revisión 2 (2026-11-11).** Borrador de Claude a partir de la narración de la
primera hora (abajo), `Artisan_Reporte_Contexto.docx` y lo ya implementado, corregido con las
notas de Sergio del 2026-11-11.

## Cómo se lee

Una línea por mecánica: **qué hace el jugador** · **motivación** (por qué lo hace; sin ella la
mecánica sobra) · **referente** · **etiqueta** · **origen**.

- **Etiqueta:** **N** núcleo (sin esto no es Artisan) · **D** deseable · **F** futuro (más
  allá del TFG; no compromete).
- **Origen:** 📖 narración de la primera hora · 🗣️ notas de Sergio · 📄 reporte de contexto ·
  ✅ ya implementado · 🔶 implementado en parte.
- Las propuestas de Claude sin resolver **no** están en las tablas: van en «Pendiente de
  Sergio», al final.

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
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Comer lo que consigue (bayas, pescado, conejo) | el hambre sube con el tiempo y el esfuerzo | Project Zomboid, Green Hell | N | 📖 |
| Hacer pocas comidas al día (3–4), cada una saciando según su tamaño | que comer se parezca a comer, no a picotear cada poco | — | N | 🗣️ |
| Obtener cantidades realistas de cada presa (un conejo no da 10 piezas de carne) | que cazar y racionar tengan peso | — | N | 🗣️ |
| Elegir qué comer según lo que aporta (calorías; quizá proteína, grasa, azúcar) | una dieta pobre trae penalizaciones distintas | Green Hell (macronutrientes) | D | 📄 |
| Cocinar o ahumar la comida | cruda sienta peor o dura menos | Green Hell, The Long Dark | D | 📖 |
| Vigilar el estado de la comida (fresca, rancia, estropeada) | comer algo estropeado tiene consecuencias desde el principio | Project Zomboid | N | 🗣️ |

### 1.2 Beber
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Beber agua (río, lago, odre) | la sed sube más rápido que el hambre; desmayo progresivo | Green Hell, DayZ | N | 📄 · 🗣️ · odre 🔶 |
| Elegir la fuente y tratar el agua (hervir, quizá filtrar) | el agua peligrosa enferma desde el principio | Green Hell | N | 🗣️ · ver «Investigación» |

### 1.3 Energía y sueño
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Gestionar el aguante a corto plazo (correr, esquivar) | se agota y no puede seguir | Dark Souls, Project Zomboid | N | ✅ |
| Gestionar el cansancio a largo plazo, que baja más rápido cuanto más oscurece | invita a dormir de noche | Project Zomboid | N | 🗣️ · cansancio 🔶 |
| Descansar o dormir | con sueño, sus acciones son ridículamente lentas | Project Zomboid | N | 📖 |
| Dormir en un sitio mejor (catre frente a suelo) | el sitio decide cuánto descansa y cuánto cura | The Long Dark | D | 📖 |

### 1.4 Heridas
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Recibir heridas por tipo: corte, contusión, perforación | cada fuente (animal, caída, herramienta) hiere distinto | Project Zomboid, DayZ | N | 📄 · 📖 |
| Heridas en una zona concreta (brazo, pierna…) | la zona decide qué se resiente (brazo: puntería; pierna: andar) | Project Zomboid | N | 📖 |
| Cortes irregulares (desgarros: garras, colmillos, cuernos, enganchones) | se infectan más y cierran peor que un corte limpio; es la herida típica de la fauna | Project Zomboid | N | 🗣️ (un corte con la marca «irregular»; en la interfaz se ve como desgarro) |
| Desangrarse si no se trata | la hemorragia baja la sangre hasta el desmayo o la muerte | DayZ | N | 📄 |
| Infección de una herida mal tratada | lo que no se cura bien empeora con los días | Project Zomboid, Green Hell | N (D si se complica) | 🗣️ |
| Fracturas (caídas, golpes fuertes; entablillar) | una fractura cambia lo que puede hacer durante días | Project Zomboid | N (D si se complica) | 🗣️ |
| Incapacitación parcial (no poder usar un brazo, cojear) | una herida grave quita capacidades | — | D | 📄 |

### 1.5 Dolor
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Sufrir dolor por las heridas | penaliza puntería y velocidad de las acciones | Project Zomboid | N | 📖 |
| Aliviar el dolor (plantas, reposo) | causar un mal sin poder remediarlo sería frustrante | Green Hell | N | 🗣️ |

### 1.6 Tratar y curar
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Tratar una herida con lo que tiene (venda, barro, hojas de una planta) | sin tratar, sangra o se infecta | Green Hell | N | 📖 · venda 🔶 |
| Curar con el tiempo, más rápido durmiendo | las heridas necesitan días | Project Zomboid | N | 📖 |
| Conocer plantas medicinales del entorno | cada planta sirve para una cosa | Green Hell | D | 📖 |

### 1.7 Ánimo
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Notar en el ánimo lo que come y cómo vive: comida rancia, comer siempre lo mismo, ciertas comidas (salazón) | que la monotonía y la miseria se noten, no solo el hambre | Project Zomboid (felicidad, aburrimiento, estrés) | N | 🗣️ |
| Ganar bonificaciones por hacer las cosas bien (dieta variada, pequeños placeres) | recompensar el buen cuidado, no solo castigar | Project Zomboid | N | 🗣️ |

**Tensión de diseño (Sergio):** en Artisan el jugador *es* el personaje (inmersión), no lo
maneja desde fuera como en Project Zomboid; eso resta legitimidad a este tipo de mecánicas.
Pero sin ellas el jugador queda como un ser «extracósmico» respecto al mundo. Hay que
equilibrarlas para que no rompan la inmersión ni sean un incordio, sin renunciar a ellas.

### 1.8 Consecuencias globales
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Ver su estado en iconos cualitativos, sin números | un campesino no tiene reloj inteligente | Project Zomboid | N | 🔶 (estados de peso) |
| Morir de forma coherente (sangre, sed, hambre, frío) | la muerte es la consecuencia última | DayZ | N | 📄 |
| Temperatura corporal: frío y calor según la ropa y el tiempo | en invierno no se puede ir desnudo sin consecuencias | The Long Dark | N (detalle creciente) | 🗣️ · aislamiento de la ropa en los datos 🔶 |
| Enfermar (comida estropeada, agua peligrosa) | las malas decisiones tienen consecuencia desde el principio | Green Hell | N (detalle creciente) | 🗣️ |

---

## 2. Carga
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Organizar el inventario en rejilla por tamaño | el espacio es limitado | Escape from Tarkov, Resident Evil | N | ✅ |
| Cargar con peso y sufrir sobrecarga | el peso frena e inmoviliza | Project Zomboid | N | ✅ |
| Usar contenedores (mochila, zurrón, arcón) | más capacidad a cambio de equiparlos | Escape from Tarkov | N | ✅ |
| Soltar objetos: caen y ruedan con física y se quedan donde se paran | que el mundo responda como un mundo real | Valheim, Skyrim | N | 🗣️ · soltar ✅ |
| Colocar objetos con precisión en vez de soltarlos (previsualizar, girar) | dejar las cosas donde uno quiere; la misma mecánica sirve para estructuras | Medieval Dynasty, Valheim | N | 🗣️ |
| Que la ropa puesta pese | decidido «el equipo pesa, con coeficiente» | — | D | 🔶 decidido sin aplicar |

## 3. Recolección y entorno
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Recoger del suelo (palos, piedras, bayas, paja, maderitas) | es la materia prima de todo | Minecraft, TerraFirmaCraft | N | 📖 · recoger ✅ |
| Talar árboles (pequeños: ramas largas; grandes: troncos) | hace falta madera de varios tamaños | Valheim, TerraFirmaCraft | N | 📖 |
| Pescar: a lanzazo con la lanza, o mejor con arpón | comida temprana; fácil en ríos con peces remontando, difícil en lagos | Medieval Dynasty, Green Hell | N | 📖 · 🗣️ |
| Encontrar arcilla | desbloquea la cerámica | TerraFirmaCraft | D | 📖 |
| Minería y extracción compleja | metales para la herrería | TerraFirmaCraft, Vintage Story | F | 📖 |

## 4. Transformación (crafteo)
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Craftear desde un menú que reúne todos los modos de crafteo (lista o botón, tallado…) | convertir lo recogido en útiles; cada modo es una pantalla intercambiable | Minecraft | N | 📖 · 🗣️ |
| Tallar piedra en una pantalla interactiva sencilla (cuchillo, hacha…) | la forma la decide el jugador | TerraFirmaCraft (tallado de piedra) | N | 📖 · 🗣️ |
| Tallado más profundo (sustituye la pantalla sencilla) | más oficio, más control | TerraFirmaCraft, Vintage Story | F | 🗣️ |
| Montar herramientas: cabeza + mango (+ atadura) | las herramientas son piezas | TerraFirmaCraft | N | 📖 |
| Hacer una lanza rudimentaria (madera, como mucho punta de piedra) y un arpón de pesca, objetos distintos | el arpón pesca mejor, pero la lanza también sirve | Medieval Dynasty | N | 📖 · 🗣️ |
| Despiezar presas: carne, piel, huesos, tendones | el animal es materia prima, no solo comida | Green Hell | N | 📖 |
| Despiezar y trabajar mejor en una estructura de trabajo que en el suelo | se aprovecha más | — | D | 🗣️ |
| Procesos con tiempo: curtir al sol, secar tendones, ahumar | lo bueno tarda; el proceso es la gracia | — | N | 📖 · 📄 |
| Hacer cuerda con tendones, arco corto, puntas de hueso | la cadena de mejoras | Green Hell | N | 📖 |
| Cerámica con arcilla | recipientes y cocina | TerraFirmaCraft | D (intención: TFG) | 📖 · 🗣️ |
| Herrería | herramientas de metal | Medieval Dynasty, TerraFirmaCraft | F | 📖 |
| Atajos delegables a aldeanos | con colonia, el proceso se reparte | — | F | 📄 |

## 5. Construcción
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Hacer un vivac con ramas largas (contra un árbol cuesta menos) | refugio para dormir; el entorno abarata | Green Hell | N | 📖 |
| Hacer un catre de paja | dormir mejor | Green Hell | N | 📖 |
| Construcción modular (cimientos, paredes, techos) | crucial a futuro; inviable para el TFG con todo lo demás | Valheim, ARK | F | 📄 · 🗣️ |

## 6. Fauna y caza
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Cazar presas que huyen (conejo) | carne y materiales; cuesta | Green Hell, Red Dead Redemption 2 | N | 📖 |
| Enfrentarse a animales que cargan si se asustan (venado, jabalí) | la fauna también hiere | The Long Dark | N | 📖 · 🗣️ |
| Cazar con lanza; luego con arco | mejores armas, mejor caza | Green Hell | N | 📖 |
| Depredadores y territorios | presión del entorno | — | D | 📄 |
| Ganadería y cría | recursos estables | Medieval Dynasty | F | 📄 |

## 7. Tiempo y clima
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Vivir un ciclo de día y noche | de noche se duerme y se ve peor | casi todos | N | 📖 |
| Notar la temperatura del mundo (día y noche; estaciones) | va de la mano de la temperatura corporal | The Long Dark | N (detalle creciente) | 🗣️ |
| Aprovechar el sol para procesos (curtir, secar) | algunos procesos dependen del tiempo | — | D | 📖 |
| Clima con efecto real (lluvia, nieve) y cosechas por estación | el entorno cambia lo que se puede hacer | — | F | 📄 |

## 8. Personas
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Hablar con NPC básicos (diálogo) | información, comercio | — | D | 🗣️ · Fuero en paralelo |
| NPC con supervivencia y estado propio (hambre, miedo) | el mundo vive sin el jugador | — | F | 📄 · Fuero |
| Rasgos de personalidad (p. ej. nocturno: invierte o anula la penalización de la noche) | individualidad; encaja más en NPC que en el jugador | — | F | 🗣️ · Fuero |
| Reputación y memoria de los NPC | lo que haces tiene consecuencias | — | F | 📄 · Fuero |
| Reclutar aldeanos por confianza | base de la colonia | — | F | 📄 |
| Identidad y apodos emergentes | reconocimiento social | — | F | 📄 |

## 9. Economía
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Comerciar | conseguir lo que no puede hacer | Medieval Dynasty | F | 📖 |
| Mercado emergente (órdenes de compra y venta) | precios por oferta y demanda | Albion Online | F | 📄 |

## 10. Conflicto
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Combate cuerpo a cuerpo ligado a las heridas | el daño es una herida concreta, no puntos de vida | Mordhau, Kingdom Come | F | 📄 |
| Facciones con objetivos propios | política emergente | Mount & Blade II: Bannerlord | F | 📄 |

## 11. Colonia
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Asignar tareas a aldeanos y poseer un NPC | el jugador no puede hacerlo todo | Medieval Dynasty, RimWorld | F | 📄 |
| Agricultura | comida estable | Medieval Dynasty | F | 📄 |
| Vista RTS | comandar y gestionar desde arriba | Manor Lords, 0 A.D. | F | 📄 · cámara RTS 🔶 |

## 12. Meta
| Hace | Motivación | Referente | Et. | Origen |
|---|---|---|---|---|
| Empezar en un mundo vacío con mapa fijo | aún no hay pueblos | Minecraft | N | 📖 |
| Guardar y cargar la partida | sesiones de juego reales; sin esto no se puede jugar más de una vez seguida | — | N | 🗣️ |
| Mundo procedural | rejugabilidad | — | F | 📄 |
| Muerte permanente con legado | consecuencias reales | — | F | 📄 |
| Cooperativo de 4 a 8 jugadores y más allá | jugar con otros | — | F | 📄 |

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

## Pendiente de Sergio

Nada por ahora.
