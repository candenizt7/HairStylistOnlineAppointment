using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineAppointment.ViewModels
{
    public class AppointmentCreateVM
    {
        [Required]
        public int ServiceId { get; set; }

        [Required]
        public DateTime Start { get; set; }
    }
}
