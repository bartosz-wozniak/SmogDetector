using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmogDetector.DataAccess.Models
{
    // ReSharper disable once PartialTypeWithSinglePart
    [Table("RandomForestStats")]
    public partial class RandomForestStats
    {
        public int Id { get; set; }

        [Required]
        public int Trees { get; set; }

        [Required]
        public double Oob { get; set; }

        public string Description { get; set; }
    }
}
