using ECS.Entity;

namespace MVC.Presenter 
{
    public interface IPresenter
    {
        void Open(IEntity entity);
        void Close();
        bool IsOpen();
        void Refresh();
    }
}
