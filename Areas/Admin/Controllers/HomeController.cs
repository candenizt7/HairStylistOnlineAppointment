using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;

namespace OnlineAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")] // sadece Admin rolündekiler
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        // Basit bir dashboard: hizmet sayısı, bekleyen randevu sayısı, bugünkü randevular
        public async Task<IActionResult> Index()
        {
            var vm = new ViewModels.AdminDashboardVM
            {
                ServiceCount = await _db.Services.CountAsync(),
                PendingCount = await _db.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
                TodayCount = await _db.Appointments.CountAsync(a =>
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.Start >= DateTime.Today &&
                                   a.Start < DateTime.Today.AddDays(1))
            };

            return View(vm);
        }
    }
}
