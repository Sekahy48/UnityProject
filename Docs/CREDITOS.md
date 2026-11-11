# Créditos — recursos de terceros

Todo lo que no ha hecho Sergio y se usa en el proyecto, con su origen y licencia. Pedido por
David (reunión del 2026-10-29): la memoria y el juego tienen que acreditarlo.

**Estado:** ✅ verificado (hay archivo de licencia o fuente clara) · ⚠️ probable (deducido del
nombre, falta confirmar) · ❓ origen desconocido (Sergio tiene que decir de dónde salió).

Inventario hecho por Claude el 2026-11-08 recorriendo `Assets/` y `Packages/manifest.json`.

## Arte y recursos

| Recurso | Dónde | Origen / autor | Licencia | Estado |
|---|---|---|---|---|
| Texturas de UI «RPG extension» | `UI Toolkit/Theme/textures/kenney-rpg/` | Kenney Vleugels (kenney.nl) | CC0 | ✅ |
| Texturas de UI «Fantasy UI Borders» | `UI Toolkit/Theme/textures/kenney-borders/` | Kenney (kenney.nl) | CC0 | ✅ |
| Fuente Alegreya Sans SC | `UI Toolkit/Inventory/fonts/` | The Alegreya Sans Project Authors (Huerta Tipográfica) | SIL OFL 1.1 | ✅ |
| Fuente Vollkorn | `UI Toolkit/Inventory/fonts/` | The Vollkorn Project Authors (Friedrich Althausen) | SIL OFL 1.1 | ✅ |
| Fuente Liberation Sans | `TextMesh Pro/Fonts/` | Viene con TextMesh Pro (Unity) | SIL OFL 1.1 | ✅ |
| Emojis de ejemplo (EmojiOne) | `TextMesh Pro/Sprites/` | EmojiOne, incluidos en TextMesh Pro | ver sus condiciones; solo los usa el menú principal desactivado | ✅ (sin uso real) |
| Personaje «Paladin WProp J Nordstrom» y animaciones (Idle, Running, Walking, Left Strafe, Left Turn, Backflip, Sprinting Forward Roll) | `Resources/3D/Animations/`, `Resources/3D/Textures/` | Mixamo (Adobe) | licencia de Mixamo: uso libre en proyectos; no redistribuir los archivos sueltos | ⚠️ |
| Personaje «Ganfaul M Aure» | `Resources/3D/Textures/Ganfaul M Aure/` | Mixamo (Adobe), por el nombre | ídem | ⚠️ |
| Modelo «Shadowed_Archer_in_th_0916224007» | `Resources/3D/Models/` | por el nombre, generado con una herramienta de IA de modelos 3D (tipo Meshy) | según la herramienta | ⚠️ confirmar herramienta |
| Iconos de ítems (arco, espada, manzana, odre, mochila, pechera, camisa, yelmo, venda…) | `StreamingAssets/images/` | — | — | ❓ |
| Siluetas de slots de equipo | `StreamingAssets/images/slots/` | — | — | ❓ |
| Modelos .glb de ítems (manzana, arcón pequeño) | `StreamingAssets/models/` | — | — | ❓ |
| Orbe de sangre (`descarga.jpg`, `Empty_Orb.png`, `EmptyOrb2.png`) | `Resources/Images/` | `descarga.jpg` suena a descargado de internet | — | ❓ (sin uso actual) |
| `health_placeholder.png`, `subslots-inspect.png` | `UI Toolkit/Inventory/images/` | — | — | ❓ |
| Iconos de estado de peso (placeholder) | `UI Toolkit/HUD/images/status/` | generados por Claude (2026-10) | propios del proyecto | ✅ (sustituir por arte final) |

## Software y bibliotecas

| Componente | Uso | Licencia | Estado |
|---|---|---|---|
| Unity 6 (6000.1.17f1) y sus paquetes (URP, Input System, Cinemachine, UI Toolkit, TextMesh Pro, Test Framework…) | motor | Unity Terms of Service / Unity Companion License | ✅ |
| glTFast (`com.unity.cloud.gltfast`) | carga de modelos .glb exportados por Stack&Go | Apache 2.0 | ✅ |
| Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`) | lectura del catálogo JSON | MIT | ✅ |
| NUnit | tests EditMode | MIT | ✅ |
| Claude (Anthropic) | asistencia en revisión, diagnóstico, vistas y tests | — (declarar en la memoria) | ✅ |

## A revisar

1. **Los ❓:** Sergio, ¿de dónde salió cada uno (dibujado por ti, por tu hermana, generado con
   IA, descargado)? Si alguno es de internet sin licencia clara, conviene sustituirlo antes de
   la presentación.
2. ~~Nombre de una carpeta~~ **Resuelto (2026-11-11):** la carpeta de «Ganfaul M Aure» tenía
   un nombre ofensivo; renombrada por Sergio a `Ganfaul M Aure`.
