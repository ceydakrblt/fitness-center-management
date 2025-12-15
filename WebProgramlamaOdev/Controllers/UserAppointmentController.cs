using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;
using System.Security.Claims;

[Authorize]
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
    // 🆕 API: Eğitmenin Müsaitlik Saatlerini Getir
    // URL: /UserAppointment/GetTrainerAvailability?trainerId=1
    // ---------------------------------
    [HttpGet]
    public async Task<IActionResult> GetTrainerAvailability(int trainerId)
    {
        var availabilities = await _context.TrainerAvailabilities
            .Where(ta => ta.TrainerId == trainerId && ta.IsActive)
            .OrderBy(ta => ta.DayOfWeek)
            .Select(ta => new
            {
                dayOfWeek = (int)ta.DayOfWeek,
                startTime = ta.StartTime.ToString(@"hh\:mm"),
                endTime = ta.EndTime.ToString(@"hh\:mm"),
                isActive = ta.IsActive
            })
            .ToListAsync();

        return Json(availabilities);
    }

    // ---------------------------------
    // USER - 1. ADIM: HİZMET SEÇİMİ
    // ---------------------------------
    public async Task<IActionResult> SelectService()
    {
        var services = await _context.Services.ToListAsync();
        return View("SelectService", services);
    }

    // ---------------------------------
    // USER - RANDEVULARIM (LİSTE)
    // ---------------------------------
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        var appointments = await _context.Appointments
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ToListAsync();

        return View("UserIndex", appointments);
    }

    // ---------------------------------
    // USER - 2. ADIM: RANDEVU OLUŞTURMA FORMU (GET)
    // ---------------------------------
    public async Task<IActionResult> Create(int serviceId)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.ServiceId == serviceId);

        if (service == null)
            return NotFound();

        ViewBag.Service = service;
        ViewBag.Trainers = new SelectList(
            _context.Trainers,
            "TrainerId",
            "FullName"
        );

        return View("UserCreate", new Appointment
        {
            ServiceId = serviceId,
            AppointmentDate = DateTime.Today.AddDays(1)
        });
    }

    // ---------------------------------
    // USER - RANDEVU OLUŞTURMA (POST)
    // ---------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        appointment.UserId = _userManager.GetUserId(User)!;
        appointment.Status = AppointmentStatus.Pending;
        appointment.CreatedAt = DateTime.Now;

        // ModelState temizleme
        ModelState.Remove("UserId");
        ModelState.Remove("CreatedAt");
        ModelState.Remove("Status");
        ModelState.Remove("User");
        ModelState.Remove("Trainer");
        ModelState.Remove("Service");
        ModelState.Remove("TotalPrice");

        var trainer = await _context.Trainers.FindAsync(appointment.TrainerId);
        var service = await _context.Services.FindAsync(appointment.ServiceId);

        if (service != null)
        {
            appointment.TotalPrice = service.Price;
        }
        else
        {
            ModelState.AddModelError("", "Seçilen hizmet bulunamadı.");
        }

        if (ModelState.IsValid)
        {
            if (appointment.AppointmentDate.Date < DateTime.Today.Date)
            {
                ModelState.AddModelError("AppointmentDate", "Randevu tarihi geçmiş bir tarih olamaz.");
            }
            else if (appointment.StartTime >= appointment.EndTime)
            {
                ModelState.AddModelError("EndTime", "Bitiş Saati, Başlangıç Saatinden sonra olmalıdır.");
            }
            else
            {
                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Randevunuz oluşturuldu ve onay bekliyor.";
                return RedirectToAction(nameof(Index));
            }
        }

        ViewBag.Service = service;
        ViewBag.Trainers = new SelectList(
            _context.Trainers,
            "TrainerId",
            "FullName",
            appointment.TrainerId
        );

        return View("UserCreate", appointment);
    }

    // ---------------------------------
    // USER - RANDEVU DETAYLARI
    // ---------------------------------
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

    // ---------------------------------
    // USER - RANDEVU SİLME
    // ---------------------------------
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
            TempData["SuccessMessage"] = "Randevunuz iptal edildi.";
        }

        return RedirectToAction(nameof(Index));
    }
}