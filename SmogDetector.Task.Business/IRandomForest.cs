namespace SmogDetector.Task.Business
{
    public interface IRandomForest
    {
        void Run(int trees = 100);

        void Test();
    }
}
