using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("KnnResultsData")]
    public partial class KnnResultsData
    {
        public int Id { get; set; }

        [Required]
        public DateTime TestingSet { get; set; }

        [Required]
        public int KnnClass1 { get; set; }

        [Required]
        public int KnnClass5 { get; set; }

        [Required]
        public int KnnClass15 { get; set; }

        [Required]
        public int CorrectClass { get; set; }

        public string Description { get; set; }
    }
}
