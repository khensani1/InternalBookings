using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InternalResourceBooking.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace InternalResourceBooking.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public async Task<IActionResult> Dashboard()
    {
        var today = DateTime.Today;
        var nextWeek = today.AddDays(7);
        var todaysBookings = await _context.Bookings
            .Include(b => b.Resource)
            .Where(b => b.StartTime.Date == today)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
        var upcomingBookings = await _context.Bookings
            .Include(b => b.Resource)
            .Where(b => b.StartTime.Date > today && b.StartTime.Date <= nextWeek)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
        ViewBag.TodaysBookings = todaysBookings;
        ViewBag.UpcomingBookings = upcomingBookings;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
