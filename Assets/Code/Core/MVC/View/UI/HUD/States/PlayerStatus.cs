using Core.MVC.View.UI.HUD.States;

namespace Core.MVC.View.UI.HUD
{
    public struct PlayerStatus    {
        public readonly StatusCategory Category {get;}
        public readonly int Level {get;}

        public PlayerStatus(StatusCategory category, int level)
        {
            Category = category;
            Level = level;
        }
    }
}