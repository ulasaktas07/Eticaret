using Eticaret.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Eticaret.Data.Configurations
{
	public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
	{
		public void Configure(EntityTypeBuilder<AppUser> builder)
		{
			builder.Property(x => x.Name).IsRequired().HasColumnType("NVARCHAR(50)").HasMaxLength(50);
			builder.Property(x => x.Surname).IsRequired().HasColumnType("NVARCHAR(50)").HasMaxLength(50);
			builder.Property(x => x.Email).IsRequired().HasColumnType("NVARCHAR(50)").HasMaxLength(50);
			builder.Property(x => x.Phone).HasColumnType("NVARCHAR(15)").HasMaxLength(15);
			builder.Property(x => x.Password).IsRequired().HasColumnType("NVARCHAR(50)").HasMaxLength(50);
			builder.Property(x => x.UserName).HasColumnType("NVARCHAR(50)").HasMaxLength(50);
			builder.HasData(
				new AppUser
				{
					Id = 1,
					UserName = "Admin",
					Email = "deneme@gmail.com",
					Password = "123",
					Name = "Admin",
					Surname = "Admin",
					IsActive = true,
					IsAdmin = true
				}
				);
		}
	}
}
