﻿using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Eticaret.WebUI.Controllers
{
	public class ProductController : Controller
	{
		private readonly IService<Product> _serviceProduct;

		public ProductController(IService<Product> serviceProduct)
		{
			_serviceProduct = serviceProduct;
		}

		public async Task<IActionResult> Index(string q = "")
		{
			var databaseContext = _serviceProduct.GetQueryable().Where(p => p.IsActive && p.Name.Contains(q) || p.Description.Contains(q))
				.Include(p => p.Brand).Include(p => p.Category);
			return View(await databaseContext.ToListAsync());
		}
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _serviceProduct.GetQueryable()
				.Include(p => p.Brand)
				.Include(p => p.Category)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (product == null)
			{
				return NotFound();
			}
			var model = new ProductDetailViewModel()
			{
				Product = product,
				RelatedProducts = _serviceProduct.GetQueryable()
					.Where(p => p.IsActive && p.CategoryId == product.CategoryId && p.Id != product.Id)
			};
			return View(model);
		}

	}
}
