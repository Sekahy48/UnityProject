# Fisiología de referencia — Artisan

Números reales para que los ritmos de la capa 1 de la Fase 2 no se inventen a ojo. Responde a
la «Investigación pendiente» de `MECANICAS.md` (calorías, agua, aguante sin comer ni beber,
temperatura corporal, pérdida de sangre, sueño; más el agua en la sociedad feudal).

**Claude, 2026-11-14.** Ampliado esa misma tarde a petición de Sergio: §5 bis (hambre y
saciedad), aclaraciones en §2, §3, §5 y §8, y verificación de las fórmulas que ya están en el
código. Búsqueda web, sin acceso a artículos de pago. Es una **referencia, no
un diseño**: aquí no se decide ningún ritmo del juego. Lo decidido saldrá de
`Assets/Code/FASE2_HITOS.md`, y este documento solo dice qué dice el mundo real y con qué
fiabilidad.

## Cómo se lee

Cada dato lleva su fiabilidad:

- **A** — organismo oficial o guía clínica (FAO/OMS/UNU, EFSA, Wilderness Medical Society,
  ATLS, ministerio de defensa) o estudio revisado por pares.
- **B** — fuente secundaria razonable (manual médico, divulgación con referencias, revisión
  que cita el estudio original).
- **C** — blog, divulgación sin referencia clara o foro. Sirve de orden de magnitud, **no se
  cita en la memoria del TFG** sin contrastarlo con la fuente primaria.

«**Cálculo propio**» marca cifras que salen de combinar datos de aquí; no son de ninguna fuente
y conviene revisarlas antes de usarlas.

Las URL están al final, numeradas: `[1]`, `[2]`… Las citas de la memoria deberían ir a la
fuente original (por ejemplo, el estudio de Keys y no la divulgación que lo resume); en cada
sección se dice cuál es.

---

## 1. Resumen de cifras para el juego

Cifras redondeadas, para un adulto sano de unos 70 kg. Todas se desarrollan más abajo.

| Qué | Cifra de referencia | Fiab. | Sección |
|---|---|---|---|
| Metabolismo basal (varón 70 kg, 175 cm, 25 años) | ~1 670 kcal/día | A* | 2 |
| Gasto total, vida sedentaria / activa / muy activa | 1,4–1,69 / 1,7–1,99 / 2,0–2,4 × basal | A | 2 |
| Gasto de un jornalero agrícola sin mecanizar | 2,0–2,4 × basal (≈ 3 300–4 000 kcal, **cálculo propio**) | A/B | 2 |
| Agua total recomendada (comida incluida) | 2,5 L varones, 2,0 L mujeres | A | 3 |
| Agua en trabajo duro y calor | 8–12 L/día; tope ~1,25 L/h | A | 3 |
| Mínimo de supervivencia de agua | ~1 L/día | A/B | 3 |
| Aguante sin agua | ~3 días de media; de 1 a 10 según calor y esfuerzo | B/C | 4 |
| Pérdida de agua corporal: molestia → grave → incompatible con la vida | ~2 % → 5–10 % → >10–12 % | B/C | 4 |
| Aguante sin comer, con agua | 30–70 días; sube con la grasa inicial | B | 5 |
| Pérdida de peso tras 24 semanas con ~1 570 kcal/día | ~25 % del peso (≈ 100–120 g/día de media, **cálculo propio**) | A | 5 |
| Hipotermia leve / moderada / grave | 32–35 / 28–32 / <28 °C | A | 6 |
| Golpe de calor | >40 °C con afectación neurológica | A | 6 |
| Volumen de sangre | ~70 mL/kg (≈ 4,9 L a 70 kg) | A/B | 7 |
| Hemorragia clase I / II / III / IV | <15 / 15–30 / 30–40 / >40 % | A | 7 |
| Sueño adulto | 7 h o más (rango 7–9 h) | A | 8 |
| Rendimiento tras horas despierto, comparado con el alcohol | a las 17–19 h, como con 0,05 % de alcohol en sangre; a las 24 h, como con 0,10 % | A (con matices) | 8 |
| Hambre tras una comida normal | vuelve a notarse a las ~3–7 h; depende del hábito | A/B | 5 bis |

\* La fórmula está ya en el código (`EnergyComponent.CalculateBasalMetabolism`, Mifflin-St Jeor) y
**está verificada** (2026-11-14, §2). El resultado de 1 670 kcal es cálculo propio.

---

## 2. Calorías al día según la actividad

**Lo que dice la referencia.** El informe de expertos FAO/OMS/UNU de 2004 [1] expresa el gasto
total diario como un múltiplo del metabolismo basal, el «nivel de actividad física» (PAL):

| Estilo de vida | PAL | Fiab. |
|---|---|---|
| Sedentario o actividad ligera | 1,40–1,69 | A [1][2] |
| Activo o moderadamente activo | 1,70–1,99 | A [1][2] |
| Vigoroso | 2,00–2,40 | A [1][2] |
| Por encima de 2,40 | «difícil de mantener durante mucho tiempo» | A [2] |

La tabla de la UNU [3] pone el trabajo extenuante o el ocio muy activo en 2,0–2,4, y el
artículo de la Wikipedia sobre el PAL [3b], que remite a la FAO, pone ahí al agricultor sin
mecanizar (B). Lo que dice exactamente el informe [1]: el coste de una actividad, medido como múltiplo del
basal, se considera parecido en hombres y mujeres. La excepción son las tareas vigorosas cuyo
esfuerzo exigido es proporcional a la masa muscular y la fuerza (cargar, talar, mazo), que de
media son mayores en varones. **No dice** que quien tiene más músculo rinda más por hora, ni
que la misma tarea le cueste más kcal: es una nota sobre cómo se comparan poblaciones.
(Corregido el 2026-11-14: la primera versión de este párrafo lo resumía peor.)

Razonamiento propio, **sin fuente aquí**: más músculo implica más potencia máxima (la misma
tarea se acaba antes o se hace con menos esfuerzo relativo), pero no necesariamente otro gasto
por tarea. Si el juego quiere que la fuerza afecte a la velocidad de trabajo, es una decisión de
diseño con un atributo propio (pendiente), no algo que la FAO dé hecho.

**Ejemplos del propio informe**, mujer de 55 kg y basal 1 290 kcal: con PAL 1,53, 1 975 kcal;
con PAL 1,76, 2 355 kcal [1].

**Cálculo propio** (basal Mifflin del código, varón 70 kg, 175 cm, 25 años ≈ 1 670 kcal):

| PAL | kcal/día |
|---|---|
| 1,55 (ligero) | ≈ 2 600 |
| 1,78 (moderado) | ≈ 3 000 |
| 2,10 (pesado) | ≈ 3 500 |
| 2,0–2,4 (agrícola) | ≈ 3 350–4 000 |

Mujer de 60 kg, 165 cm, 25 años: basal ≈ 1 345; con PAL 1,5, ≈ 2 000 kcal.

**Masa muscular: cómo estimarla (2026-11-14, noche).** Con peso, talla, edad y sexo solo se
puede estimar **una media poblacional**, no la de una persona concreta.

- El músculo esquelético es de media ~38,4 % del peso en varones y ~30,6 % en mujeres, con una
  banda adulta de 30–40 % (MRI, Janssen 2000; citado de segunda mano) [110].
- Ecuación antropométrica de Lee (2000), validada con MRI y citada de segunda mano por una
  calculadora [110]: músculo (kg) = 0,244·peso + 7,80·talla (m) − 0,098·edad + 6,6·sexo − 3,3,
  con varón = 1. **No pude abrir el artículo original.** Con ella, el varón de referencia de
  70 kg / 170 cm / 25 años da ~31 kg y la mujer de 60 kg / 160 cm ~21 kg (**cálculo propio**),
  frente a los 28 y 17 kg que cita Heymsfield como referencia [111]: sobreestima ~11 % y ~26 %.
- Con peso, talla y edad se explica la mayor parte de la variación (R² 0,85 en varones blancos
  de NHANES, [111]), pero el modelo cambia con la etnia y la medida de referencia importa.
- Con **bioimpedancia** el error es ±2,7 kg (9 %) [112], pero requiere medir resistencia.
- Lo que **no** capta ninguna: el entrenamiento. Dos personas con el mismo peso, talla, edad y
  sexo pueden tener músculo distinto.

Conclusión: de entrada vale una ecuación de este tipo para el **valor inicial** de cada
personaje; si el músculo ha de poder cambiar (entrenar, atrofiarse), deja de poder deducirse y
pasa a ser un dato guardado. Ni `ROADMAP.md` ni `MECANICAS.md` mencionan fuerza, músculo ni
entrenamiento (comprobado con grep el 2026-11-14): sería alcance nuevo.

**Reparto del músculo y entrenamiento (2026-11-15).** Búsqueda hecha a petición de Sergio para
decidir piezas anatómicas, reparto inicial y mecánica de entrenamiento.

*Reparto por regiones* (RM de cuerpo entero, N = 475, Kiel [114]): del músculo total, **brazos
13,0 % (mujeres) y 14,6 % (varones); tronco 36,5 y 37,0 %; piernas 50,5 y 48,4 %**. Ojo: no sé
dónde sitúa ese estudio el límite entre «brazo» y «tronco» (pectoral, dorsal y deltoides pueden
caer a un lado u otro), así que **no se traslada tal cual a los cuatro grupos**. Janssen [113]:
la diferencia entre sexos es mayor en el tren superior (40 %) que en el inferior (33 %), y con la
edad se pierde sobre todo músculo del tren inferior. En 20 jóvenes japoneses el pico de músculo
está a la altura del hombro en varones y del muslo en mujeres [124] (muestra pequeña).

*Reparto dentro de una extremidad.*
- Miembro superior (32 músculos, RM, 10 sujetos, de una mujer del percentil 20 a un varón del 97):
  los que cruzan hombro, codo y muñeca suponen 52,5 %, 31,4 % y 16,0 % del volumen; el deltoides,
  el mayor, 15,2 %. **La distribución es muy conservada**: con el volumen total variando ×3, usar
  la fracción media predice el volumen de cada músculo con un 85 % de la variación explicada [115].
- Miembro inferior (35 músculos, 24 sujetos): el volumen de cada músculo escala con el total
  (R² mediano 0,66) y con la masa (0,52) [117].
- Consecuencia para el diseño: un **reparto inicial por fracciones constantes** de la masa muscular
  total tiene respaldo; las fracciones se pueden sacar de [115] y [117] (**no las he copiado aquí**).

*Fuerza y volumen.* En 10 adultos jóvenes sanos, la fuerza máxima de cada articulación covarió con
el volumen muscular del grupo funcional correspondiente; los autores concluyen que las diferencias
de fuerza entre adultos jóvenes se deben sobre todo a la variación en volumen muscular [116]. Esos
mismos autores agrupan los 32 músculos «según su función principal» (aductores y abductores del
hombro, flexores y extensores del codo, etc.) [116]; es la agrupación funcional con revista que sí
encontré, y solo cubre el miembro superior.

*Adaptación nerviosa.* Es aceptado que los factores nerviosos importan en la ganancia de fuerza
[118]. Su contribución es mayor al principio del entrenamiento, aunque la hipertrofia empieza
desde el inicio [119].

*Adaptación nerviosa: lo que dicen las revisiones (2026-11-15).* Leído: resúmenes de [118], [132]
y [133] y el texto de [128] (hasta ~100 000 de sus 125 000 caracteres).
- **Plazos.** Las primeras semanas de entrenamiento tienen un componente nervioso fuerte y la
  ganancia posterior se atribuye a hipertrofia; la neuroplasticia con entrenamiento crónico
  «apenas se ha explorado de forma sistemática» [132]. **No hay cifras de plazos utilizables**
  en lo que he leído: la curva (cuántas semanas) es calibración.
- **Dónde ocurre.** Mecanismo principal propuesto: corteza motora (activación de la corteza
  ipsilateral); la contribución espinal parece pequeña [128]. El lugar exacto sigue sin
  resolverse [133]. Consecuencia para el modelo: es una adaptación de **control del movimiento**,
  no de un músculo concreto.
- **Transferencia al lado no entrenado (cross-education).** Ganancia de fuerza del lado no
  entrenado de ~7,6 % sobre la inicial, aproximadamente **la mitad** de la ganancia del lado
  entrenado, según una revisión que cita un metaanálisis de 2004 [128]. Otros metaanálisis
  dan 8–12 % [129] y uno reciente con 96 estudios da ~18 % en jóvenes sanos [130] (solo he visto
  el resumen y no sé si ese 18 % es del lado no entrenado). Es mayor con trabajo excéntrico y
  cuando el test usa la misma acción que el entrenamiento [128].
- **El tamaño muscular casi no se transfiere.** La mayoría de estudios halla ganancia de fuerza
  sin ganancia medible de tamaño en el lado no entrenado; en inmovilización, entrenar el otro
  lado frena la atrofia más que construye músculo [128]. Los protocolos multimúsculo no
  hallaron ganancias significativas de fuerza ni de tamaño en el lado no entrenado [128].
- **Asimetría de dominancia en la transferencia:** el metaanálisis específico solo encontró tres
  estudios y cinco tamaños de efecto, con intervalos que incluyen el cero; los autores hablan de
  «apoyo mínimo» a la hipótesis [131]. No prueba que no exista.
- **Límites.** [128] es una revisión crítica de hipótesis, con muestras pequeñas y medidas
  de tamaño por ecografía. Casi todo el trabajo es sobre muñeca y mano; el miembro inferior está
  poco estudiado [128].

*Ganar músculo con déficit de energía (2026-11-15).*
- Metaanálisis de Murphy et al. [125]: en déficit, la ganancia de masa magra es menor que sin
  déficit (ES −0,57 en comparaciones directas); la fuerza no se resiente (ES −0,31, no
  significativo). La metarregresión sitúa en **~500 kcal/día de déficit** el punto en que la
  ganancia de masa magra se anula; no es un corte rígido [125][126].
- Casi todos los estudios con ganancia clara de magra tenían déficit de unas 200–300 kcal/día o
  menos [126]. **Matiz:** la mayoría de participantes eran no entrenados (edad media 51–60) y con
  IMC medio de 32,7 en los estudios con déficit [126]; no es una población de gente delgada y
  entrenada.
- **Quién puede recomponerse:** sobre todo no entrenados con mucha grasa; la ventana se estrecha
  según baja la grasa y sube el nivel de entrenamiento [126]. Los novatos ganan además
  fuerza por adaptación nerviosa [126].
- **Proteína.** El metaanálisis no controló la ingesta de proteína, así que no dice si más
  proteína compensa el déficit [126]. La revisión sistemática en deportistas entrenados y delgados
  con restricción calórica concluye que necesitan **más** proteína (2,3–3,1 g/kg de masa libre
  de grasa, más cuanto más severa la restricción y más delgados) y que los grupos que no perdieron
  magra eran sobre todo los más grasos, los de menor restricción o los de un estímulo nuevo [127].
  Esto es **preservar**, no construir.
- **Respuesta honesta a «¿proteína alta y pocas calorías, quemando la grasa propia?».** Con grasa
  abundante y poco entrenamiento previo, sí hay ganancia posible con déficit moderado; con
  déficit grande o con poca grasa, lo normal es perder magra aunque haya mucha proteína. La
  evidencia que he leído no da una fórmula; es un gradiente, no un interruptor.

*Dominancia (para la asimetría derecha/izquierda).* Metaanálisis de 87 estudios: la extremidad
dominante es de media un 11,6 % más fuerte, con un rango de 2,1 a 19,5 % según articulación y
movimiento [120]. En 310 personas, la «regla del 10 %» se cumplía solo en diestros (12,7 %) y no
en zurdos [121]. Otro estudio dio diferencias de solo 0,1–3 % en diestros [121b]. Conclusión: la
asimetría existe y ronda el 10 %, con mucha variación.

*Sueño y síntesis de proteína muscular.* Una noche de privación aguda de sueño redujo la síntesis
de proteína muscular un 18 % [122]; cinco noches con 4 h de cama también la redujeron [122b]. Eso
respalda que dormir mal frena el crecimiento, **no** que el músculo solo se construya durmiendo:
la mayor parte de la divulgación sitúa la elevación de la síntesis en las 24–48 h posteriores al
ejercicio (**C**), así que gastar el estímulo acumulado «al dormir» sería una simplificación de
juego, no un hecho fisiológico.

*Agrupar por patrón de movimiento.* La literatura de preparación física clasifica los ejercicios
en patrones: sentadilla, bisagra de cadera, zancada, empuje, tirón y transporte [123]; casi
toda es de entrenadores, no de revista (**C**). Los cuatro grupos propuestos (empuje, tirón,
tronco, piernas) son esa agrupación comprimida; no he encontrado una publicación que clasifique
así el cuerpo humano.

**Verificación del código (2026-11-14).**

- `EnergyComponent.CalculateBasalMetabolism` es Mifflin-St Jeor tal como la publican Mifflin et
  al. (1990) [102]: 10·peso (kg) + 6,25·talla (cm) − 5·edad + 5 en varones o − 161 en mujeres;
  en el código `sex == 0` es varón. `PrototypeFactory` pasa la talla como `Height * 100f`
  (`BodyComponent` guarda metros), así que las unidades cuadran.
- Matices de la fuente: se desarrolló con personas de IMC normal y con sobrepeso, y no se
  extiende sin más a delgadez extrema [103]; predice dentro del ±10 % en la mayoría de adultos
  [104]. Un personaje muy flaco por una hambruna queda fuera de su rango de validez: el basal
  que dé la fórmula será una aproximación.
- `BodyComponent.EstimateFatPercentage` es la fórmula de Deurenberg (1991) [105]:
  1,20·IMC + 0,23·edad − 10,8·sexo − 5,4, con varón = 1. En el código aparece ya sustituida
  (−16,2 varón, −5,4 mujer), y coincide. Error típico de ±4,1 puntos y sobreestima algo en
  obesos [105]. Es la fórmula de adultos.

**Cuidado con «cuánto comía un campesino medieval».** Es un asunto en el que los historiadores
no se ponen de acuerdo: las estimaciones de calorías por persona en la Inglaterra medieval y
preindustrial son contradictorias; Muldrew da la más optimista [4]. Circulan cifras de 2 000 y
de 4 500 kcal en divulgación [5][6] sin respaldo claro (**C**). Recomendación: **no usar cifras
históricas para fijar el ritmo**; usar el PAL de la FAO, que es medido, y dejar la historia para
el «sabor».

**Lo que el estudio de Minnesota añade** (§5): el cuerpo se adapta a comer poco (gasta menos),
así que un déficit constante no produce una pérdida constante.

**Lo que esto no resuelve:** cuántas comidas al día y de qué tamaño (la fila de `MECANICAS.md`
pide 3–4); eso no sale de la fisiología, es una decisión de diseño. Tampoco el reparto de
macronutrientes (nutrientes está recortado a «D» en el roadmap).

---

## 3. Agua al día

**Ingesta adecuada.** EFSA (2010) [7]: 2,0 L/día para mujeres y 2,5 L/día para varones,
**contando el agua de todas las bebidas y de la comida**. Los alimentos aportan de ordinario
alrededor del 20 % de la necesidad, hasta el 50 % según lo que se coma [8]. EFSA habla de
actividad y condiciones ambientales moderadas; hay quien necesita más y quien menos.

**Pérdidas.** Evaporación por piel de 0,5 L/día en reposo y frío hasta unos 10 L/día haciendo
ejercicio en calor [9]. En clima caluroso y esfuerzo prolongado se pueden sudar 8–12 L al día
[10]. Máximo registrado puntual: ~3,7 L/h en un maratoniano [9][10].

**Con trabajo duro y calor.** El ministerio de defensa británico (JSP 375) [11] dice que la
necesidad diaria pasa de 2–4 L a 8–12 L en condiciones extremas, que **no conviene pasar de
~1,25 L/hora** de ingesta (se absorbe mal y hay riesgo de hiponatremia) y de 12 L al día. La
tabla horaria depende de temperatura (WBGT) y esfuerzo: de 0,25 L/h en trabajo fácil a 20–25 °C
hasta 1,0 L/h en trabajo muy duro a esa misma temperatura [11].

**Mínimo de supervivencia.** Una referencia del Instituto de Medicina de EE. UU. usa 1 L/día como
mínimo para sobrevivir en condiciones de reposo [12]. (Quité una cita a [13] sobre
entrenamiento de comandos con 1 L/día: la página archivada no se puede leer y no pude
comprobarla.)

**La sed va por detrás de la pérdida de agua** (dato útil para el diseño del agua, explicado el
2026-11-14 a petición de Sergio):

- La sed y el ahorro de agua en el riñón no se disparan hasta que se ha perdido cerca del 2 %
  del peso en agua (≈ 1,4 L a 70 kg) [99][100]. Antes de eso, hay déficit y no hay sed.
- Durante el ejercicio, la sed es menos sensible, y se calma antes de que se haya repuesto todo
  lo perdido [99][100].
- Resultado: quien suda mucho y bebe «hasta quitarse la sed» repone menos de lo que pierde, y
  el déficit se acumula. A eso se le llama **deshidratación voluntaria** [100].
- El resto se repone **con las comidas**: la mayor parte de la ingesta de líquidos ocurre
  comiendo [101].

Para el modelo: la sed del personaje no tiene por qué ser una función lineal del agua que le
falta. Es una señal con umbral (~2 %) y que se apaga antes de tiempo. Eso encaja con un agua
corporal real simulada aparte y una sed derivada.

**Ritmo por actividad.** Las guías dan el agua como ritmo por hora según esfuerzo y temperatura
[11]: es decir, «base × multiplicador de actividad y de calor», que es el modelo de simulación
continua que se estaba planteando (y que además es como funciona el cuerpo).

**Sal (electrolitos).** El sudor lleva menos sodio que el plasma [100]; el reglamento
británico dice que el personal **aclimatado** que come bien a lo largo del día recibe sal
suficiente de la comida (y recomienda sal extra al aclimatarse y bebidas con electrolitos si no
se puede comer) [11], y que beber agua de
más sin sal puede causar hiponatremia [11]. Dos lecturas: (a) la sal va dentro de «comer
normalmente» y no se modela aparte; (b) tiene campo propio (sodio) para poder representar la
hiponatremia. Un único campo `micronutrients` sirve para la primera, **no** para la segunda
(a mi juicio: el sodio se pierde por el sudor y se gana por la comida, las vitaminas no). Es
decisión de diseño.

---

## 4. Aguante sin beber

**Lo que hay.** No existen estudios controlados (no es ético); se trabaja con casos [15][16].
La cifra de «unos 3 días» es una regla de supervivencia popular («regla de los tres»); lo que
dicen los especialistas es que **depende del calor y del esfuerzo**: desde ~1 día en calor
extremo y esfuerzo hasta 10 o más en frío y reposo [15][17]. A 1,5 L de sudor por hora en un día
caluroso la deshidratación grave llega mucho antes de lo que cree la gente [18].

**Por porcentaje de peso perdido en agua** (la unidad que usan los investigadores [15]):

| Pérdida | Qué se nota | Fiab. |
|---|---|---|
| ~2 % | sed; empieza a bajar el rendimiento físico y cognitivo [19] | B |
| 3–5 % | pulso y respiración rápidos, mareo, irritabilidad [20] | C |
| 5 % y más | síntomas «mucho más amplificados»: cansancio, dolor de cabeza, humor peor; en algunos escalofríos y piel de gallina [15] | B |
| 5–10 % | menos sudor y orina, pulso débil, menos alerta [21] | C |
| 6 % o más | confusión, presión baja, desmayo, fallo de órganos, coma o muerte [20] | C |
| >10 % | somnolencia, sudor que se corta, orina mínima, respiración rápida [21] | C |
| 11–12 % | los textos de medicina lo consideran incompatible con la vida [15] | B |

Los tramos de 3–6 % y los de 5–10 % **no coinciden entre fuentes** (las dos son divulgación):
sirven de gradiente cualitativo, no de umbrales exactos. Los dos extremos (≈2 % para el
rendimiento y ≈10–12 % para la muerte) sí se repiten en fuentes mejores.

**El agua del cuerpo.** Alrededor del 60 % del peso en un adulto (la divulgación da 60–70 %) [15][21];
el código ya lo usa en `NutritionComponent.GenerateStoredWater` (55–65 % varones, 45–55 %
mujeres; **esos rangos no los he contrastado**).
**Cálculo propio:** a 70 kg y 60 % son 42 L de agua; el 2 % del *peso* son 1,4 L, el 10 %, 7 L.
Con pérdidas de 3–4 L/día en esfuerzo moderado (§3) y sin beber, grosso modo, el 2 % se alcanza
a media jornada y el 10 %, en un par de días.
Eso concuerda con los «2–3 días» de la regla de los tres, lo cual es una comprobación cruzada,
no una confirmación.

**Lo que esto no resuelve:** el «desmayo progresivo» de `MECANICAS.md` §1.2 no tiene un umbral
de desmayo propio en la referencia; el desmayo por deshidratación aparece en el tramo
severo y mezclado con otras causas (calor, bajada de tensión). El umbral concreto es decisión
de diseño.

---

## 5. Aguante sin comer

**Lo que hay.** Los datos salen de huelgas de hambre y hambrunas. Con agua, un adulto sano
aguanta de **30 a 70 días**; la huelga de hambre irlandesa de 1981 es la referencia: Bobby
Sands murió a los 66 días y otros nueve a los 46–73 [22][23]. Otros casos terminaron sus huelgas
por enfermedad a los 28, 36 y 38 días [24]. Los resultados dependen sobre todo de la grasa de
partida [25][26].

**Fases** (resumen B de [22][27][28]):

| Fase | Cuándo | Qué pasa |
|---|---|---|
| Glucógeno | ~24–48 h | se gasta la reserva rápida |
| Cetosis y grasa | días 2–7 en adelante | pasa a vivir de la grasa; se pierde ~0,9 kg/día la primera semana, bajando a ~0,3 kg/día hacia la tercera, sobre todo por sodio y agua [106] |
| Proteína | tras ~3 semanas en delgados | cuando la grasa baja mucho, empieza a gastarse músculo y órganos; infecciones y arritmias |

**Límite corporal.** Revisiones de hambrunas y huelgas sitúan el índice de masa corporal mínimo
compatible con la vida en torno a 12–13 en varones y algo más bajo en mujeres [22] (**C**: sin
primaria a mano). Se ha usado también BMI ≤16,5 o pérdida del 10 % del peso como punto a partir
del cual hay complicaciones serias: las guías suizas para huelguistas piden valorar hospitalización
con >10 % de pérdida o IMC <16,5–18 [108] (**B**).

**Estudio de Minnesota** (Keys, 1944–45; la referencia es Keys et al., *The Biology of Human
Starvation*, 1950) [30][31][32]:

- 36 varones jóvenes sanos; 12 semanas de control con ~3 200 kcal y trabajo físico [32];
- **24 semanas con ~1 560–1 570 kcal/día** (media; se ajustaba por hombre) [32][33]. **Fuentes en
  conflicto:** el artículo de Kalm y Semba da 1 800 kcal [30]; me quedo con ~1 570 porque lo
  repiten más fuentes, pero hay que contrastarlo con Keys antes de citarlo;
- pérdida media del 25 % del peso [30][33]; unos 17 kg [34];
- apatía, irritabilidad, debilidad, anemia, edema en piernas, déficits neurológicos, interés
  sexual casi nulo, aislamiento social [30][32];
- el peso **dejó de bajar hacia la semana 20**: el cuerpo se adaptó y gastaba menos [35].

**Ritmo medio** (**cálculo propio**): el 25 % de 80 kg son 20 kg en 168 días, unos 120 g al día;
con los 17 kg medios de [34], unos 100 g al día. Es poco porque era **semi-inanición**
(≈ 1 570 kcal frente a las ~3 200 del control, un déficit de ~1 600 kcal), no ayuno total, y
porque el cuerpo se adaptó. Con la regla de 7 700 kcal/kg habría salido más del doble, y los
hombres perdieron menos de la mitad de lo esperado [35] (**C**: divulgación). En ayuno total el
principio es más rápido: ~0,9 kg/día la primera semana y ~0,3 kg/día hacia la tercera, sobre todo
por sodio y agua [106]. (**Corregido el 2026-11-14:** antes decía 1–2 kg/día, cifra de [29] que no
pude comprobar y que resultó exagerada. Una serie de 33 huelguistas medía 420 g/día de media, sin
caída a lo largo de 2 semanas [107].)

Esto último importa para el diseño: **un balance negativo constante no da una pérdida constante**.
Y el efecto psicológico (apatía, irritabilidad, retirada social) es el referente real de lo que
`MECANICAS.md` §1.7 llama ánimo.

**Energía de la grasa.** La regla clásica de 7 700 kcal por kg de peso perdido (3 500 por libra)
[36] **sobreestima** el déficit necesario en personas delgadas, porque en ellas se pierde más
masa magra, que tiene mucha menos energía por kg [36][37]. Hall (2008) lo formaliza [37].
Ojo con a qué se aplica: 7 700 kcal/kg es el valor del *tejido adiposo* (lleva agua); la grasa pura
da ~9,3–9,4 kcal/g, es decir ~9 300 kcal/kg [36][38]. Ninguna de las dos sirve como equivalencia
de «peso total perdido».

**Cálculo propio** de comprobación cruzada: varón de 70 kg con 15 % de grasa = 10,5 kg de grasa
≈ 81 000 kcal contando 7 700 por kg, o ≈ 98 000 con 9 300; con un gasto de ~1 800 kcal/día sin
comer (reducido por adaptación) son ~45–54 días. Cae dentro de los 30–70 de las fuentes. Una mujer con más grasa porcentual aguanta algo
más [24].

**Lo que esto no resuelve:** el ritmo del *hambre* (la sensación) no es el de la inanición: se
siente a las pocas horas; el roadmap pide un hambre que sacia «según el tamaño de la comida».
Eso es diseño. Tampoco he mirado el síndrome de realimentación (riesgo real al volver a comer
tras semanas) porque no entra en el alcance, pero ya aparece citado en [39].

---

## 5 bis. Hambre y saciedad (la sensación, no la inanición)

Añadida el 2026-11-14 a petición de Sergio, para decidir **cuándo vuelve el hambre tras una
comida** y **cómo se comporta en ayunos largos**. **Es la sección con evidencia más desigual de
este documento:** mezcla estudios de laboratorio con divulgación.

**Dos conceptos distintos** (vocabulario de la literatura): *saciación* es lo que termina una
comida y decide su tamaño; *saciedad* es lo que impide volver a tener hambre después, y se mide
por la duración del intervalo hasta la siguiente comida (definiciones de revisiones de
nutrición; ver [89] y [90] para estudios que usan el intervalo como medida). Lo que importa aquí es la saciedad.

**Cuánto dura la saciedad tras una comida.**

- Los adultos se adaptan a su costumbre: hay quien tiene un patrón de 2,5–3,5 h entre
  desayuno y comida y quien lo tiene de 5,5–6,5 h [88].
- Sin pistas de hora ni de comida, sujetos que pidieron su cena cuando quisieron, tras una
  comida programada, lo hicieron tras intervalos de 320–425 min (5,3–7,1 h), y su hambre
  subjetiva seguía el mismo perfil que la grelina [87].
- Vaciado gástrico: en adultos de peso normal, la mitad de una comida de ~500 g y 145 kcal/100 g
  (**cálculo propio:** ~725 kcal) salía del estómago en unos 148 min [89].
- Composición: en un estudio, la grasa dio más saciedad (más intervalo) que el carbohidrato
  simple [90].

**Hora del día y hábito.** La grelina (la hormona del hambre) casi se duplica justo antes de
cada comida y cae a su mínimo en menos de una hora tras comer, en 10 sujetos con comidas a
horas fijas [86]. Sus picos se ajustan al horario habitual: el cuerpo se anticipa a la comida
[88]. Es la base real de «el hambre entra cuando tocaría comer».

**En ayunos largos.**

- Las oleadas de grelina siguen el horario habitual de comidas durante los primeros días de ayuno
  y se hacen más pequeñas cada día [94][95] (**C**: divulgación; no he leído los estudios que
  citan).
- Con una dieta cetogénica muy baja en calorías (**no es ayuno total**), el aumento de hambre
  por adelgazar aparece al principio (día 3, ~5 % de pérdida) y, ya en cetosis, una pérdida del
  10–17 % no se asoció a más apetito [91]. La cetosis atenúa el aumento de grelina tras perder
  peso [92], y unas bebidas de cetonas bajaron grelina y hambre percibida [93].
- El hambre vuelve a aumentar con la realimentación [91].

Lectura honesta: **hay apoyo para que el hambre no crezca sin límite y se embote tras unos
días**, y para que sea oleadas ligadas al reloj de las comidas. Pero la fuente directa son dietas
muy bajas en calorías y divulgación sobre ayunos, **no hambrunas reales**; y el estudio de
Minnesota (§5) no me da nada que lo confirme ni lo niegue. La intensidad y el momento varían
entre personas.

**Historia (para «un horario normal de comidas»).** En la Europa medieval tardía lo habitual era
comer **dos veces**: «dinner» hacia las 10:30–11 y «supper» unas cinco horas después, con el
desayuno a menudo omitido [96]; hay textos de la época que ya decían que un jornalero podía comer
tres veces [96], y a mediodía los jornaleros tomaban algo ligero de pan y queso [97]; la cena era
entre las 17 y las 20 h [97]. (**C**: divulgación histórica; hay discrepancia sobre si eran dos o
tres/cuatro [98].) Un intervalo de ~5 h entre las dos comidas encaja en los intervalos de arriba.

**Lo que esto NO dice.** Ninguna fuente fija «muy hambriento a las X horas»: los estudios miden
cuándo la gente **pide** la comida, no etapas de hambre. Los tramos (hambriento, muy hambriento,
extremo) y su duración son **diseño**. Lo que la referencia sí da es el orden de magnitud del
primer tramo (≈ 3–7 h después de una comida normal, según hábito y composición) y la **forma**:
oleadas ligadas al horario habitual, y embotamiento si el ayuno se alarga.

**Implicación para el modelo, sin decidir:** la sensación de hambre (saciedad a corto plazo) y
las reservas de energía (largo plazo) son cosas distintas. El hambre depende del estómago y del
hábito; las reservas, de la grasa. El `NutritionComponent` ya trae `hunger` y `storedKcal`, o
sea, las dos capas.

---

## 6. Temperatura corporal

**Hipotermia** (Wilderness Medical Society, vía manual MSD) [40][41]:

| Grado | Temperatura central | Qué pasa |
|---|---|---|
| Leve | 32–35 °C | tiembla, taquicardia, pierde coordinación, apatía, juicio alterado [42][43] |
| Moderada | 28–32 °C | deja de temblar, somnolencia, pulso y respiración lentos, arritmias [42][43] |
| Grave | <28 °C | inconsciencia, riesgo de fibrilación; «no está muerto hasta que está caliente y muerto» [43][44] |

La clasificación suiza usa estadios por síntomas: I consciente y temblando (32–35), II confuso
y sin temblar (28–32), III inconsciente (24–28), IV sin signos vitales (<24) [43][45]. El temblor
deja de ser eficaz hacia los 30–32 °C: las fuentes **no coinciden en el punto exacto** (30 en
[45], 32 en [44]) y es un gradiente, no un interruptor.

**Desnudamiento paradójico** (quitarse la ropa en fases avanzadas por confusión) [42].

**Calor.**

- Agotamiento por calor: 38–40 °C con la cabeza intacta [44].
- Golpe de calor: >40 °C con alteración neurológica (delirio, convulsiones, coma) [46][47].
  El golpe de calor por esfuerzo **puede ocurrir sudando** [46].

**Tiempo hasta el daño.** No hay datos controlados de hipotermia profunda [48]; Tikuisis (1995)
da, para un varón sano desnudo en aire en calma, supervivencia (hasta 30 °C central) de unas
1,8 h a −30 °C, 4,1 h a −10 °C, 9 h a 0 °C y más de 24 h a 10 °C; es un **modelo extrapolado**,
no un dato medido [48]. En agua fría, la regla de Giesbrecht «1–10–1»: 1 minuto de choque por frío,
10 minutos de fuerza útil, 1 hora antes de hipotermia grave [49][50] (agua helada; no aplica a
aire).

**Factores:** la ropa mojada y el viento aumentan el riesgo; hay hipotermia incluso en climas
cálidos con inmovilidad o agua a 20–24 °C durante mucho tiempo [41]. Cansancio, hambre y
deshidratación empeoran el cuadro [50][51]: **la hipotermia se alimenta de las otras
necesidades**, lo cual encaja con un sistema de estados que se influyen entre sí.

**Lo que esto no resuelve:** cuánto aísla cada prenda. El catálogo ya tiene «aislamiento de la
ropa en los datos» (`MECANICAS.md` §1.8, 🔶); la referencia da la fisiología del cuerpo, no los
valores de aislamiento de una prenda medieval. Faltaría una fuente de «clo» (unidad de
aislamiento de la ropa) si se quiere realismo ahí. No la he buscado.

---

## 7. Pérdida de sangre

**Volumen.** Adulto: ~70 mL/kg, ~7 % del peso (≈ 4,9 L a 70 kg); varones 70–75, mujeres ~65 mL/kg
[52][53][54]. La donación ordinaria (450 mL ± 10 %) se diseña para no pasar del 15 % del volumen
[55][56].

**Clasificación ATLS** (el estándar de trauma) [57][58]:

| Clase | Pérdida | A 70 kg | Pulso | Tensión | Estado mental |
|---|---|---|---|---|---|
| I | <15 % | <750 mL | <100 | normal | ligera ansiedad |
| II | 15–30 % | 750–1 500 mL | 100–120 | normal | ansiedad moderada |
| III | 30–40 % | 1 500–2 000 mL | 120–140 | baja | ansioso, confuso |
| IV | >40 % | >2 000 mL | >140 | muy baja | confuso, letárgico |

Detalles que importan:

- Son una **clasificación didáctica**: la frecuencia cardiaca real no sube de forma lineal (a
  veces se estabiliza o incluso baja hacia el 30 % por un reflejo vagal) y en la clase IV puede
  haber bradicardia justo antes del paro [58][59]. La guía europea de hemorragia en trauma
  reproduce la misma clasificación [60].
- **La clase IV (>40 %) es la que amenaza la vida de forma inmediata.** Es el umbral más
  claro de «muerte por desangrado»; para el desmayo no hay un umbral único.
- **Desmayo.** En donantes (10–15 % del volumen, bajo control) se desmayan unos 30 de cada
  10 000 (2–40 de 10 000 según población), y la mayoría de las veces es una reacción vasovagal
  (miedo, de pie de golpe) y no solo volumen [61][62]. Con pérdida rápida, la zona de
  semiconsciencia/confusión coincide con las clases III–IV (30–40 %) [57][58]. **Es
  coherente fijar el desmayo en torno al 30–40 %, pero es una decisión de diseño apoyada en la
  clínica, no un número de la literatura.**
- **Tiempo hasta morir desangrado:** **no lo he encontrado en una fuente fiable.** Depende del
  vaso (arteria o vena), de la zona y de la herida; no hay una cifra de «minutos» aceptable
  para citar. Hueco en la investigación.

**Lo que esto no resuelve:** el ritmo de sangrado de cada tipo de herida (corte, perforación,
desgarro), la coagulación, qué para una hemorragia (presión, venda, torniquete) y la
recuperación del volumen. La donación da un dato: el plasma se repone en 24–48 h y los glóbulos
rojos en 4–6 semanas [63]. El resto, la mecánica de heridas, lo decidirá el diseño; la clínica
de heridas (infección, fracturas) **no está en el alcance de esta investigación**.

---

## 8. Sueño

**Recomendación.** Consenso AASM/SRS (2015): los adultos deberían dormir **7 h o más** de forma
regular [64][65]; el rango habitual citado es 7–9 h (18–64 años) [66].

**Efecto de la vigilia.** En cristiano: se midió el rendimiento en pruebas de reflejos y atención
a gente que llevaba muchas horas despierta, y se comparó con el de gente que había bebido
cantidades conocidas de alcohol. Sirve como **escala del deterioro**: tras 17–19 h despierto
estás como alguien con 0,05 % de alcohol (en parte de Europa, el límite legal para conducir) y a
las 24 h, como con 0,10 %. Williamson y Feyer (2000) [67][68]:

- tras **17–19 h despierto**, el rendimiento en algunas pruebas era igual o peor que con 0,05 %
  de alcohol en sangre;
- tras **24 h**, equivalente a 0,10 % [65][69] (el trabajo previo de Dawson y Reid, 1997, iba en
  la misma línea [71]).

**Matiz que afecta al diseño:** esos 17–19 h caen en la hora circadiana más baja (22:30–01:00),
así que el experimento **no separa las horas despierto del reloj** [68]. Eso apoya la idea de
`MECANICAS.md` §1.3 («el cansancio baja más rápido cuanto más oscurece») y es un dato a favor,
no en contra.

**Restricción crónica.** Van Dongen et al. (2003): 6 h de cama durante 14 noches dejan déficits
de atención comparables a una noche entera en vela; 4 h, a dos noches [65][71]. Se subestima el
propio deterioro [71]. La recuperación de unas pocas noches cortas se logra con 2–3 noches
buenas [72].

**Vigilia total.** Microsueños (lapsos de segundos) desde las ~36–48 h; alucinaciones y
paranoia hacia las 48–72 h [73][74] (**B/C**: divulgación; la primaria sería una revisión de
medicina del sueño). El récord documentado es de 264 h (11 días, Randy Gardner, 1964), sin
secuelas permanentes conocidas [75][76].

**¿Se muere por no dormir?** En ratas sí: murieron en 11–32 días en los experimentos de
Rechtschaffen (la causa no está clara) [75][77][78]. **En humanos no hay ninguna muerte
documentada por privación de sueño sola** [78][79]; lo que mata es la *consecuencia* (accidentes,
colapso, la enfermedad rara «insomnio familiar fatal», que mata por fallo multiorgánico) [75].
Esto es relevante porque `MECANICAS.md` y el roadmap dicen que descuidar el cuerpo lleva «al
desmayo y a la muerte»: **para el sueño no hay base real de muerte directa, sí de desmayo
(microsueños)**. Qué hacer con eso es decisión de diseño.

**Lo que esto no resuelve:** cuánto dura un ciclo de sueño y cuánto cura dormir (la mecánica
«curar más rápido durmiendo» se apoya en que la restauración existe, pero no he buscado
tasas de cicatrización).

---

## 9. El agua en la sociedad feudal

(Pendiente de `MECANICAS.md`: qué fuentes se consideraban seguras, si bastaba hervir, qué era
normal, y si el agua mala debería afectar también al ánimo.)

**Lo que se sostiene.** El tópico de «bebían cerveza porque el agua era peligrosa» es un mito
discutido por historiadores de la alimentación [80][81][82]. La gente bebía **agua de pozo,
manantial, conductos y lluvia**; se dejaban escritos sobre cómo distinguir la buena de la mala;
la cerveza se bebía mucho, pero por ser **alimento y calorías**, no por huir del agua [80][83].

**Criterios de calidad.** Agua **clara, fría e inodora** se consideraba buena; los pozos se
forraban con madera para que no se enturbiaran [81][84]. Hay escritos que aconsejan hervir la
que olía mal, y Hildegard de Bingen recomienda hervir y enfriar el agua de río y de pantano, y
desaconseja la de nieve [84][85]. **Hervir no es una invención moderna** pero tampoco aparece
como norma general [80][84].

**Lo que no se sostiene.**

- Que no existiera agua potable: no hay fuente medieval que lo diga [80].
- Que la fermentación «desinfecte» y por eso se bebiera cerveza: es una explicación moderna
  [80]; además la cerveza floja de diario tiene poco alcohol.

**Fiabilidad.** **B/C.** Las fuentes son un verificador de datos, un blog de historia de la
comida y un hilo de historiadores en un foro; apuntan a la bibliografía correcta (**Chevallier;
Squatriti, *Water and Society in Early Medieval Italy*; Ælfric, *Colloquy*; Fitzstephen**) pero
yo **no he leído esos libros**. Antes de citarlo en la memoria hay que ir a ellos.

**Preguntas abiertas que la historia no resuelve:**

- Si hace falta **filtrar** además de hervir: no he encontrado nada fiable sobre filtrado en el
  medievo; el sabor terroso lo menciona `MECANICAS.md` como hipótesis, y queda sin respaldo.
- Si el agua de mala calidad debe **afectar al ánimo**: no hay dato histórico; es diseño.

---

## 10. Huecos de esta investigación

Lo que **no** he conseguido o no estaba en el encargo, para que no se dé por cubierto:

1. Tiempo hasta morir por desangrado (§7).
2. Valores de aislamiento de la ropa («clo») y pérdida de calor por viento y humedad
   cuantificada (§6).
3. Sal y electrolitos (§3).
4. Ritmos de cicatrización, riesgo y evolución de infecciones y fracturas (capa 2: fuera del
   alcance de esta primera investigación).
5. Fuente primaria de las alucinaciones y microsueños (§8) y de los umbrales de deshidratación
   intermedios (§4).
6. Frecuencia y tamaño de comidas en la época (§2): no resuelto por la historia.
7. Hambre y saciedad (§5 bis): ningún estudio fija etapas de hambre por horas; las fuentes sobre
   el embotamiento en ayuno son dietas muy bajas en calorías y divulgación.
8. Sodio como campo propio y su efecto (hiponatremia): sin investigar más allá de lo citado.

## Fuentes

Números `[n]` a los que remiten las secciones. «B/C» según la escala del principio.

**Calorías**
1. FAO/WHO/UNU, *Human energy requirements* (2004), cap. 5 — https://www.fao.org/3/y5686e/y5686e07.htm — A
2. FAO, resumen de hallazgos del comité de expertos (tabla PAL) — https://www.fao.org/4/Y4249e/y4249e0j.htm — A
3. UNU, *Energy expenditure of free-living adults* (PAL por actividad) — https://archive.unu.edu/unupress/food2/UID01E/UID01E08.HTM — A/B
3b. Wikipedia, *Physical activity level* (agricultor sin mecanizar 2,0–2,4; remite a la FAO) — https://en.wikipedia.org/wiki/Physical_activity_level — B
4. Muldrew, *Calories consumed by labourers* (Cambridge UP, 2011) — https://resolve.cambridge.org/core/books/abs/food-energy-and-the-creation-of-industriousness/calories-consumed-by-labourers/2AFF7BA541270C6ED610A3AFDA325F71 — A (solo resumen); estimaciones en conflicto: Kelly y Ó Gráda, UCD WP12/12 — https://www.ucd.ie/economics/t4media/WP12_12.pdf — A
5. encyclopedia.com, «Farmers and Peasants» — https://www.encyclopedia.com/history/news-wires-white-papers-and-books/farmers-and-peasants-food-and-harvest — C
6. DeLong (substack), estimaciones de nivel de vida agrario — https://braddelong.substack.com/p/1-guesstimating-typical-living-standards — C

**Agua**
7. EFSA, valores de referencia de agua (2010) — https://www.efsa.europa.eu/en/press/news/nda100326 — A
8. CIEAH (Univ. Las Palmas), preguntas sobre hidratación — https://cieah.ulpgc.es/?p=128 — B
9. Gisolfi, *Water requirements during exercise in the heat* (NAP, 1993) — https://www.ncbi.nlm.nih.gov/books/NBK236237/ — A
10. DTIC ADA329907, balance de agua y sudor — https://apps.dtic.mil/sti/pdfs/ADA329907.pdf — A
11. UK MoD, JSP 375 vol. 1 cap. 41, anexo F (hidratación) — https://assets.publishing.service.gov.uk/media/66606e4a0c8f88e868d3361c/Annex_F_-_hydration_guidance.pdf — A
12. NAP, *Predictions of daily water and sodium requirements* — https://books.nap.edu/read/10925/chapter/14 — A
13. USARIEM, guía de hidratación — https://webarchive.library.unt.edu/web/20130214204845/http:/www.usariem.army.mil/Pages/download/HydrationPDF.pdf — **no comprobada** (solo se ve el visor del archivo web, sin el texto); ya no se cita en el cuerpo
14. NLM, requisitos de agua del ejército (histórico, estudios «Hot Room») — https://collections.nlm.nih.gov/ocr/nlm:nlmuid-101676662-bk — A

**Sin beber**
15. Discover, entrevista a Kavouras (ASU) — https://www.discovermagazine.com/health/how-dehydration-can-kill-you-in-a-matter-of-days — B
16. Business Insider (sin estudios controlados) — https://businessinsider.nl/how-many-days-a-person-can-survive-without-food-or-water/ — C
17. Live Science, supervivencia sin agua — https://www.livescience.com/32320-how-long-can-a-person-survive-without-water.html — B
18. Live Science (misma, sudor de 1,5 L/h) — https://www.livescience.com/amp/32320-how-long-can-a-person-survive-without-water.html — B
19. Hydratis, citando a Adan (2012) — https://es.hydratis.co/en/blogs/blog/combien-de-jours-sans-boire-peut-on-survivre-duree-dangers-et-signes-vitaux — C
20. WaterDropFilter (etapas de deshidratación) — https://www.waterdropfilter.ca/blogs/home-and-wellness/how-long-can-you-live-without-water — C
21. OffGridWeb (5/10 %) — https://www.offgridweb.com/preparation/managing-dehydration-a-thirst-for-knowledge/amp/ — C

**Sin comer**
22. Science ABC (IMC y huelga irlandesa) — https://www.scienceabc.com/humans/what-would-happen-if-you-stopped-eating-altogether.html — C
23. People's Pharmacy (huelga de 1981) — https://library.peoplespharmacy.com/long-human-live-eating — C
24. Business Insider (huelgas de 28, 36 y 38 días; mujeres) — https://businessinsider.nl/how-many-days-a-person-can-survive-without-food-or-water/ — C
25. Acute Center, «What happens when you stop eating» — https://www.acute.org/resources/when-you-stop-eating — B
26. Ubie Health, citando a Cahill y Owen (1968), *Starvation and survival* — https://ubiehealth.com/doctors-note/how-long-can-you-go-without-food-survival-times-5793q6 — B (la primaria es Cahill)
27. Aeviva 136 (fases) — https://aeviva-news.beehiiv.com/p/aeviva-136-starving — C
28. Ubie Health (fases) — misma que [26]
29. Healthline — https://www.healthline.com/health/food-nutrition/how-long-can-you-live-without-food — **no comprobada** (la sesión no puede abrirla); sustituida por [106]–[108]
30. Kalm y Semba, *They starved so that others be better fed*, J Nutr 135:1347 (2005) — https://doi.org/10.1093/jn/135.6.1347 — A (consultado en una copia de Scribd, no en la revista)
31. APA Monitor, «Hunger» (2013) — https://www.apa.org/monitor/2013/10/hunger — B
32. Wikipedia, Minnesota Starvation Experiment — https://en.wikipedia.org/wiki/Minnesota_Starvation_Experiment — B; primaria: Keys et al., *The Biology of Human Starvation*, 1950
33. Medium/NU Sci y MedBound Times (1 560–1 570 kcal) — https://medium.com/nusci/the-minnesota-starvation-experiment-how-we-know-what-we-know-about-diet-688d792de0c7 — C
34. Hall, *Body fat and fat-free mass interrelationships* (2007) — https://pmc.ncbi.nlm.nih.gov/articles/PMC2376748 — A
35. Zoe Harcombe, resumen del estudio — https://www.zoeharcombe.com/?p=397 — C (la meseta en la semana 20 hay que contrastarla con Keys)
36. PMC3810417, *Energy content of weight loss* — https://pmc.ncbi.nlm.nih.gov/articles/PMC3810417 — A
37. Hall, *What is the required energy deficit per unit weight loss?* Int J Obes 32:573 (2008) — https://pubmed.ncbi.nlm.nih.gov/17848938/ — A
38. Hall et al., PMC2376751 (densidades energéticas) — https://pmc.ncbi.nlm.nih.gov/articles/PMC2376751 — A
39. People's Pharmacy (realimentación) — misma que [23]

**Temperatura**
40. MSD Manual, *Hypothermia* (WMS) — https://msdmanuals.com/en-gb/professional/injuries-poisoning/cold-injury/hypothermia — A
41. Mismo (hipotermia en clima cálido) — A
42. Pitt, resumen AAFP — https://sites.pitt.edu/~nancyp/uhc-1510/HypothermiaArticle.pdf — B
43. Better Health Victoria — https://www.betterhealth.vic.gov.au/health/HealthyLiving/hypothermia — A
44. GetOnCourse (termorregulación) — https://getoncourse.ai/lessons/indian-medical-pg/ini-cet/physiology/thermoregulation — C
45. FPNotebook (escala suiza) — https://mobile.fpnotebook.com/ER/Environ/AcdntlHypthrm.htm — B
46. Open Exam Prep (ambiente térmico) — https://open-exam-prep.com/study-guides/ccp-c/toxic-exposure-and-environmental-patient/environmental-thermal — C
47. Iatrox — https://www.iatrox.com/academy/usmle/hypothermia-hyperthermia-heat-stroke — C
48. Tikuisis, *Predicting survival time for cold exposure*, Int J Biometeorol 39:94 (1995) — https://pubmed.ncbi.nlm.nih.gov/8530210/ — A
49. Adventure Medical Kits (1-10-1, Giesbrecht) — https://adventuremedicalkits.com/blogs/news/cold-water-immersion-survival — B
50. NOLS, *Falling through the ice* — https://www.nols.edu/?p=742179 — B
51. Borden Institute, *Human physiological responses to cold stress and hypothermia* — https://medcoeckapwstorprd01.blob.core.usgovcloudapi.net/pfw-images/borden/harshenv1/Ch11-HumanPhysiologicalResponsestoColdStressandHypot.pdf — A

**Sangre**
52. FPNotebook, clasificación de hemorragia — https://mobile.fpnotebook.com/ER/Exam/HmrhgClsfctn.htm — B
53. PMC4164065, síncope en donación — https://pmc.ncbi.nlm.nih.gov/articles/PMC4164065 — A
54. Acibadem, volumen de sangre — https://acibademinternational.com/blog/how-much-blood-is-in-the-human-body-liters-pints-and-donations/ — C
55. UK Transfusion Services (JPAC), peso y volumen del donante — https://www.transfusionguidelines.org/document-library/documents/donor-weight-and-donation-volumes/download-file/dl_support_jpac-cn13.pdf — A
56. UK Transfusion Services, altura y peso (Nadler) — https://www.transfusionguidelines.org/document-library/documents/recommendations-on-donor-height-and-weight/download-file/dl_support_jpac_11-10.pdf — A
57. PMC10241213, tabla 1, clasificación ATLS — https://pmc.ncbi.nlm.nih.gov/articles/PMC10241213/table/t0001 — A
58. Relias, *Traumatic hemorrhagic shock* — https://www.reliasmedia.com/articles/traumatic-hemorrhagic-shock — B
59. Clinician.com (bradicardia en clases III–IV) — https://www.clinician.com/articles/traumatic-hemorrhagic-shock — B
60. Rossaint et al., *European guideline on management of major bleeding and coagulopathy following trauma*, 6.ª ed., Crit Care 2023 — https://doi.org/10.1186/s13054-023-04327-7 — A
61. Notify Library, síncope del donante — https://www.notifylibrary.org/incident/1657 — A
62. Vox Sang 2011;101:303 (desmayos en donación) — https://api-pubmed-ncbi.laibokeji.com/21535440 — A
63. Acibadem (reposición de plasma 24–48 h; hematíes 4–6 semanas) — misma que [54] — C

**Sueño**
64. American Bar Association, resumen AASM/SRS 2015 — https://www.americanbar.org/groups/young_lawyers/resources/tyl/personal-financial/sleep-and-your-health/ — B
65. Open Exam Prep (consenso AASM/SRS) — https://open-exam-prep.com/study-guides/brpt-ccsh/abnormal-sleep-overview/sleep-deprivation-effects — C
66. Sleep Pulse / Canadian Sleep Society, por edad — https://sleep-pulse.netlify.app/deficits — C
67. Williamson y Feyer, *Occup Environ Med* 57:649 (2000) — doi:10.1136/oem.57.10.649 — A
68. Soon.works, análisis de Williamson y Feyer — https://soon.works/research/sleep-deprivation-compared-with-alcohol-impairment — B
69. Harvard Medical School, División de Medicina del Sueño — https://sleep.hms.harvard.edu/education-training/public-education/sleep-and-health-education-program/sleep-health-education-89 — A
70. Van Dongen et al., *Sleep* 26:117 (2003) — doi:10.1093/sleep/26.2.117 — A (el resumen lo da [65])
71. Loku (resumen de Van Dongen) — https://loku-blog-v1.vercel.app/category/wellbeing/sleep-deprivation-cognitive-cost — C
72. Mattress Miracle (recuperación) — https://mattressmiracle.ca/blogs/mattress-miracle-blog/sleep-deprivation-guide — C
73. Acibadem, vigilia hora a hora — https://acibademinternational.com/blog/how-long-can-you-go-without-sleep-deprivation-hour-by-hour/ — C
74. Mattress Miracle (48–72 h) — https://mattressmiracle.ca/blogs/mattress-miracle-blog/how-long-without-sleep — C
75. Scientific American, *How long can humans stay awake?* — https://www.scientificamerican.com/article/how-long-can-humans-stay/ — B
76. Gizmodo, *Can you die from sleep deprivation?* — https://gizmodo.com/can-you-die-from-sleep-deprivation-1684235719 — B
77. Rechtschaffen et al., 1989 (ratas), citado en [76]
78. Slate, *Can you die from lack of sleep?* — https://www.slate.com/articles/news_and_politics/explainer/2009/05/can_you_die_from_lack_of_sleep.html — B
79. Lifehacker AU — https://www.lifehacker.com.au/?p=719754 — C

**Agua en el medievo**
80. Ancient Origins, verificación del tópico — https://www.ancient-origins.net/news-history-archaeology/medieval-people-drink-ale-because-water-unsafe-00128331 — C
81. Tastes of History, *Dirty water? Drink beer!* — https://www.tastesofhistory.co.uk/post/dispelling-some-myths-dirty-water-drink-beer — C
82. Gizmodo, resumen del artículo de Chevallier — https://gizmodo.com/no-medieval-people-didnt-drink-booze-to-avoid-dirty-w-1533442326 — C
83. HistoryFacts — https://historyfacts.com/?p=5919 — C
84. r/AskHistorians, 2013 y 2021 (hilos de historiadores) — https://redlib.hackliberty.org/r/AskHistorians/comments/1svj1q/how_did_people_esp_european_townsmen_get_fresh — C
85. Mismo hilo (Hildegard de Bingen) — C

**Ampliación del 2026-11-14**
86. Cummings et al., *A preprandial rise in plasma ghrelin levels suggests a role in meal initiation in humans*, Diabetes 50:1714 (2001) — https://pubmed.ncbi.nlm.nih.gov/11473029/ — A
87. Cummings et al., *Plasma ghrelin levels and hunger scores in humans initiating meals voluntarily without time- and food-related cues*, Am J Physiol Endocrinol Metab 287:E297 (2004) — A (solo resumen)
88. *Possible entrainment of ghrelin to habitual meal patterns in humans*, Am J Physiol GI, doi:10.1152/ajpgi.00448.2007 — https://peeref.com/works/6622755 — A (solo resumen)
89. Gastric emptying y saciedad en peso normal y obesidad (Univ. Nottingham, resonancia) — https://nottingham-repository.worktribe.com/index.php/output/51341526/ — A
90. Estudio de intervalo entre comidas y composición de la precarga (Maastricht) — https://cris.maastrichtuniversity.nl/ws/files/1436292/guid-2468e3ae-346a-426d-beae-ba9163779135-ASSET1.0.pdf — A
91. Nymo et al., *Timeline of changes in appetite during weight loss with a ketogenic diet*, Int J Obes 41:1224 (2017) — https://pmc.ncbi.nlm.nih.gov/articles/PMC5550564 — A
92. Sumithran et al., *Ketosis and appetite-mediating nutrients and hormones after weight loss*, Eur J Clin Nutr 67:759 (2013) — https://www.scribd.com/document/462293612/Cetose-e-apetite (copia de Scribd) — A
93. Estudio de cetonas exógenas y apetito — https://pmc.ncbi.nlm.nih.gov/articles/PMC5813183/ — A
94. Happy Herbivore, podcast sobre ayuno y grelina — https://happyherbivore.com/podcast/fasting-vs-caloric-restriction-weight-loss-ghrelin — C
95. Diet Doctor, ayuno y grelina — https://www.dietdoctor.com/not-get-hungry-fasting-ghrelin — C
96. History Extra, «How the Tudors invented breakfast» (copia en Goodreads) — https://www.goodreads.com/author_blog_posts/8376083-how-the-tudors-invented-breakfast — C
97. Tastes of History, *Daily bread: meals in Tudor England* — https://www.tastesofhistory.co.uk/post/daily-bread-meals-in-tudor-england — C
98. Metafilter Ask, «The Most Important Meal of the Day» (debate sobre cuántas comidas) — https://ask.metafilter.com/376395/The-Most-Important-Meal-of-the-Day — C
99. Kenefick, *Drinking strategies: planned drinking versus drinking to thirst*, Sports Med 48 (2018) — https://pmc.ncbi.nlm.nih.gov/articles/PMC5790864 — A
100. Gatorade Sports Science Institute, SSE 182 (estrategias de ingesta de líquido) — https://www.gssiweb.org/en/sports-science-exchange/article/fluid-intake-strategies-for-optimal-hydration-and-performance-planned-drinking-vs.-drinking-to-thirst — B
101. Kenefick, presentación sobre el debate de hidratación (cita a Eichna 1945: la mayor parte de la ingesta ocurre con las comidas) — https://cdn.zephyrcms.com/8fc9e29b-1630-44f4-ab5e-cdabe5d487a5/-/inline/yes/0019-kenefick-hydration-debate.pdf — C
102. Mifflin et al., *A new predictive equation for resting energy expenditure in healthy individuals*, Am J Clin Nutr 51:241 (1990), doi:10.1093/ajcn/51.2.241 — comprobada la fórmula en https://msdmanuals.com/medical-calculators/MifflinStJeor.htm — A
103. Wikipedia, *Harris–Benedict equation* (aviso de rango de validez) — https://en.wikipedia.org/wiki/Harris%E2%80%93Benedict_equation — B
104. QxMD, Mifflin-St Jeor (precisión ±10 %; Frankenfield 2005) — https://calculate.qxmd.com/storage/calculators/846/Mifflin-StJeorEquation.html — B
105. Deurenberg, Weststrate y Seidell, Br J Nutr 65:105 (1991) — https://pubmed.ncbi.nlm.nih.gov/2043597/ — A

**Reparto del músculo y entrenamiento (2026-11-15)**
113. Janssen et al., *Skeletal muscle mass and distribution in 468 men and women aged 18–88 yr*, J Appl Physiol 89:81 (2000) — https://pubmed.ncbi.nlm.nih.gov/10904038/ — A
114. *Total and regional appendicular skeletal muscle mass prediction from DXA body composition models*, Sci Rep 13:2590 (2023) — https://pmc.ncbi.nlm.nih.gov/articles/PMC9929067 — A
115. Holzbaur et al., *Upper limb muscle volumes in adult subjects*, J Biomech 40:742 (2007) — https://mobl.wordpress.ncsu.edu/files/2018/06/Holzbaur2007b.pdf — A
116. Holzbaur et al., *Moment-generating capacity of upper limb muscles in healthy adults*, J Biomech 40:2442 (2007) — https://stanfordhealthcare.org/publications/566/56691.html (resumen) — A
117. Handsfield et al., *Relationships of 35 lower limb muscles to height and body mass quantified using MRI*, J Biomech 47:631 (2014), doi:10.1016/j.jbiomech.2013.12.002 — resumen en https://wbldb.lievers.net/10346444.html — A (resumen)
118. Gabriel, Kamen y Frost, *Neural adaptations to resistive exercise*, Sports Med 36:133 (2006) — https://pubmed.ncbi.nlm.nih.gov/16464122/ — A
119. Folland y Williams, *The adaptations to strength training*, Sports Med 37:145 (2007) — https://pubmed.ncbi.nlm.nih.gov/17241104/ — A
120. Foley et al., *A comprehensive scoping review and meta-analysis of upper limb strength asymmetry*, Sci Rep 15 (2025), doi:10.1038/s41598-025-87413-w — https://library.kab.ac.ug/Record/doaj-art-711c681c55734b9ba6b466b523649370/Description — A (resumen)
121. Petersen et al., *Grip strength and hand dominance: challenging the 10% rule*, Am J Occup Ther 43:444 (1989) — https://jayscholar.etown.edu/facpubharvest/1567 — A. 121b: *A comparison of dominant and non-dominant hand strengths*, J Hand Surg Eur 24:421 (1999) — https://pubmed.ncbi.nlm.nih.gov/10473148/ — A
122. *The effect of acute sleep deprivation on skeletal muscle protein synthesis and the hormonal environment*, Physiol Rep 9:e14660 (2021) — https://acuresearchbank.acu.edu.au/item/8w4w7/ — A. 122b: Saner et al., J Physiol 598:1523 (2020) — https://pmc.ncbi.nlm.nih.gov/articles/PMC7217042 — A
123. Science for Sport, «Basic movement patterns» — https://www.scienceforsport.com/basic-movement-patterns/ — C (coaching)
124. Abe, Kearns y Fukunaga, Br J Sports Med 37:436 (2003) — https://bjsm.bmj.com/content/bjsports/37/5/436.full.pdf — A (n = 20)

**Verificación del 2026-11-14 (tarde)**
106. *Fasting: the history, pathophysiology and complications*, West J Med 1982 — https://pubmed.ncbi.nlm.nih.gov/6758355/ — A (resumen: ~0,9 kg/día la primera semana, ~0,3 kg/día hacia la tercera; la pérdida rápida inicial se debe sobre todo a balance negativo de sodio)
107. Huelguistas sudafricanos hospitalizados, S Afr Med J 83:391 (1993) — https://ajol.info/index.php/samj/article/view/157938/147536 — A (resumen: 420 ± 150 g/día de media)
108. Gétaz et al., *Hunger strike among detainees*, Swiss Med Wkly 142:w13675 (2012) — https://smw.ch/index.php/smw/article/download/1579/2043 — B (criterios de hospitalización: >10 % de pérdida, IMC <16,5–18, T <35,5 °C)
109. NBC News, «What Happens on a Hunger Strike?» (cita un documento del California Correctional Health Care Services) — https://www.nbcnews.com/news/amp/wbna51649659 — C (las punzadas de hambre suelen desaparecer a los 2–3 días de ayuno)

**Masa muscular (2026-11-14, noche)**
110. Calculadora Coachway (cita Lee 2000, Am J Clin Nutr 72:796, y Janssen 2000, J Appl Physiol 89:81) — https://coachway.io/tools/muscle-mass-calculator/ — B (segunda mano; originales sin abrir)
111. Heymsfield et al., *Simple skeletal muscle mass estimation formulas*, Front Endocrinol 11:31 (2020) — https://pmc.ncbi.nlm.nih.gov/articles/PMC7012897 — A
112. Janssen et al., *Estimation of skeletal muscle mass by bioelectrical impedance analysis*, J Appl Physiol 89:465 (2000) — https://www.citedrive.com/en/discovery/estimation-of-skeletal-muscle-mass-by-bioelectrical-impedance-analysis — A (resumen)

**Adaptación nerviosa y déficit energético (2026-11-15)**
125. Murphy et al., *Energy deficiency impairs resistance training gains in lean mass but not strength*, Scand J Med Sci Sports 32(1) (2022) — https://portal.fis.tum.de/en/publications/energy-deficiency-impairs-resistance-training-gains-in-lean-mass-/ — A (solo resumen)
126. Stronger By Science, reseña del anterior — https://www.strongerbyscience.com/muscle-caloric-deficit/ — B (secundaria, con cifras de la metarregresión)
127. Helms et al., *Dietary protein during caloric restriction in resistance trained lean athletes*, Int J Sport Nutr Exerc Metab 24:127 (2014) — https://pubmed.ncbi.nlm.nih.gov/24092765/ — A (resumen; N = 6 estudios)
128. *Cross-education: mecanismos*, Front Physiol 8:297 (2017) — https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2017.00297/full — B (revisión crítica; leídos ~100 000 de 125 000 caracteres)
129. Manca et al., *Cross-education of muscular strength following unilateral resistance training: a meta-analysis*, Eur J Appl Physiol 117:2335 (2017) — https://iris.uniss.it/handle/11388/200540 — A (solo se vio el resumen; la cifra 8–12 % la cita el metaanálisis de [130])
130. Green y Gabriel, metaanálisis de cross-education (96 estudios), 2018 — https://informahealthcare.com/doi/abs/10.1080/10833196.2018.1499272 y https://peeref.com/works/42709971 — B (fragmentos de buscador; no consta si el 18 % es del lado no entrenado)
131. Metaanálisis de dominancia en cross-education, Eur J Appl Physiol (2024) — https://link.springer.com/content/pdf/10.1007/s00421-024-05472-9.pdf — A (3 estudios, 5 tamaños de efecto)
132. Pearcey et al., *Chronic resistance training: is it time to rethink the time course of neural contributions to strength gain?*, Eur J Appl Physiol 121:2413 (2021) — https://link.springer.com/10.1007/s00421-021-04730-4 — A (resumen; texto de pago)
133. Škarabot et al., *The knowns and unknowns of neural adaptations to resistance training*, Eur J Appl Physiol 121:675 (2021) — https://nrl.northumbria.ac.uk/id/eprint/44822 — A (resumen)
134. Morton et al., *Protein supplementation and resistance training-induced gains*, Br J Sports Med 52:376 (2018) — https://pubmed.ncbi.nlm.nih.gov/28698222/ — A (49 estudios, 1 863 participantes; meseta de ganancia de masa libre de grasa hacia 1,62 g/kg/día)

**Estado de la revisión de fuentes (2026-11-14)**

Comprobadas contra la fuente: [11] (confirmada, con el matiz de la sal), [34] (Hall: 17 kg de media
en Minnesota, confirmado), [35] (Harcombe: meseta hacia la semana 20, y pérdida real menos de la
mitad de la predicha por la regla de las 3 500 kcal/lb; son **cálculos de una columnista**, no
de Keys; sus cifras de kcal son inconsistentes: ~1 570/1 600), fórmulas del código (§2).
No comprobables: [13] y [29] (ver arriba). Aún **sin revisar de nuevo**: el resto de fuentes, en
particular [27] (Aeviva, C) y [14]. La revisión completa sigue pendiente.

> Nota: los números `[27]`, `[28]` y `[39]`, `[63]`, `[77]` y `[85]` remiten a fuentes ya listadas;
> están separados solo para que cada afirmación señale su origen.
