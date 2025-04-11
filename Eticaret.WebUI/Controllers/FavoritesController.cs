using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.ExtensionMethods;
using Microsoft.AspNetCore.Mvc;

namespace Eticaret.WebUI.Controllers
{
	public class FavoritesController : Controller
	{
		private readonly IService<Product> _service;

		public FavoritesController(IService<Product> service)
		{
			_service = service;
		}
		public IActionResult Index()
		{
			var favorites = GetFavorites();
			return View(favorites);
		}
		private List<Product> GetFavorites()
		{
			return HttpContext.Session.GetJson<List<Product>>("GetFavorites") ?? [];
		}
		public IActionResult Add(int ProductId)
		{
			var favorites = GetFavorites();
			var product = _service.Find(ProductId);
			if (product != null && !favorites.Any(p=>p.Id==ProductId))
			{
				favorites.Add(product);
				HttpContext.Session.SetJson("GetFavorites", favorites);
			}
			return RedirectToAction("Index");
		}
		public IActionResult Remove(int ProductId)
		{
			var favorites = GetFavorites();
			var product = _service.Find(ProductId);
			if (product != null && favorites.Any(p=>p.Id==ProductId))
			{
				favorites.RemoveAll(i=>i.Id==product.Id);
				HttpContext.Session.SetJson("GetFavorites", favorites);
			}
			return RedirectToAction("Index");
		}
	}
}
