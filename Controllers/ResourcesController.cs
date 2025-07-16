using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System; // Added for Exception
using System.Linq; // Added for .Any()

public class ResourcesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ResourcesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Resources
    public async Task<IActionResult> Index(string searchName, string searchLocation, bool? searchAvailable)
    {
        ViewBag.SearchName = searchName;
        ViewBag.SearchLocation = searchLocation;
        ViewBag.SearchAvailable = searchAvailable;
        var resources = _context.Resources.AsQueryable();
        if (!string.IsNullOrEmpty(searchName))
            resources = resources.Where(r => r.Name.Contains(searchName));
        if (!string.IsNullOrEmpty(searchLocation))
            resources = resources.Where(r => r.Location.Contains(searchLocation));
        if (searchAvailable.HasValue)
            resources = resources.Where(r => r.IsAvailable == searchAvailable.Value);
        return View(await resources.ToListAsync());
    }

    // GET: Resources/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var resource = await _context.Resources
            .Include(r => r.Bookings.OrderBy(b => b.StartTime))
            .FirstOrDefaultAsync(m => m.Id == id);
        if (resource == null) return NotFound();
        return View(resource);
    }

    // GET: Resources/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Resources/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description,Location,Capacity,IsAvailable")] Resource resource)
    {
        if (ModelState.IsValid)
        {
            try
            {
                _context.Add(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Resource created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log error and show user-friendly message
                ModelState.AddModelError(string.Empty, "An error occurred while saving the resource. Please try again.");
            }
        }
        return View(resource);
    }

    // GET: Resources/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var resource = await _context.Resources.FindAsync(id);
        if (resource == null) return NotFound();
        return View(resource);
    }

    // POST: Resources/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Location,Capacity,IsAvailable")] Resource resource)
    {
        if (id != resource.Id) return NotFound();
        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Resource updated successfully.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ResourceExists(resource.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(resource);
    }

    // GET: Resources/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var resource = await _context.Resources.FirstOrDefaultAsync(m => m.Id == id);
        if (resource == null) return NotFound();
        return View(resource);
    }

    // POST: Resources/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var resource = await _context.Resources.FindAsync(id);
            if (resource != null)
            {
                _context.Resources.Remove(resource);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Resource deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Resource not found or already deleted.";
            }
        }
        catch (Exception ex)
        {
            // Log error and show user-friendly message
            TempData["ErrorMessage"] = "An error occurred while deleting the resource. Please try again.";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool ResourceExists(int id)
    {
        return _context.Resources.Any(e => e.Id == id);
    }
} 