namespace Strategy
{
    public interface ICameraStrategy
    {
        void Execute(float deltaTime);

        void activate();
        void deactivate();
    }
    
}