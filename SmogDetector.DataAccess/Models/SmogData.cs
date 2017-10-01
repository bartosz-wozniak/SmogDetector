using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("SmogData")]
    public partial class SmogData
    {
        public int Id { get; set; }

        [Required]
        public string Station { get; set; }

        public int Value { get; set; }

        [Required]
        public string Type { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}
