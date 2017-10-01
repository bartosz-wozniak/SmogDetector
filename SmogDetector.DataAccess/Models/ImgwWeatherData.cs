using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("ImgwWeatherData")]
    public partial class ImgwWeatherData
    {
        public int Id { get; set; }

        public double? Value { get; set; }

        public int Status { get; set; }

        public DateTime? ValueDate { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public string CbdoCbdh { get; set; }

        [Required]
        public int ClassificationId { get; set; }

        [Required]
        public int StationId { get; set; }
    }
}
