using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace WebProgramlamaOdev.Areas.User.Controllers
{
    [Area("User")]
    [Authorize] // Sadece giriş yapmış üyeler erişebilir
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Üyenin Randevularını Listeleme
        // GET: User/Appointment
        public async Task<IActionResult> Index()
        {
            // Mevcut giriş yapan kullanıcının ID'sini al
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Sadece bu üyenin randevularını yükle
            var userAppointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(userAppointments);
        }

        // 2. Randevu Oluşturma Akışı - Adım 1: Hizmet Seçimi
        // GET: User/Appointment/SelectService
        public async Task<IActionResult> SelectService()
        {
            // Aktif hizmetleri listele
            var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            return View(services);
        }

        // 3. Randevu Oluşturma Akışı - Adım 2: Eğitmen ve Saat Seçimi Formu
        // GET: User/Appointment/Create?serviceId=X
        [HttpGet]
        public async Task<IActionResult> Create(int? serviceId)
        {
            if (serviceId == null)
            {
                TempData["ErrorMessage"] = "Lütfen önce bir hizmet seçin.";
                return RedirectToAction(nameof(SelectService));
            }

            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) return NotFound();

            // Bu hizmeti verebilen eğitmenleri bul (Basit filtreleme)
            var availableTrainers = await _context.Trainers
                .Where(t => EF.Functions.Like(t.Specialization, $"%{service.Name}%"))
                .ToListAsync();

            ViewBag.Service = service;
            ViewBag.Trainers = new SelectList(availableTrainers, "TrainerId", "FullName");

            // Randevu oluşturma formu için boş bir model gönder
            return View(new Appointment { ServiceId = serviceId.Value });
        }

        // 4. Randevu Oluşturma İşlemi - Adım 3: Kayıt (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            // Güvenlik: Mevcut kullanıcıyı ata
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            appointment.UserId = userId;

            // Randevu durumunu daima Beklemede (Pending) olarak ayarla
            appointment.Status = AppointmentStatus.Pending;

            // Randevu süresini ve ücretini Service modelinden otomatik doldur
            var service = await _context.Services.FindAsync(appointment.ServiceId);
            if (service != null)
            {
                appointment.TotalPrice = service.Price;
            }

            // Randevu süresinin tutarlılığını kontrol et (Başlangıç < Bitiş olmalı)
            if (appointment.StartTime >= appointment.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
            }

            // Ödev Gereksinimi: Randevu Çakışma Kontrolü
            if (await IsAppointmentConflict(
                appointment.TrainerId,
                appointment.AppointmentDate,
                appointment.StartTime,
                appointment.EndTime))
            {
                // Çakışma varsa kullanıcıyı uyar
                ModelState.AddModelError("", "Seçtiğiniz tarih ve saat aralığı, eğitmenin müsaitliği veya mevcut randevularla çakışmaktadır. Lütfen başka bir zaman deneyin.");
            }

            // Model geçerliyse ve çakışma yoksa kaydet
            if (ModelState.IsValid)
            {
                _context.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevu talebiniz başarıyla alındı ve onay bekleniyor.";
                return RedirectToAction(nameof(Index));
            }

            // Hata durumunda formu tekrar yükle
            ViewBag.Service = service;
            ViewBag.Trainers = new SelectList(_context.Trainers, "TrainerId", "FullName", appointment.TrainerId);

            // Hata durumunda, formdaki güncel değerlerin kaybolmaması için TimeSpans ve DateTime tekrar atanabilir (isteğe bağlı)

            return View(appointment);
        }

        // Yardımcı Metot: Ödev Gereksinimi Olan Çakışma Kontrolü
        private async Task<bool> IsAppointmentConflict(
            int trainerId,
            DateTime appointmentDate,
            TimeSpan startTime,
            TimeSpan endTime)
        {
            // 1. Antrenörün O Gün Müsaitlik Kaydı Var mı? (TrainerAvailability Kontrolü)
            DayOfWeek day = appointmentDate.DayOfWeek;

            var availability = await _context.TrainerAvailabilities
                .FirstOrDefaultAsync(ta =>
                    ta.TrainerId == trainerId &&
                    ta.DayOfWeek == day &&
                    ta.IsActive == true);

            // Müsaitlik kaydı yoksa VEYA seçilen saat aralığı müsaitlik aralığının dışındaysa
            if (availability == null || startTime < availability.StartTime || endTime > availability.EndTime)
            {
                return true; // Çakışma var: Antrenör uygun değil
            }

            // 2. Randevu Çakışması Var mı? (Appointment Kontrolü)
            DateTime dateOnly = appointmentDate.Date;

            // Aynı Eğitmen için, aynı tarihte, onaylanmış veya beklemede olan randevuları kontrol et
            var conflictExists = await _context.Appointments
                .AnyAsync(a =>
                    a.TrainerId == trainerId &&
                    a.AppointmentDate.Date == dateOnly && // Sadece tarih eşleşmeli
                    (a.Status == AppointmentStatus.Approved || a.Status == AppointmentStatus.Pending) && // Onaylanan veya bekleyen randevular

                    // Çakışma Mantığı: Yeni randevu mevcut randevu ile kesişiyorsa
                    !(a.EndTime <= startTime || a.StartTime >= endTime)); // Kesişmeme koşulunun tersi

            return conflictExists; // True ise çakışma var, False ise randevu alınabilir.
        }
    }
}