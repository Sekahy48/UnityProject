using System;

/// <summary>
/// Una estrategia de camara que puede interactuar con el mundo. Hermana de
/// <see cref="IInventoryInputSource"/> y con la misma regla: la estrategia solo traduce la
/// tecla en intenciones, y quien decide que hacer con ellas es el presentador.
///
/// <para>Tres eventos y no uno con un parametro porque son tres intenciones distintas:
/// ejecutar ya, abrir el menu, y decidir en el menu. Distinguir toque de mantener es
/// interpretar la tecla, asi que pasa aqui y no en el presentador.</para>
/// </summary>
public interface IWorldInteractionInputSource
{
    /// <summary>La tecla se solto antes del umbral de mantener: accion por defecto.</summary>
    event Action OnInteractTapped;

    /// <summary>La tecla supero el umbral y sigue pulsada: abrir el menu radial.</summary>
    event Action OnInteractHoldStarted;

    /// <summary>La tecla se solto despues de mantener: decidir en el menu.</summary>
    event Action OnInteractHoldReleased;
}
