using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HizliOgren.Data;
using HizliOgren.Models;
using HizliOgren.Services;

namespace HizliOgren.Controllers;

public class CheckoutController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly CartService _cart;
    private readonly PayTRService _paytr;

    public CheckoutController(ApplicationDbContext db, CartService cart, PayTRService paytr)
    {
        _db = db;
        _cart = cart;
        _paytr = paytr;
    }

    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var ids = _cart.CourseIds;
        if (ids.Count == 0)
        {
            ViewBag.Total = 0m;
            return View(new List<Course>());
        }
        var courses = await _db.Courses.Include(c => c.Category).Where(c => ids.Contains(c.Id)).AsNoTracking().ToListAsync(ct);
        var total = courses.Sum(c => c.Price);
        ViewBag.Total = total;
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddToCart(int courseId)
    {
        _cart.Add(courseId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int courseId)
    {
        _cart.Remove(courseId);
        return RedirectToAction(nameof(Index));
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(CancellationToken ct = default)
    {
        var ids = _cart.CourseIds;
        if (ids.Count == 0) return RedirectToAction(nameof(Index));
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var user = await _db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
        var courses = await _db.Courses.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
        var total = courses.Sum(c => c.Price);
        if (total <= 0) return RedirectToAction(nameof(Index));

        var order = new Order
        {
            UserId = userId,
            Total = total,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(ct);
        foreach (var c in courses)
            _db.OrderItems.Add(new OrderItem { OrderId = order.Id, CourseId = c.Id, Price = c.Price });
        await _db.SaveChangesAsync(ct);

        var token = await _paytr.GetIframeTokenAsync(order.Id.ToString(), user.Email, user.FullName, total, user.City, ct);
        if (!string.IsNullOrEmpty(token))
        {
            ViewBag.PayTRToken = token;
            ViewBag.OrderId = order.Id;
            ViewBag.Total = total;
            return View("Pay");
        }
        order.Status = "Paid";
        await _db.SaveChangesAsync(ct);
        foreach (var c in courses)
            _db.UserCourses.Add(new UserCourse { UserId = userId, CourseId = c.Id });
        await _db.SaveChangesAsync(ct);
        _cart.Clear();
        TempData["Toast"] = "Ödeme simüle edildi (PayTR ayarları yok). Eğitimleriniz hesabınıza eklendi.";
        return RedirectToAction(nameof(PaySuccess));
    }

    [Authorize]
    public IActionResult PaySuccess()
    {
        _cart.Clear();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Callback(CancellationToken ct = default)
    {
        var merchantOid = Request.Form["merchant_oid"].ToString();
        var status = Request.Form["status"].ToString();
        var totalAmount = Request.Form["total_amount"].ToString();
        var hash = Request.Form["hash"].ToString();
        if (string.IsNullOrEmpty(merchantOid) || !int.TryParse(merchantOid, out var orderId)) return Ok("OK");
        var order = await _db.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId, ct);
        if (order == null) return Ok("OK");
        if (status == "success")
        {
            var ok = await _paytr.VerifyCallbackHashAsync(merchantOid, status, totalAmount, hash, ct);
            if (ok)
            {
                order.Status = "Paid";
                order.PayTRPaymentId = Request.Form["transaction_id"].ToString();
                await _db.SaveChangesAsync(ct);
                foreach (var oi in order.OrderItems)
                    _db.UserCourses.Add(new UserCourse { UserId = order.UserId, CourseId = oi.CourseId });
                await _db.SaveChangesAsync(ct);
            }
        }
        else
        {
            order.Status = "Failed";
            await _db.SaveChangesAsync(ct);
        }
        return Ok("OK");
    }
}
