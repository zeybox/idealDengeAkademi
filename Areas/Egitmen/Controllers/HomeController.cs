using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;

namespace HizliOgren.Areas.Egitmen.Controllers;

[Area("Egitmen")]
[Authorize(Roles = "Egitmen")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var courseCount = await _db.Courses.CountAsync(c => c.InstructorId == userId, ct);
        var salesCount = await _db.OrderItems.Where(oi => oi.Course!.InstructorId == userId).CountAsync(ct);
        var totalRevenue = await _db.OrderItems.Where(oi => oi.Course!.InstructorId == userId).Join(_db.Orders.Where(o => o.Status == "Paid"), oi => oi.OrderId, o => o.Id, (oi, o) => oi.Price).SumAsync(ct);
        ViewBag.CourseCount = courseCount;
        ViewBag.SalesCount = salesCount;
        ViewBag.TotalRevenue = totalRevenue;
        var recentSales = await _db.OrderItems.Where(oi => oi.Course!.InstructorId == userId).Include(oi => oi.Order).ThenInclude(o => o!.User).Include(oi => oi.Course).OrderByDescending(oi => oi.Order!.CreatedAt).Take(10).AsNoTracking().ToListAsync(ct);
        ViewBag.RecentSales = recentSales;
        return View();
    }
}
