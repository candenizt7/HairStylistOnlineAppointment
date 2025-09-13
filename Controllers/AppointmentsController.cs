using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;
using OnlineAppointment.ViewModels;

namespace OnlineAppointment.Controllers
{
    // Bu controller'a sadece giriş yapmış (authenticated) kullanıcılar erişebilir
    [Authorize]
    public class AppointmentsController : Controller
    {
        // Veritabanı işlemleri için ApplicationDbContext kullanılır
        private readonly ApplicationDbContext _context;
        // Kullanıcı işlemleri için UserManager kullanılır
        private readonly UserManager<ApplicationUser> _userManager;

        // Controller'ın constructor'ı, bağımlılıkları enjekte eder
        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // KULLANICININ RANDEVULARI: Kullanıcının kendi randevularını listeler
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            // Giriş yapan kullanıcının Id'sini al
            var userId = _userManager.GetUserId(User);

            // Kullanıcının randevularını, hizmet bilgisiyle birlikte çek ve tarihe göre sırala
            var myAppointments = await _context.Appointments
                                               .Include(a => a.Service) // Service tablosunu da dahil et
                                               .Where(a => a.CustomerId == userId) // Sadece bu kullanıcıya ait olanlar
                                               .OrderByDescending(a => a.Start) // En yeni randevular en üstte
                                               .ToListAsync();

            ViewBag.FullName = user.FullName; // Kullanıcının tam adını ViewBag'e ekle
            // Listeyi view'a gönder
            return View(myAppointments);
        }

        // RANDEVU OLUŞTURMA FORMU (GET): Yeni randevu oluşturma formunu gösterir
        [HttpGet]
        public async Task<IActionResult> Create(int? serviceId = null)
        {
            var services = await _context.Services.OrderBy(s => s.Name).ToListAsync();

            ViewBag.ServiceList = new SelectList(services, "Id", "Name", serviceId);
            ViewBag.Services = services; // canlı özet JS için

            return View(new AppointmentCreateVM
            {
                ServiceId = serviceId ?? 0,
                Start = DateTime.Now.AddHours(1)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AppointmentCreateVM vm)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            // ViewBag'leri her dönüşte doldur (dropdown + canlı özet için)
            async Task LoadLists()
            {
                var services = await _context.Services.OrderBy(s => s.Name).ToListAsync();
                ViewBag.ServiceList = new SelectList(services, "Id", "Name", vm.ServiceId); // <-- SEÇİLİ DEĞER
                ViewBag.Services = services; // canlı özet için
            }

            await LoadLists();

            // Geçmiş tarih engeli
            if (vm.Start < DateTime.Now.AddMinutes(-1))
            {
                ModelState.AddModelError(nameof(vm.Start), "You cannot book in the past.");
                return View(vm); // <-- View'e dönerken selected korunur
            }

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please check the form fields.";
                return View(vm);
            }

            // ÇALIŞMA SAATİ KONTROLÜ
            var local = vm.Start;
            var startTime = new TimeSpan(9, 0, 0);
            var endTime = new TimeSpan(20, 0, 0);
            if (local.TimeOfDay < startTime || local.TimeOfDay > endTime)
            {
                ModelState.AddModelError(nameof(vm.Start), "Bookings are allowed between 09:00 and 20:00.");
                return View(vm);
            }

            var service = await _context.Services.FindAsync(vm.ServiceId);
            if (service == null)
            {
                ModelState.AddModelError(nameof(vm.ServiceId), "Service not found.");
                TempData["Error"] = "Service not found.";
                return View(vm);
            }

            var start = vm.Start;
            var end = start.AddMinutes(service.DurationMinutes);

            bool overlaps = await _context.Appointments.AnyAsync(a =>
                a.Status != AppointmentStatus.Cancelled &&
                ((start >= a.Start && start < a.End) ||
                 (end > a.Start && end <= a.End) ||
                 (start <= a.Start && end >= a.End))
            );
            if (overlaps)
            {
                ModelState.AddModelError("", "Selected time overlaps with another appointment.");
                return View(vm);
            }

            _context.Appointments.Add(new Appointment
            {
                CustomerId = userId,
                ServiceId = vm.ServiceId,
                Start = start,
                End = end,
                Status = AppointmentStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Your appointment was created.";
            return RedirectToAction(nameof(Index));
        }


        // RANDEVU İPTALİ: Kullanıcı kendi randevusunu iptal edebilir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            // Giriş yapan kullanıcının Id'sini al
            var userId = _userManager.GetUserId(User);

            // Kullanıcının kendi randevusunu bul
            var appt = await _context.Appointments
                                     .FirstOrDefaultAsync(a => a.Id == id && a.CustomerId == userId);
            // Randevu bulunamazsa 404 döndür
            if (appt == null) return NotFound();

            // Randevu zaten iptal edilmemişse, durumunu iptal olarak güncelle
            if (appt.Status != AppointmentStatus.Cancelled)
            {
                appt.Status = AppointmentStatus.Cancelled;
                await _context.SaveChangesAsync();
            }
            // Randevu listesine yönlendir
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Busy(DateTime start, DateTime end)
        {
            var nowUtc = DateTime.UtcNow;

            var items = await _context.Appointments
                .AsNoTracking()
                .Where(a => a.Status != AppointmentStatus.Cancelled
                            // görünür aralıkla çakışsın
                            && a.End > start
                            && a.Start < end
                            // VE süresi bitmiş olmasın
                            && a.End > nowUtc)
                .Select(a => new
                {
                    title = a.Service.Name,
                    start = a.Start,  // ISO'ya serialize olur
                    end = a.End
                })
                .ToListAsync();

            return Json(items);
        }
    }
}
