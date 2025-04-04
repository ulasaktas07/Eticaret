using Eticaret.Core.Entities;
using Eticaret.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Eticaret.Data
{
	public class DatabaseContext:DbContext
	{
		public DbSet<AppUser> AppUsers { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Category> Categories { get; set; }
		public DbSet<Brand> Brands { get; set; }
		public DbSet<Contact> Contacts { get; set; }
		public DbSet<News> News { get; set; }
		public DbSet<Slider> Sliders { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseSqlServer("Server=MELYAT;Database=EticaretDb; Trusted_Connection=True; TrustServerCertificate=True;");
			base.OnConfiguring(optionsBuilder);
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//modelBuilder.ApplyConfiguration(new AppUserConfiguration());
			//modelBuilder.ApplyConfiguration(new BrandConfiguration());

			modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
			base.OnModelCreating(modelBuilder);
		}
	}
}
