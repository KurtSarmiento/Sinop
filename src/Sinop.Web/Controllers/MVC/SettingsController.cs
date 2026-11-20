using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Sinop.Web.Controllers.MVC;

[Area("MVC")]
[Authorize(Roles = "Administrator")]
public class SettingsController : Controller
{
    public IActionResult Index() => View();
}
