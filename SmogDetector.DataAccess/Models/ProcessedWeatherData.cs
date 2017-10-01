using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("ProcessedWeatherData")]
    public partial class ProcessedWeatherData
    {
        public int Id { get; set; }

        public double? Value { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int Classification { get; set; }
    }
}
