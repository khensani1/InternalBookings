using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;
using System.Linq;

public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookingsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Bookings
    public async Task<IActionResult> Index(int? resourceId, DateTime? date, string bookedBy)
    {
        ViewBag.BookedBy = bookedBy;
        ViewBag.Date = date?.ToString("yyyy-MM-dd");
        var bookings = _context.Bookings.Include(b => b.Resource).AsQueryable();
        if (resourceId.HasValue)
            bookings = bookings.Where(b => b.ResourceId == resourceId.Value);
        if (date.HasValue)
            bookings = bookings.Where(b => b.StartTime.Date == date.Value.Date);
        if (!string.IsNullOrEmpty(bookedBy))
            bookings = bookings.Where(b => b.BookedBy.Contains(bookedBy));
        bookings = bookings.OrderBy(b => b.StartTime);
        ViewData["ResourceId"] = new SelectList(_context.Resources, "Id", "Name", resourceId);

        // Calculate stats from the database (not just filtered list)
        var now = DateTime.Today;
        ViewBag.TotalBookings = await _context.Bookings.CountAsync();
        ViewBag.TodaysBookings = await _context.Bookings.CountAsync(b => b.StartTime.Date == now);
        ViewBag.UpcomingBookings = await _context.Bookings.CountAsync(b => b.StartTime.Date > now);

        return View(await bookings.ToListAsync());
    }

    // GET: Bookings/Create
    public async Task<IActionResult> Create()
    {
        var resources = await _context.Resources
            .Select(r => new { r.Id, Name = r.Name + " (ID: " + r.Id + ")" })
            .ToListAsync();
        ViewData["ResourceId"] = new SelectList(resources, "Id", "Name");
        return View();
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
    {
        var resources = await _context.Resources
            .Select(r => new { r.Id, Name = r.Name + " (ID: " + r.Id + ")" })
            .ToListAsync();
        ViewData["ResourceId"] = new SelectList(resources, "Id", "Name", booking.ResourceId);

        // Debug: Log the posted ResourceId value
        TempData["PostedResourceId"] = $"Posted ResourceId: {booking.ResourceId}";

        if (booking.EndTime <= booking.StartTime)
        {
            ModelState.AddModelError("EndTime", "End time must be after start time.");
        }

        // Conflict detection: ensure no overlap with existing bookings for the same resource
        bool hasConflict = await _context.Bookings.AnyAsync(b =>
            b.ResourceId == booking.ResourceId &&
            b.StartTime < booking.EndTime &&
            booking.StartTime < b.EndTime);
        if (hasConflict)
        {
            ModelState.AddModelError(string.Empty, "This resource is already booked during the requested time. Please choose another slot or resource, or adjust your times.");
        }

        // Debug: Show model state errors in TempData for troubleshooting
        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            TempData["DebugError"] = $"Validation failed: {errors}";
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking created successfully.";
                return RedirectToAction(nameof(Create));
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                ModelState.AddModelError(string.Empty, "An error occurred while saving the booking. Please try again.");
                TempData["DebugError"] = $"Exception: {ex.Message}";
            }
        }
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        ViewData["ResourceId"] = new SelectList(await _context.Resources.ToListAsync(), "Id", "Name", booking.ResourceId);
        return View(booking);
    }

    // POST: Bookings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
    {
        if (id != booking.Id) return NotFound();
        ViewData["ResourceId"] = new SelectList(await _context.Resources.ToListAsync(), "Id", "Name", booking.ResourceId);

        if (booking.EndTime <= booking.StartTime)
        {
            ModelState.AddModelError("EndTime", "End time must be after start time.");
        }

        // Conflict detection (exclude this booking)
        bool hasConflict = await _context.Bookings.AnyAsync(b =>
            b.Id != booking.Id &&
            b.ResourceId == booking.ResourceId &&
            b.StartTime < booking.EndTime &&
            booking.StartTime < b.EndTime);
        if (hasConflict)
        {
            ModelState.AddModelError(string.Empty, "This resource is already booked during the requested time. Please choose another slot or resource, or adjust your times.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // GET: Bookings/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings
            .Include(b => b.Resource)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (booking == null) return NotFound();
        return View(booking);
    }

    // POST: Bookings/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Booking deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Booking not found or already deleted.";
            }
        }
        catch (Exception ex)
        {
            // Log error and show user-friendly message
            TempData["ErrorMessage"] = "An error occurred while deleting the booking. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.Id == id);
    }
} 