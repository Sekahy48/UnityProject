using ECS.Entity;

namespace MVC.Presenter 
{
    public interface IPresenter
    {
        void Open(IEntity entity);
        void Close(bool absolute);
        bool IsOpen();
        void Refresh();
    }
}
