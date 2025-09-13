using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace OnlineAppointment.Infrastructure
{
    public class AppEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
            => Task.CompletedTask; // e-posta göndermiyoruz
    }
}
