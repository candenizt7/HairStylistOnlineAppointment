using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;

namespace OnlineAppointment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // En popüler / öne çýkan 6 hizmet (þimdilik ada göre)
            var services = await _context.Services
                                         .OrderBy(s => s.Name)
                                         .Take(6)
                                         .ToListAsync();

            // Kullanýcý giriþ yaptýysa son randevusunu da çek
            Appointment? lastAppt = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                lastAppt = await _context.Appointments
                                         .Include(a => a.Service)
                                         .Where(a => a.CustomerId == userId)
                                         .OrderByDescending(a => a.Start)
                                         .FirstOrDefaultAsync();
            }

            var vm = new HomeIndexViewModel
            {
                Services = services,
                LastAppointment = lastAppt
            };

            return View(vm);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            ViewBag.Path = feature?.Path;
            ViewBag.Message = feature?.Error?.Message;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult StatusCode(int code)
        {
            ViewBag.Code = code;
            return View();
        }
    }

    // Basit ViewModel
    public class HomeIndexViewModel
    {
        public List<Service> Services { get; set; } = new();
        public Appointment? LastAppointment { get; set; }
    }
}
