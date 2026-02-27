using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;

namespace HizliOgren.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class InstructorApplicationsController : Controller
{
    private readonly ApplicationDbContext _db;

    public InstructorApplicationsController(ApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? status, CancellationToken ct = default)
    {
        IQueryable<InstructorApplication> query = _db.InstructorApplications
            .AsNoTracking()
            .Include(ia => ia.User);

        if (!string.IsNullOrWhiteSpace(status) && int.TryParse(status, out var s))
            query = query.Where(ia => (int)ia.Status == s);

        var list = await query.OrderByDescending(ia => ia.CreatedAt).ToListAsync(ct);
        ViewBag.Status = status;
        return View(list);
    }

    public async Task<IActionResult> Detail(int id, CancellationToken ct = default)
    {
        var app = await _db.InstructorApplications
            .AsNoTracking()
            .Include(ia => ia.User)
            .FirstOrDefaultAsync(ia => ia.Id == id, ct);
        if (app == null) return NotFound();
        return View(app);
    }
}
