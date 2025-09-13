using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineAppointment.Models
{
    public class Service
    {
        public int Id { get; set; }                 // Primary key

        [Required, StringLength(80)]
        [Display(Name = "Service name")]
        public string Name { get; set; }   // Örn: "Saç Kesimi"

        [Range(5, 300)]
        [Display(Name = "Duration (minutes)")]
        public int DurationMinutes { get; set; }  // Örn: 30

        [Range(0, 10000)]
        [DataType(DataType.Currency)]
        [Display(Name = "Price")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
    }
}
