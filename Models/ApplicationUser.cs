using Microsoft.AspNetCore.Identity;

namespace OnlineAppointment.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string  FullName { get; set; } = "";
    }
}
