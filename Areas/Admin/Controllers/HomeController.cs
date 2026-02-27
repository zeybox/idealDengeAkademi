using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class HomeController : Controller
{
    private readonly ApplicationDbContext _db;

    public HomeController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var coursesCount = await _db.Courses.CountAsync(ct);
        var usersCount = await _db.Users.CountAsync(ct);
        var ordersCount = await _db.Orders.Where(o => o.Status == "Paid").CountAsync(ct);
        var totalSales = await _db.Orders.Where(o => o.Status == "Paid").SumAsync(o => o.Total, ct);
        var blogCount = await _db.BlogPosts.CountAsync(ct);
        var recentOrders = await _db.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems).ThenInclude(oi => oi.Course)
            .OrderByDescending(o => o.CreatedAt)
            .Take(10)
            .AsNoTracking()
            .ToListAsync(ct);

        ViewBag.CoursesCount = coursesCount;
        ViewBag.UsersCount = usersCount;
        ViewBag.TotalSales = totalSales;
        ViewBag.BlogCount = blogCount;
        ViewBag.RecentOrders = recentOrders;
        return View();
    }
}
