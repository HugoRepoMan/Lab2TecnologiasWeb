using Microsoft.AspNetCore.Mvc;
using SakilaWebMVC.Data;
namespace SakilaWebMVC.Controllers;

public class HomeController(SakilaRepository repository) : Controller
{
    public async Task<IActionResult> Index() => View(await repository.GetDashboardAsync());
    public IActionResult Error() => View();
}
