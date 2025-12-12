using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebProgramlamaOdev.Areas.Identity.Data;
using WebProgramlamaOdev.Models;

[Authorize(Roles = "Admin")]
public class AdminAppointmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public AdminAppointmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // ADMIN - TÜM RANDEVULAR
    public async Task<IActionResult> Index(AppointmentStatus? filterStatus)
    {
        var query = _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .AsQueryable();

        if (filterStatus.HasValue)
        {
            query = query.Where(a => a.Status == filterStatus);
        }

        ViewBag.FilterStatus = filterStatus;
        var appointments = await query
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

        return View("AdminIndex", appointments);
    }

    // ADMIN - CREATE (GET)
    public IActionResult Create()
    {
        LoadDropdowns();
        return View("AdminCreate");
    }

    // ADMIN - CREATE (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View("AdminCreate", appointment);
        }

        appointment.CreatedAt = DateTime.Now;
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ADMIN - EDIT (GET)
    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            return NotFound();

        LoadDropdowns();
        return View("Edit", appointment);
    }

    // ADMIN - EDIT (POST)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.AppointmentId)
            return NotFound();

        if (!ModelState.IsValid)
        {
            LoadDropdowns();
            return View("Edit", appointment);
        }

        _context.Update(appointment);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // ADMIN - DELETE
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.User)
            .Include(a => a.Trainer)
            .Include(a => a.Service)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);

        if (appointment == null)
            return NotFound();

        return View("Delete", appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    // ------------------------
    private void LoadDropdowns()
    {
        ViewBag.UserId = new SelectList(_context.Users, "Id", "UserName");
        ViewBag.TrainerId = new SelectList(_context.Trainers, "TrainerId", "FullName");
        ViewBag.ServiceId = new SelectList(_context.Services, "ServiceId", "Name");
        ViewBag.StatusList = new SelectList(Enum.GetValues(typeof(AppointmentStatus)));
    }
}
