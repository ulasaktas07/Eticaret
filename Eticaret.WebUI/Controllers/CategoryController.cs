using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
	public class CategoryController : Controller
	{
		private readonly IService<Category> _service;

		public CategoryController(IService<Category> service)
		{
			_service = service;
		}

		public async Task<IActionResult> IndexAsync(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var category = await _service.GetQueryable().Include(c => c.Products)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}
	}
}
