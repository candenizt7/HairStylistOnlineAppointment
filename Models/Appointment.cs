using System.ComponentModel.DataAnnotations;

namespace OnlineAppointment.Models
{
    public enum AppointmentStatus { Pending, Confirmed, Cancelled, Completed }

    public class Appointment
    {
        public int Id { get; set; }                  // Primary key

        public string CustomerId { get; set; }       // ApplicationUser.Id
        public ApplicationUser Customer { get; set; }

        [Required(ErrorMessage = "Hizmet seçiniz.")]
        public int ServiceId { get; set; }           // Alınan hizmet
        public Service Service { get; set; }

        [Required(ErrorMessage = "Başlangıç zamanı giriniz.")]
        [Display(Name = "Başlangıç")]
        public DateTime Start { get; set; }          // Başlangıç zamanı
        public DateTime End { get; set; }            // Bitiş zamanı
            
        public AppointmentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }      // Oluşturulma zamanı
    }
}
