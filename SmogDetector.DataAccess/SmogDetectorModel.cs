using SmogDetector.DataAccess.Models;

namespace SmogDetector.DataAccess
{
    using System.Data.Entity;

    // ReSharper disable once PartialTypeWithSinglePart
    public partial class SmogDetectorModel : DbContext
    {
        public SmogDetectorModel() : base("name=SmogDetectorModel")
        {

        }

        public virtual DbSet<ImgwClassification> ImgwClassifications { get; set; }

        public virtual DbSet<ImgwStation> ImgwStations { get; set; }

        public virtual DbSet<ImgwWeatherData> ImgwWeatherData { get; set; }

        public virtual DbSet<SmogData> SmogData { get; set; }

        public virtual DbSet<ProcessedSmogData> ProcessedSmogData { get; set; }

        public virtual DbSet<ProcessedWeatherData> ProcessedWeatherData { get; set; }

        public virtual DbSet<NormalizedWeatherData> NormalizedWeatherData { get; set; }

        public virtual DbSet<KnnDistancesData> KnnDistancesData { get; set; }

        public virtual DbSet<KnnResultsData> KnnResultsData { get; set; }

        public virtual DbSet<RandomForestResult> RandomForestResults { get; set; }

        public virtual DbSet<RandomForestStats> RandomForestStats { get; set; }

        // ReSharper disable once RedundantOverriddenMember
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
