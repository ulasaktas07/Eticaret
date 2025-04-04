using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class ContactController : Controller
	{
		private readonly DatabaseContext _context;

		public ContactController(DatabaseContext context)
		{
			_context = context;
		}
		public IActionResult Index()
		{
			return View(_context.Contacts);
		}
	}
}
