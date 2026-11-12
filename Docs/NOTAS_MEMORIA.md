# Notas para la memoria del TFG

Cosas que hay que tener en cuenta al escribir la memoria. No es la memoria: es la lista de
lo que no se puede olvidar, con de dónde sale cada punto. Se va ampliando según aparezcan.

## Recomendaciones de David (reunión del 2026-10-29)

1. **Justificar el motor.** Por qué Unity (C#, familiaridad, UI Toolkit, ecosistema) y no
   Godot u otro. Hay material previo: el debate Unity/Godot está resumido en
   `Artisan_Reporte_Contexto.docx` («Posibles cambios de motor — debate abierto»). Conviene
   añadir lo que pesa a favor de la arquitectura actual: el Core no depende del motor, así
   que un cambio de motor afectaría solo a la capa Unity.
2. **Competencia.** Juegos de referencia y en qué se diferencia Artisan. Punto de partida: la
   lista de referentes del reporte de contexto (Medieval Dynasty, Project Zomboid, DayZ,
   RimWorld, Going Medieval, TerraFirmaCraft, Dwarf Fortress, Bannerlord…).
3. **Libro Blanco del Desarrollo Español de Videojuegos** (DEV). Contexto del sector en
   España: tamaño de estudios, financiación, salidas. Útil para el apartado de viabilidad.
4. **Vertical slice orientada a financiación.** Para la presentación (julio) conviene tener
   una porción jugable y pulida del ciclo de juego que sirva como demo ante un publisher.
   Encaja con la estrategia acordada: fisiología en profundidad, el resto en versión slice.
5. **Créditos.** Todo lo que se use de terceros (texturas, fuentes, modelos, animaciones,
   bibliotecas) tiene que estar acreditado. Inventario y estado en `CREDITOS.md`.
6. **Planificación.** Ir apuntando el trabajo real: `BITACORA.md`. De ahí salen las horas por
   fase y el diagrama de Gantt.
7. **Identificar todo lo que se quiere en el juego.** Catálogo de mecánicas y roadmap por
   fases (en preparación).

## Otros puntos que la memoria tiene que cubrir

- **Uso de IA.** El desarrollo se ha hecho con ayuda de Claude (Anthropic): revisión,
  diagnóstico, vistas y tests; el núcleo lo escribe Sergio. Hay que declararlo y explicar el
  reparto. La regla del reparto está en `CLAUDE.md` («El código del núcleo lo escribe
  Sergio»).
- **Herramienta propia: Stack&Go.** Aplicación JavaFX de Sergio para definir ítems y
  exportarlos a Artisan (JSON, iconos, modelos 3D). **David (2026-11-12):** es una herramienta
  común en proyectos así; como la base se hizo para Ingeniería del Software, quizá no proceda
  en la solución. **Postura de Sergio, a acordar con David:** delimitar con fechas (historial
  de git) qué es la base de Ingeniería del Software y qué se hizo para Artisan (refactor de
  arquitectura, componentes de ítem, exportación, módulo de modelos 3D), en un apartado de
  «herramientas de soporte desarrolladas».
- **Fuero (sistema social de NPC, asignatura BISI).** David (2026-11-12): se conecte o no al
  juego, se puede presentar como módulo desarrollado, enseñándolo con el visor de BISI.
- **Arquitectura.** ECS propio, separación Core/Unity, MVP con bus de eventos. El
  razonamiento está en `Assets/Code/FASE1_HITOS.md` y `PR_FASE_0.md`.
- **Pruebas.** Tests EditMode del Core (56 a 2026-11-07), ejecutables también fuera de Unity
  con .NET.
- **Referencia de estructura:** el TFG de Alejandro Alonso Pérez (en el proyecto de Claude):
  planificación por sprints, presupuesto por roles, pruebas con usuarios.
