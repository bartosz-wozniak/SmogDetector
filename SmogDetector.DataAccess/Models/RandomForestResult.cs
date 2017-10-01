using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("RandomForestResults")]
    public partial class RandomForestResult
    {
        public int Id { get; set; }

        [Required]
        public DateTime TestingSet { get; set; }

        [Required]
        public int PredictedClass { get; set; }

        [Required]
        public int CorrectClass { get; set; }

        public string Description { get; set; }
    }
}
