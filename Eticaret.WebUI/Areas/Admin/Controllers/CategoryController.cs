using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eticaret.Core.Entities;
using Eticaret.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Eticaret.WebUI.Utils;

namespace Eticaret.WebUI.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class CategoryController : Controller
	{
		private readonly DatabaseContext _context;

		public CategoryController(DatabaseContext context)
		{
			_context = context;
		}

		// GET: Admin/Category
		public async Task<IActionResult> Index()
		{
			return View(await _context.Categories.ToListAsync());
		}

		// GET: Admin/Category/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var category = await _context.Categories
				.FirstOrDefaultAsync(m => m.Id == id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		// GET: Admin/Category/Create
		public IActionResult Create()
		{
			ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
			return View();
		}

		// POST: Admin/Category/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Category category, IFormFile? Image)
		{
			if (ModelState.IsValid)
			{
				category.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Categories/");
				await _context.AddAsync(category);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
			return View(category);
		}

		// GET: Admin/Category/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var category = await _context.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
			return View(category);
		}

		// POST: Admin/Category/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, Category category, IFormFile? Image, bool cbResmiSil = false)
		{
			if (id != category.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{

					if (cbResmiSil)
						category.Image = string.Empty;
					if (Image is not null)
						category.Image = await FileHelper.FileLoaderAsync(Image, "/Img/Categories/");
					_context.Update(category);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!CategoryExists(category.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewBag.Kategoriler = new SelectList(_context.Categories, "Id", "Name");
			return View(category);
		}

		// GET: Admin/Category/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var category = await _context.Categories
				.FirstOrDefaultAsync(m => m.Id == id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		// POST: Admin/Category/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category != null)
			{
				if (!string.IsNullOrEmpty(category.Image))
				{
					FileHelper.FileRemover(category.Image, "/Img/Categories/");
				}
				_context.Categories.Remove(category);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool CategoryExists(int id)
		{
			return _context.Categories.Any(e => e.Id == id);
		}
	}
}
