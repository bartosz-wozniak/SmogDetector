namespace SmogDetector.Task.Business
{
    public interface IImgwDataSynchronizer
    {
        void Sync();

        void ImportClassifications();

        void ImportStations();
    }
}
