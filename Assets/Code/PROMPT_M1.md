# Contexto para nuevo chat de Cowork — Milestone 1

## Proyecto
Artisan es un juego Unity con arquitectura ECS custom (Entity-Component, no Unity DOTS). El código está separado en `Assets/Code/Core/` (C# puro) y `Assets/Code/Unity/` (dependencias Unity). Hay interfaces bridge para desacoplar (ILogger, IEntityLinker, etc.).

## Documento de referencia
Lee `Assets/Code/FASE1_HITOS.md` — contiene todas las decisiones de arquitectura y los 7 milestones de la Phase 1 (sistema de inventario). Está todo decidido, no hay nada ambiguo.

## Tarea
Implementar el **Milestone 1 — Item catalog & numeric IDs**. Las tareas concretas están en el documento.

## Archivos que vas a necesitar leer/modificar
- `Core/ECS/Component/InventoryComponents/BaseItemComponent.cs` — refactorizar para que delegue a ItemDefinition
- `Core/ECS/Entity/ItemEntity.cs` — añadir typeId
- `Core/Inventory/IInventoryElement.cs` — GetId() pasa de string a int
- `Core/Inventory/InventoryObject.cs` — actualizar BFS a typeId
- `Core/Inventory/ItemObject.cs` — NO tocar, se sustituye en M2
- `Core/Factories/PrototypeFactory.cs` — actualizar para crear desde catálogo
- Crear nuevos: `ItemDefinition.cs`, `ItemCatalog.cs`, interfaz `IItemCatalogLoader.cs`

## Enfoque de desarrollo
**MUY IMPORTANTE**: No implementes directamente. Hazme pensar y aprender. Que yo sea un agente real de la planificación de la arquitectura. Hazme preguntas de diseño antes de escribir código, explícame las implicaciones de cada decisión, y deja que yo dirija. Cuando llegue el momento de codear, explícame qué vas a hacer y por qué antes de hacerlo.

## Conocimiento del proyecto
Hay un knowledge source llamado "Artisan" con documentación adicional del proyecto. Consúltalo si necesitas contexto extra.
