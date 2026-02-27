using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;

namespace HizliOgren.Areas.Egitmen.Controllers;

[Area("Egitmen")]
[Authorize(Roles = "Egitmen")]
public class SalesController : Controller
{
    private readonly ApplicationDbContext _db;

    public SalesController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var items = await _db.OrderItems.Where(oi => oi.Course!.InstructorId == userId).Include(oi => oi.Order).ThenInclude(o => o!.User).Include(oi => oi.Course).OrderByDescending(oi => oi.Order!.CreatedAt).AsNoTracking().ToListAsync(ct);
        var total = items.Where(oi => oi.Order?.Status == "Paid").Sum(oi => oi.Price);
        ViewBag.Total = total;
        return View(items);
    }
}
