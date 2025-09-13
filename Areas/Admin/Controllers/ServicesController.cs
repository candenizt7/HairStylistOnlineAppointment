using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineAppointment.Data;
using OnlineAppointment.Models;

namespace OnlineAppointment.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ServicesController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Services
        public async Task<IActionResult> Index()
        {
            // Listeleme: en çok kullanılan senaryo -> AsNoTracking performans için iyi
            var items = await _db.Services
                                 .AsNoTracking()
                                 .OrderBy(s => s.Name)
                                 .ToListAsync();

            return View(items);
        }

        // GET
        public IActionResult Create()
        {
            return View(new Service());
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            // 1) Model doğrulama
            if (!ModelState.IsValid)
                return View(model);

            // 2) İsim benzersiz mi? (Unique index olsa bile, kullanıcıya güzel mesaj vermek için önden kontrol)
            bool nameExists = await _db.Services
                .AnyAsync(s => s.Name == model.Name);

            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.Name), "This service name already exists.");
                return View(model);
            }

            // 3) Kaydet
            _db.Services.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Service has been created.";
            return RedirectToAction(nameof(Index));
        }
        // GET: /Admin/Services/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var service = await _db.Services.FindAsync(id.Value);
            if (service == null) return NotFound();

            return View(service);
        }

        // POST: /Admin/Services/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Service model)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
                return View(model);

            // Aynı isim var mı? (kendisi hariç)
            bool nameExists = await _db.Services
                .AnyAsync(s => s.Id != model.Id && s.Name == model.Name);
            if (nameExists)
            {
                ModelState.AddModelError(nameof(model.Name), "This service name already exists.");
                return View(model);
            }

            try
            {
                // Attach & update pattern
                _db.Attach(model);
                _db.Entry(model).Property(s => s.Name).IsModified = true;
                _db.Entry(model).Property(s => s.DurationMinutes).IsModified = true;
                _db.Entry(model).Property(s => s.Price).IsModified = true;

                await _db.SaveChangesAsync();
                TempData["Success"] = "Service has been updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                // satır yoksa
                if (!await _db.Services.AnyAsync(s => s.Id == id))
                    return NotFound();
                throw; // başka bir concurrency çatışması ise fırlat
            }
        }

        // GET: /Admin/Services/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var service = await _db.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id.Value);
            if (service == null) return NotFound();

            return View(service); // basit bir "emin misin?" sayfası
        }

        // POST: /Admin/Services/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _db.Services.FindAsync(id);
            if (service == null) return NotFound();

            _db.Services.Remove(service);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Service has been deleted.";
            return RedirectToAction(nameof(Index));
        }

    }
}
