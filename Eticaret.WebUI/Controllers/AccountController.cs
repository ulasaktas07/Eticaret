﻿using Eticaret.Core.Entities;
using Eticaret.Service.Abstract;
using Eticaret.WebUI.Models;
using Eticaret.WebUI.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Eticaret.WebUI.Controllers
{
	public class AccountController : Controller
	{
		private readonly IService<AppUser> _service;
		private readonly IService<Order> _serviceOrder;

		public AccountController(IService<AppUser> service, IService<Order> serviceOrder)
		{
			_service = service;
			_serviceOrder = serviceOrder;
		}

		[Authorize]
		public async Task<IActionResult> Index()
		{
			AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
			if (user is null)
			{
				return NotFound();
			}
			var model = new UserEditViewModel
			{
				Id = user.Id,
				Name = user.Name,
				Surname = user.Surname,
				Email = user.Email,
				Password = user.Password,
				Phone = user.Phone,
				UserName = user.UserName
			};
			return View(model);
		}
		[HttpPost, Authorize]
		public async Task<IActionResult> IndexAsync(UserEditViewModel model)
		{
			if (ModelState.IsValid)
			{
				try
				{
					AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);

					if (user is not null)
					{
						user.Surname = model.Surname;
						user.Name = model.Name;
						user.Email = model.Email;
						user.Password = model.Password;
						user.Phone = model.Phone;
						user.UserName = model.UserName;
						_service.Update(user);
						var sonuc = _service.SaveChanges();
						if (sonuc > 0)
						{
							TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                             <strong>Bilgileriniz Güncellenmiştir!</strong> 
                            <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                            </div>";
							// await MailHelper.SendMailAsync(contact); 
							return RedirectToAction("Index");
						}
					}

				}
				catch (Exception)
				{

					ModelState.AddModelError("", "Hata Oluştu!");
				}
			}
			return View(model);
		}
		[Authorize]
		public async Task<IActionResult> MyOrders()
		{
			AppUser user = await _service.GetAsync(x => x.UserGuid.ToString() == HttpContext.User.FindFirst("UserGuid").Value);
			if (user is null)
			{
				await HttpContext.SignOutAsync();
				return RedirectToAction("SignIn");
			}
			var model = _serviceOrder.GetQueryable().Where(x => x.AppUserId == user.Id).Include(o => o.OrderLines).ThenInclude(p => p.Product);
			return View(model);
		}
		public IActionResult SignIn()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> SignIn(LoginViewModel loginViewModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var account = await _service.GetAsync(x => x.Email == loginViewModel.Email &
					x.Password == loginViewModel.Password & x.IsActive);
					if (account == null)
					{
						ModelState.AddModelError("", "Giriş Başarısız!");
					}
					else
					{
						var claims = new List<Claim>()
						{
							new(ClaimTypes.Name,account.Name),
							new(ClaimTypes.Role,account.IsAdmin ? "Admin":"Customer"),
							new(ClaimTypes.Email,account.Email),
							new("UserId",account.Id.ToString()),
							new("UserGuid",account.UserGuid.ToString())
						};
						var userIdentity = new ClaimsIdentity(claims, "Login");
						ClaimsPrincipal userPrincipal = new ClaimsPrincipal(userIdentity);
						await HttpContext.SignInAsync(userPrincipal);
						return Redirect(string.IsNullOrEmpty(loginViewModel.ReturnUrl) ? "/" : loginViewModel.ReturnUrl);
					}

				}
				catch (Exception)
				{

					ModelState.AddModelError("", "Hata Oluştu!");
				}
			}
			return View(loginViewModel);
		}
		public IActionResult SignUp()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> SignUp(AppUser appUser)
		{
			appUser.IsAdmin = false;
			appUser.IsActive = true;
			if (ModelState.IsValid)
			{
				await _service.AddAsync(appUser);
				await _service.SaveChangesAsync();
				return RedirectToAction("Index");
			}
			return View(appUser);
		}
		public async Task<IActionResult> SignOutAsync()
		{
			await HttpContext.SignOutAsync();
			return RedirectToAction("SignIn");
		}
		public IActionResult PasswordRenew()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> PasswordRenewAsync(string Email)
		{
			if (string.IsNullOrWhiteSpace(Email))
			{
				ModelState.AddModelError("", "Email Adresi Boş Geçilemez!");
				return View();

			}
			AppUser user = await _service.GetAsync(x => x.Email == Email);
			if (user is null)
			{
				ModelState.AddModelError("", "Girdiğiniz Email Bulunamadı!");
				return View();
			}
			string mesaj = $"Sayın {user.Name} {user.Surname} <br> Şifrenizi Yenilemek İçin Lütfen" +
				$" <a href='https://localhost:44341/Account/PasswordChange?user={user.UserGuid.ToString()}" +
				$"'>Buraya Tıklayınız</a>	";
			var sonuc = await MailHelper.SendMailAsync(Email, "Şifremi Yenile", mesaj);
			if (sonuc)
			{
				TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                             <strong>Şifre Sıfırlama Bağlantınız Gönderilmiştir!</strong> 
                            <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                            </div>";
			}
			else
			{
				TempData["Message"] = @"<div class=""alert alert-danger alert-dismissible fade show"" role=""alert"">
                             <strong>Şifre Sıfırlama Bağlantınız Gönderilemedi!</strong> 
                            <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                            </div>";
			}
			return View();
		}
		public async Task<IActionResult> PasswordChangeAsync(string user)
		{
			if (user is null)
			{
				return BadRequest("Geçersiz İstek!");
			}
			AppUser appUser = await _service.GetAsync(x => x.UserGuid.ToString() == user);
			if (appUser is null)
			{
				return NotFound("Geçersiz Değer!");
			}
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> PasswordChange(string user, string Password)
		{
			if (user is null)
			{
				return BadRequest("Geçersiz İstek!");
			}
			AppUser appUser = await _service.GetAsync(x => x.UserGuid.ToString() == user);
			if (appUser is null)
			{
				ModelState.AddModelError("", "Geçersiz Değer!");
				return View();
			}
			appUser.Password = Password;
			var sonuc = await _service.SaveChangesAsync();
			if (sonuc > 0)
			{
				TempData["Message"] = @"<div class=""alert alert-success alert-dismissible fade show"" role=""alert"">
                             <strong>Şifreniz Güncellenmiştir! Giriş Ekranından Oturum Açabilirsiniz.</strong> 
                            <button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" aria-label=""Close""></button>
                            </div>";
			}
			else
			{
				ModelState.AddModelError("", "Güncelleme Başarısız!");
			}
			return View();
		}
	}
}
