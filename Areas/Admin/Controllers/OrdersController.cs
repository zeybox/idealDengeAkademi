using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class OrdersController : Controller
{
    private readonly ApplicationDbContext _db;

    public OrdersController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? status, CancellationToken ct = default)
    {
        var query = _db.Orders.Include(o => o.User).Include(o => o.OrderItems).ThenInclude(oi => oi.Course).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(status)) query = query.Where(o => o.Status == status);
        var list = await query.OrderByDescending(o => o.CreatedAt).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }
}
