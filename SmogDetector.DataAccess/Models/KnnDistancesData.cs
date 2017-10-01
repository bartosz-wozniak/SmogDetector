using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("KnnDistancesData")]
    public partial class KnnDistancesData
    {
        public int Id { get; set; }

        [Required]
        public double? Distance { get; set; }

        [Required]
        public DateTime TestingSet { get; set; }

        [Required]
        public DateTime TrainingSet { get; set; }
    }
}
