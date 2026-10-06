using System;

namespace Core.MVC.View.UI.HUD
{
    /// <summary>
    /// Lo que el presentador del HUD necesita de su vista. Recibe proporciones (0..1) ya
    /// calculadas: la vista no sabe que es la estamina, solo cuanto llenar.
    /// </summary>
    public interface IHUDView : IView
    {
        void Show();
        void Hide();

        void SetStamina(float ratio);
        void SetFatigue(float ratio);

        void AddState(PlayerStatus state, String description);
    }
}
