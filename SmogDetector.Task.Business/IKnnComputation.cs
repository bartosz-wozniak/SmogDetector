namespace SmogDetector.Task.Business
{
    public interface IKnnComputation
    {
        void ComputeDistances();

        void FindClasses();
    }
}
