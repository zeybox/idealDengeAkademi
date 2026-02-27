using Microsoft.AspNetCore.Mvc;
using HizliOgren.Data;
using HizliOgren.Models;
using Microsoft.EntityFrameworkCore;

namespace HizliOgren.ViewComponents;

public class CategoryMenuViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _db;

    public CategoryMenuViewComponent(ApplicationDbContext db) => _db = db;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken ct = default)
    {
        var list = await _db.Categories.OrderBy(c => c.SortOrder)
            .Select(c => new CategoryMenuDto { Name = c.Name, Slug = c.Slug }).ToListAsync(ct);
        return View(list);
    }
}
