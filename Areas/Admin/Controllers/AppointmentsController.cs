using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;

namespace OnlineAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db) => _db = db;

        // GET: /Admin/Appointments?filter=Pending|Today|All
        public async Task<IActionResult> Index(string? filter = "Pending")
        {
            IQueryable<Appointment> q = _db.Appointments
                .AsNoTracking()
                .Include(a => a.Service)
                .Include(a => a.Customer);

            switch (filter)
            {
                case "Today":
                    var start = DateTime.Today;
                    var end = start.AddDays(1);
                    q = q.Where(a => a.Status != AppointmentStatus.Cancelled &&
                                     a.Start >= start && a.Start < end);
                    break;

                case "All":
                    // tümünü göster (filtre yok)
                    break;

                default: // "Pending"
                    q = q.Where(a => a.Status == AppointmentStatus.Pending);
                    break;
            }

            var list = await q.OrderBy(a => a.Start).ToListAsync();
            ViewBag.Filter = filter;
            return View(list);
        }

        // POST: /Admin/Appointments/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status == AppointmentStatus.Pending)
            {
                appt.Status = AppointmentStatus.Confirmed;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Appointment approved.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Appointments/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status != AppointmentStatus.Completed &&
                appt.Status != AppointmentStatus.Cancelled)
            {
                appt.Status = AppointmentStatus.Cancelled;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Appointment cancelled.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Appointments/Complete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var appt = await _db.Appointments.FindAsync(id);
            if (appt == null) return NotFound();

            if (appt.Status == AppointmentStatus.Confirmed)
            {
                appt.Status = AppointmentStatus.Completed;
                await _db.SaveChangesAsync();
                TempData["Success"] = "Appointment marked as completed.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
