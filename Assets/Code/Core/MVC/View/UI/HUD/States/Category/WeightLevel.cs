using System.ComponentModel;

namespace Core.MVC.View.UI.HUD.States.Category
{
    public enum WeightLevel
    {
        [Description("")]
        None = 0,
        [Description("Llevas algo de peso de más, te notas más lento.")]
        Extra = 1,
        [Description("Llevas demasiado peso, no puedes correr.")]
        Over = 2,
        [Description("No puedes moverte, llevas demasiado peso como para mover un musculo.")]
        Immobile = 3
    }
}