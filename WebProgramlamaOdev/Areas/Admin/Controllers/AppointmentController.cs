using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;
using System;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class AppointmentController : Controller
{
    private readonly ApplicationDbContext _context;
    // IdentityUser'ın detaylarını almak için gerekli
    private readonly UserManager<IdentityUser> _userManager;

    public AppointmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Admin/Appointment (LIST)
    public async Task<IActionResult> Index(AppointmentStatus? filterStatus)
    {
        // Gerekli tüm ilişkileri yüklüyoruz: User, Eğitmen ve Hizmet
        var query = _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .AsQueryable();

        // Varsayılan filtre: Onay Bekleyenler (Pending)
        if (!filterStatus.HasValue)
        {
            query = query.Where(a => a.Status == AppointmentStatus.Pending);
        }
        else
        {
            query = query.Where(a => a.Status == filterStatus.Value);
        }

        var appointments = await query.OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime).ToListAsync();

        ViewBag.FilterStatus = filterStatus;
        return View(appointments);
    }

    // GET: Admin/Appointment/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(m => m.AppointmentId == id);

        if (appointment == null) return NotFound();

        return View(appointment);
    }

    // GET: Admin/Appointment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();

        // Eğitmen, Hizmet ve Durum listelerini View'a gönder
        ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
        ViewBag.ServiceId = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
        // Enum listesini, seçili durumla birlikte View'a gönder
        ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(AppointmentStatus)), appointment.Status);

        return View(appointment);
    }

    // POST: Admin/Appointment/Edit/5 (Güncelleme ve Durum Değişikliği)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id,
        [Bind("AppointmentId,UserId,TrainerId,ServiceId,AppointmentDate,StartTime,EndTime,Status,Notes,TotalPrice,CreatedAt")] Appointment appointment)
    {
        if (id != appointment.AppointmentId) return NotFound();

        // Navigation property'leri ve zaman damgalarını Model doğrulamasından çıkar
        ModelState.Remove("User");

        if (ModelState.IsValid)
        {
            // Randevu süresinin tutarlılığını kontrol et
            if (appointment.StartTime >= appointment.EndTime)
            {
                // Sorunu Controller'da çözdüğümüz için, burada özel bir hata ekleyelim
                ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
            }
            else
            {
                try
                {
                    _context.Update(appointment);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Randevu #{id} güncellendi. Yeni Durum: {appointment.Status.ToString()}";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Appointments.Any(e => e.AppointmentId == id)) return NotFound();
                    else throw;
                }
            }
        }

        // Hata durumunda SelectList'leri tekrar yükle
        ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);
        ViewBag.ServiceId = new SelectList(_context.Services, "ServiceId", "Name", appointment.ServiceId);
        ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(AppointmentStatus)), appointment.Status);

        return View(appointment);
    }

    // GET: Admin/Appointment/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(m => m.AppointmentId == id);

        if (appointment == null) return NotFound();

        return View(appointment);
    }

    // POST: Admin/Appointment/Delete/5 (Randevuyu veritabanından siler)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);

        if (appointment != null)
        {
            // İstenirse, silmek yerine durumu Cancelled olarak da işaretleyebiliriz.
            // Örnek: appointment.Status = AppointmentStatus.Cancelled; _context.Update(appointment);
            _context.Appointments.Remove(appointment);
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Randevu #{id} tamamen silindi.";
        return RedirectToAction(nameof(Index));
    }
}