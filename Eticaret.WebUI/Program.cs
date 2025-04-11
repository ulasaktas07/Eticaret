using Eticaret.Data;
using Eticaret.Service.Abstract;
using Eticaret.Service.Concrete;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace Eticaret.WebUI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.
			builder.Services.AddControllersWithViews();

			builder.Services.AddSession(options =>
			{
				options.Cookie.Name = ".Eticaret.Session";
				options.Cookie.HttpOnly = true;
				options.Cookie.IsEssential = true; // GDPR uyumlulu�u i�in gerekli
				options.IdleTimeout = TimeSpan.FromDays(1); // Oturum zaman a��m s�resi
				options.IOTimeout = TimeSpan.FromMinutes(10); // Giri� s�resi
			});

			builder.Services.AddDbContext<DatabaseContext>();
			builder.Services.AddScoped(typeof(IService<>), typeof(Service<>));

			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(x =>
			{
				x.LoginPath = "/Account/SignIn";
				x.AccessDeniedPath = "/AccessDenied";
				x.Cookie.Name = "Account";
				x.Cookie.MaxAge = TimeSpan.FromDays(7);
				x.Cookie.IsEssential = true;
			});
			builder.Services.AddAuthorization(x =>
			{
				x.AddPolicy("AdminPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin"));
				x.AddPolicy("UserPolicy", policy => policy.RequireClaim(ClaimTypes.Role, "Admin", "User", "Customer"));
			});
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (!app.Environment.IsDevelopment())
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();
			app.UseSession();

			app.UseAuthentication(); // �nce oturum a�ma
			app.UseAuthorization(); // Sonra yetkilendirme

			app.MapControllerRoute(
		   name: "admin",
		   pattern: "{area:exists}/{controller=Main}/{action=Index}/{id?}");
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}
	}
}
