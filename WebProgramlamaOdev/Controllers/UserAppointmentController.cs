using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;

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
    // USER - RANDEVULARIM
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
    // USER - CREATE (GET)
    // URL: /UserAppointment/Create?serviceId=3
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
            ServiceId = serviceId
        });
    }

    // ---------------------------------
    // USER - CREATE (POST)
    // ---------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            // 🔴 View tekrar render edilecekse bunlar ZORUNLU
            ViewBag.Service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == appointment.ServiceId);

            ViewBag.Trainers = new SelectList(
                _context.Trainers,
                "TrainerId",
                "FullName"
            );

            return View("UserCreate", appointment);
        }

        appointment.UserId = _userManager.GetUserId(User)!;
        appointment.Status = AppointmentStatus.Pending;
        appointment.CreatedAt = DateTime.Now;

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ---------------------------------
    // USER - DETAILS
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
    // USER - DELETE
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
        }

        return RedirectToAction(nameof(Index));
    }
}
