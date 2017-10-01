namespace SmogDetector.Task.Business
{
    public interface IDataNormalizer
    {
        void ComputeClassifications();

        void ProcessSmogData();

        void ProcessWeatherData();

        void NormalizeWeatherData();
    }
}
