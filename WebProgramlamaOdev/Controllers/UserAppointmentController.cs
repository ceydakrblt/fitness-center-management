using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using System.Security.Claims;

[Authorize] // Sadece üyeler erişebilir
public class UserAppointmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public UserAppointmentController(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // ---------------------------------
    // USER - 1. ADIM: HİZMET SEÇİMİ (404 HATASINI GİDERİR)
    // URL: /UserAppointment/SelectService
    // ---------------------------------
    public async Task<IActionResult> SelectService()
    {
        // View için aktif tüm hizmetleri getiriyoruz
        var services = await _context.Services.ToListAsync();

        // View adı: Views/UserAppointment/SelectService.cshtml
        return View("SelectService", services);
    }

    // ---------------------------------
    // USER - RANDEVULARIM (LİSTE)
    // URL: /UserAppointment/Index
    // ---------------------------------
    public async Task<IActionResult> Index()
    {
        // Kullanıcının ID'sini al
        var userId = _userManager.GetUserId(User);

        var appointments = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        // View adı: Views/UserAppointment/UserIndex.cshtml
        return View("UserIndex", appointments);
    }

    // ---------------------------------
    // USER - 2. ADIM: RANDEVU OLUŞTURMA FORMU (GET)
    // URL: /UserAppointment/Create?serviceId=3
    // ---------------------------------
    public async Task<IActionResult> Create(int serviceId)
    {

        // Seçilen hizmeti bul
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        if (service == null)
            return NotFound();

        ViewBag.Service = service;

        // Eğitmenleri dropdown için yükle
        ViewBag.Trainers = new SelectList(
            _context.Trainers,
            "TrainerId",
            "FullName"
        );

        // View adı: Views/UserAppointment/UserCreate.cshtml
        return View("UserCreate", new Appointment
        {
            ServiceId = serviceId,
            // Başlangıç tarihi olarak yarını varsayabiliriz
            AppointmentDate = DateTime.Today.AddDays(1)
        });
    }
    // UserAppointmentController.cs - POST Create metodu (NİHAİ DÜZELTME)

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {

        // Modelin zorunlu alanlarını doldur (UserId, Status, CreatedAt)
        appointment.UserId = _userManager.GetUserId(User)!;
        appointment.Status = AppointmentStatus.Pending;
        appointment.CreatedAt = DateTime.Now;

        // Eğitmen ve Hizmet verilerini çek
        var trainer = await _context.Trainers.FindAsync(appointment.TrainerId);
        var service = await _context.Services.FindAsync(appointment.ServiceId);

        if (trainer != null)
        {
            // 🚨 POTANSİYEL HATA GİDERİCİ: GymId ataması (DB'de zorunluysa)
            // Eğer Appointment tablosunda zorunlu GymId varsa, buradan atıyoruz.
            // Appointment modelinizde bu alanı oluşturmadıysanız, bunu atlayın. 
            // Ancak bu, şema zorunluluğunu aşmanın tek yoludur.
            // Eğer bu satır hata verirse (GymId Appointment modelinde yoksa), yoruma alın.
            // appointment.GymId = trainer.GymId; 
        }

        if (service != null)
        {
            // TotalPrice ataması (ModelState.IsValid kontrolünden ÖNCE zorunlu)
            appointment.TotalPrice = service.Price;
        }
        else
        {
            ModelState.AddModelError("", "Seçilen hizmet bulunamadı.");
        }

        // Şimdi, formdan gelmeyen, ancak kodla doldurulan alanlarla modelin geçerliliğini kontrol et.
        if (ModelState.IsValid)
        {
            // EK KONTROL 1: Randevu tarihinin geçmişte olup olmadığını kontrol et
            if (appointment.AppointmentDate.Date < DateTime.Today.Date)
            {
                ModelState.AddModelError("AppointmentDate", "Randevu tarihi geçmiş bir tarih olamaz.");
            }
            // EK KONTROL 2: Başlangıç ve Bitiş saati kontrolü
            else if (appointment.StartTime >= appointment.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
            }
            // Randevu/Müsaitlik Çakışma Kontrolü (İLERİDE EKLENECEK)

            else
            {
                // Kayıt işlemi
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevunuz oluşturuldu ve onay bekliyor.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Hata varsa, View'a geri dönerken ViewBag'leri yeniden doldur
        ViewBag.Service = service;

        ViewBag.Trainers = new SelectList(
            _context.Trainers,
            "TrainerId",
            "FullName",
            appointment.TrainerId
        );

        // View adı: Views/UserAppointment/UserCreate.cshtml
        return View("UserCreate", appointment);
    }
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a =>
                a.AppointmentId == id &&
                a.UserId == userId);

        if (appointment == null)
            return NotFound();

        return View("UserDetails", appointment);
    }

    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a =>
                a.AppointmentId == id &&
                a.UserId == userId);

        if (appointment == null)
            return NotFound();

        return View("UserDelete", appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var userId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a =>
                a.AppointmentId == id &&
                a.UserId == userId);

        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}