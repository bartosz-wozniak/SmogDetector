using System.ComponentModel.DataAnnotations;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    public partial class ImgwClassification
    {
        public int Id { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }

        public string Unit { get; set; }

        public int? HoursCounter { get; set; }

        public int? Counter { get; set; }

        public double? PerHourCounter { get; set; }
    }
}
