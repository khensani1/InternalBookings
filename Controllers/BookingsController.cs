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
        return View(await bookings.ToListAsync());
    }

    // GET: Bookings/Create
    public async Task<IActionResult> Create()
    {
        ViewData["ResourceId"] = new SelectList(await _context.Resources.Where(r => r.IsAvailable).ToListAsync(), "Id", "Name");
        return View();
    }

    // POST: Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
    {
        ViewData["ResourceId"] = new SelectList(await _context.Resources.Where(r => r.IsAvailable).ToListAsync(), "Id", "Name", booking.ResourceId);

        if (booking.EndTime <= booking.StartTime)
        {
            ModelState.AddModelError("EndTime", "End time must be after start time.");
        }

        // Conflict detection
        bool hasConflict = await _context.Bookings.AnyAsync(b =>
            b.ResourceId == booking.ResourceId &&
            b.StartTime < booking.EndTime &&
            booking.StartTime < b.EndTime);
        if (hasConflict)
        {
            ModelState.AddModelError(string.Empty, "This resource is already booked during the requested time. Please choose another slot or resource, or adjust your times.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(booking);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Booking created successfully.";
            return RedirectToAction(nameof(Index));
        }
        return View(booking);
    }

    // GET: Bookings/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var booking = await _context.Bookings.FindAsync(id);
        if (booking == null) return NotFound();
        ViewData["ResourceId"] = new SelectList(await _context.Resources.Where(r => r.IsAvailable).ToListAsync(), "Id", "Name", booking.ResourceId);
        return View(booking);
    }

    // POST: Bookings/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ResourceId,StartTime,EndTime,BookedBy,Purpose")] Booking booking)
    {
        if (id != booking.Id) return NotFound();
        ViewData["ResourceId"] = new SelectList(await _context.Resources.Where(r => r.IsAvailable).ToListAsync(), "Id", "Name", booking.ResourceId);

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
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        return _context.Bookings.Any(e => e.Id == id);
    }
} 