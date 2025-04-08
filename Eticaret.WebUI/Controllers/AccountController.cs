using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
	public class AccountController : Controller
	{
		private readonly DatabaseContext _context;

		public AccountController(DatabaseContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			return View();
		}
		public IActionResult SignIn()
		{
			return View();
		}
		[HttpPost]
		public IActionResult SignIn(AppUser appUser)
		{
			return View();
		}
		public IActionResult SignUp()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> SignUpAsync(AppUser appUser)
		{
			appUser.IsAdmin = false;
			appUser.IsActive = true;
			if (ModelState.IsValid)
			{
				await _context.AddAsync(appUser);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(appUser);
		}
	}
}
