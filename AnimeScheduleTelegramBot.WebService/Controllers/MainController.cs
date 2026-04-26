using Microsoft.AspNetCore.Mvc;

namespace AnimeScheduleTelegramBot.WebService.Controllers;

[ApiController]
public sealed class MainController : ControllerBase
{
	[HttpGet("/")]
	public IActionResult Index()
	{
		return Content("Hello World!", "text/plain");
	}
}
