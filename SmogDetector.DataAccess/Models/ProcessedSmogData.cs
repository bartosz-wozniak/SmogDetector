using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("ProcessedSmogData")]
    public partial class ProcessedSmogData
    {
        public int Id { get; set; }

        public int? Value { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public int? Class { get; set; }
    }
}
