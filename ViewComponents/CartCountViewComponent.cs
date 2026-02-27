using HizliOgren.Services;
using Microsoft.AspNetCore.Mvc;

namespace HizliOgren.ViewComponents;

public class CartCountViewComponent : ViewComponent
{
    private readonly CartService _cart;

    public CartCountViewComponent(CartService cart) => _cart = cart;

    public IViewComponentResult Invoke()
    {
        return View(_cart.Count);
    }
}
