# Bitácora de desarrollo — Artisan

Registro de las sesiones de trabajo: qué se hizo, cuándo, cuánto tiempo y con quién. Sirve
para la planificación de la memoria del TFG (horas reales frente a créditos, diagrama de
Gantt) y para que una sesión nueva sepa qué pasó en las anteriores.

**Cómo se escribe:** una entrada por sesión, la más reciente al final. Al cerrar cada sesión
con Claude se añade su entrada (regla en `CLAUDE.md`); las sesiones de Sergio en solitario se
añaden cuando él las cuente.

**Horas:** las da Sergio. Si no hay dato, se escribe «sin dato»; nunca se inventan. Las que
Claude deduzca del ritmo de la conversación van marcadas como «estimación, confirmar».

Formato de cada entrada:

```
### AAAA-MM-DD — título corto
- **Horas:** N h | sin dato | ~N h (estimación, confirmar)
- **Con:** Sergio | Sergio + Claude
- **Fase / hito:** ...
- **Hecho:** 2–4 líneas.
- **Commits:** hashes, si los hay.
- **Pendiente que deja:** opcional.
```

---

## Reconstrucción (antes de 2026-11-08)

Reconstruido el 2026-11-08 a partir del historial de git, `FASE1_HITOS.md`, `PR_FASE_0.md` y
la memoria del proyecto. Los commits de esa época son pocos y grandes, así que cada fila es un
bloque de trabajo, no una sesión. **Horas: sin dato en todas**; Sergio puede añadir una
estimación por bloque si la recuerda.

| Periodo | Fase / hito | Qué se hizo | Commits |
|---|---|---|---|
| 2025-09-17 → 09-29 | Prehistoria | Proyecto Unity inicial: enlace del jugador nativo con el de Unity, cámara en primera persona desacoplada del motor. | 4e5b72d … c3eb5b0 |
| 2025-10-18 → 10-27 | Diseño | Diagramas UML (`Diagrams/UML`): componentes, sistema de construcción, sistema de inventario. | 11f25f3 … ddd0fec |
| 2026-03-02 → 04-11 | Sin describir | Commits de cambio de dispositivo y «subo lo que tengo»; el contenido no está descrito. | 5f2c6e3, 875d5c6, 5fc0d74, fb13493 |
| 2026-07-06 → 07-08 | **Fase 0** | Revisión y limpieza del código heredado; separación en `Core/` (C# puro) y `Unity/`; `FisiologicComponent` partido en varios componentes. PR #1. | 06ca342 … 9772696 |
| 2026-07-11 → 07-17 | Fase 1 · M1 | Documento de hitos de la Fase 1; logger y configuración independientes de Unity; catálogo de ítems con ids numéricos y carga JSON desde Stack&Go. | a78c61a … 93f880a |
| 2026-07-18 → 07-21 | Fase 1 · M2 | Apilado por sublotes (`BatchItem`, refactor a Composite). | 18293ba, 27ec133 |
| 2026-07-23 | Fase 1 · M3 | Límites de peso y de espacio en la rejilla. | 67564c1 |
| 2026-07-27 | Fase 1 · M4 | Rediseño del sistema de equipo (capas, slots bloqueables). | 8cc8263 |
| 2026-08-03 → 09-18 | Fase 1 · M5 | Interfaz de la rejilla tetris: cruz de equipo, pestañas, mano, catálogo, barra de peso, movimientos transaccionales, gestos de equipar, cantidades parciales, intercambio, sublotes y subinventarios. Cambio a Unity 6000.1.17 y normalización de finales de línea. | 3f6c727 … 76fa9ea |
| 2026-09 (aprox. hasta 09-24) | Stack&Go | Módulo de modelos 3D en Stack&Go y su exportación a Artisan (repo aparte). | 5e91036 |
| 2026-09-24 → 09-30 | Fase 1 · M6 | Interacción con el mundo: recoger del suelo, apuntar con la cámara, menú radial, panel de inspeccionar, temas medievales, paneles laterales A/B, abrir inventarios del mundo. | aedd3ce … 90d6f46 |
| 2026-10-01 → 10-06 | Fase 1 · M7 | Core como ensamblado propio y tests EditMode; paleta de temas en variables; sobrecarga del cuerpo y «Coger» al estilo RimWorld; HUD como presentador y vista con estados de peso. | 4e74358 … b624482 |
| 2026-10-26 → 10-29 | Fase 1 · cierre | Helper de banda de carga; refactor de clases USS a BEM; tooltip de estados arreglado; cierre de la Fase 1 en HITOS (T6 de rendimiento aplazada). | 192c1eb, 2fe4772, af3a46f |
| 2026-10-29 | Reunión TFG | Primera reunión con David Sobrín. Notas en la entrada de abajo y en `NOTAS_MEMORIA.md`. | — |
| 2026-11-07 | Fase 1 · arreglo | La banda de carga no llegaba al portador de una mochila: subida por la cadena de inventarios y `Rebind` del inventario al clonar entidades. Tests `CarrierLoadTests`; Claude ejecuta los tests del Core con .NET fuera de Unity. | 149a6ea |

### 2026-10-29 — Reunión con David Sobrín (tutor)
- **Fecha:** jueves 29 (confirmada por Sergio).
- **Horas:** sin dato
- **Con:** Sergio y David
- **Fase / hito:** planificación del TFG
- **Hecho:** David ve el proyecto como algo serio, no como un trámite. Recomendaciones para la
  memoria: justificar el motor, estudiar la competencia, consultar el Libro Blanco del
  Desarrollo Español de Videojuegos, plantear una vertical slice orientada a buscar
  financiación (publisher), cuidar los créditos de todo lo que se use de terceros e ir
  apuntando la planificación. Prioridades orientativas de mecánicas: fisiología, crafteo,
  diálogos (NPC básicos). Pidió identificar todo lo que se quiere en el juego. Próxima
  reunión: el jueves siguiente.
- **Decisiones de Sergio:** el orden de las fases es suyo; la fisiología va a continuación;
  los NPC quedan en segundo plano (Fuero se desarrolla en paralelo).
- **Pendiente que deja:** catálogo de mecánicas y roadmap mk1 de fases.

---

## Entradas

### 2026-11-08 — Planificación tras la reunión
- **Horas:** ~1 h (estimación de Sergio)
- **Con:** Sergio + Claude
- **Fase / hito:** planificación (entre la Fase 1 y la siguiente)
- **Hecho:** decidido cómo se organiza la planificación: bitácora (este documento), notas
  para la memoria y créditos en `Docs/`, catálogo de mecánicas y roadmap por fases en dos
  niveles, alcance del TFG por líneas de corte (mínimo, objetivo, ampliación) y estrategia
  mixta: fisiología en profundidad, el resto en versión vertical slice salvo lo que Sergio
  marque para profundizar.
- **Pendiente que deja:** identificar mecánicas con el método acordado; roadmap mk1.

### 2026-11-11 — Catálogo de mecánicas, revisión 2
- **Horas:** ~3 h 30 min (estimación de Sergio): ~2 h 30 min hasta el cierre de la Fase 1 en
  `main` y ~1 h más definiendo la Fase 1.5 (ya entrada la madrugada del 12)
- **Con:** Sergio + Claude
- **Fase / hito:** planificación (entre la Fase 1 y la siguiente)
- **Hecho:** Sergio revisa el borrador del catálogo y responde a sus preguntas; Claude rehace
  `MECANICAS.md` con sus notas (etiquetas N/D/F, columna «motivación», propuestas sin resolver
  aparte; principio del cuerpo realista; ánimo; agua con investigación pendiente; temperatura
  corporal y del mundo; tallado interactivo sencillo dentro de un menú de crafteo por modos;
  lanza y arpón distintos). Carpeta con nombre ofensivo renombrada por Sergio (`Ganfaul M Aure`).
- **Hecho después (misma sesión):** versión del catálogo para la reunión
  (`MECANICAS_REUNION.md`, con estado de implementación); `ROADMAP.md` mk1 con la alternativa
  B (fisiología partida e intercalada con lo que la alimenta); decidida una Fase 1.5 (soltar
  con física y guardado base) y el guardado progresivo como criterio de cierre de cada fase;
  colocar objetos va a la Fase 5 junto a la construcción. Revisados el Libro Blanco DEV 2025 y
  la plantilla de documento de diseño que pasó David: útiles para la memoria y para presentar,
  en espera para el desarrollo. `CLAUDE.md` apunta ya a los hitos de la fase en curso y recoge
  el modo socrático. `FASE1_HITOS.md`: fase terminada.
- **Fase 1.5:** PR #2 fusiona la Fase 1 en `main`; rama `fase1.5`. Definida en modo
  socrático en `Assets/Code/FASE1_5_HITOS.md`: soltar con física (todo cae y rueda, masa =
  peso, el jugador se mueve por física y lleva su carga), guardado por entidades con ids y
  contrato `ISaveComponent` al estilo del catálogo, JSON, autoguardado rotativo, sin migración
  de versiones hasta tener una estable. Lanzar objetos, aplazado.
- **Pendiente que deja:** reunión del jueves 12; propuesta cerrada de hitos de la Fase 1.5;
  investigación del agua antes de la Fase 2. Decidido: el desgarro es un corte con la marca «irregular» (en la interfaz se
  ve como desgarro); guardar y cargar partida entra como N.

### 2026-11-12 — Segunda reunión con David y replanificación
- **Horas:** reunión ~45 min (10:00–10:45); replanificación con Claude sin dato (sesión en
  curso, sigue con la Fase 1.5)
- **Con:** Sergio y David; después Sergio + Claude
- **Fase / hito:** planificación
- **Hecho (reunión):** Stack&Go y Fuero le parecen bien. Stack&Go: herramienta común; como la
  base fue de Ingeniería del Software, quizá no proceda en la solución (Sergio quiere
  delimitar lo hecho para Artisan; a acordar). Fuero: se puede presentar como módulo aunque no
  se conecte, con el visor de BISI. Viendo solo la fisiología, David vuelve a temer por el
  alcance: pide identificar el tronco y centrarse en él, y tener la fisiología antes que el
  resto. Soltar con física: bien, es poco trabajo. Guardar partida: no perder tiempo; una
  vertical slice se enseña con un escenario preparado.
- **Hecho (replanificación):** `ROADMAP.md` mk2: fisiología entera en la Fase 2, ordenada por
  capas (tronco, capa 1, capa 2) y probada con menú de depuración y escenarios; Fase 1.5 solo
  mundo físico; guardado aplazado a después del TFG (diseño conservado en `FASE1_5_HITOS.md`);
  líneas de corte defensivas (mínimo: 1.5 + Fase 2 hasta la capa 1).
- **Hecho después (misma sesión):** decisiones 25–32 de la Fase 1.5 e hitos M1–M6 en
  `FASE1_5_HITOS.md`; Sergio empieza M1 (Rigidbody en la cámara, `GetSpeed` con el correr).
  Claude: estándares de código (`Docs/ESTANDARES_CODIGO.md`) y limpieza de todo el repositorio
  (campos privados a `_camelCase`, propiedades, `this.`, comentarios al español, erratas); Core
  compilado y 56 tests en verde fuera de Unity.
- **Pendiente que deja:** M1 (mover al jugador por física); compilar en Unity la limpieza;
  investigación del agua.
