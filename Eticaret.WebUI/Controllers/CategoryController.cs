using Eticaret.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DatabaseContext _context;

		public CategoryController(DatabaseContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> IndexAsync(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var category = await _context.Categories.Include(c => c.Products)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}
	}
}
