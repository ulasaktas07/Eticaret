
using System.ComponentModel.DataAnnotations;

namespace Eticaret.Core.Entities
{
	public class AppUser:IEntity
	{
		public int Id { get; set; }
		[Display(Name = "Adı"), Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		public string Name { get; set; }
		[Display(Name = "Soyadı"), Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		public string Surname { get; set; }
		[Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		public string Email { get; set; }
		[Display(Name = "Şifre"), Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		public string Password { get; set; }
		[Display(Name = "Telefon")]
		public string? Phone { get; set; }
		[Display(Name = "Kullanıcı Adı")]
		public string? UserName { get; set; }
		[Display(Name = "Aktif?")]
		public bool IsActive { get; set; }
		[Display(Name = "Admin?")]
		public bool IsAdmin { get; set; }
		[Display(Name = "Kayıt Tarihi"),ScaffoldColumn(false)]
		public DateTime CreateDate { get; set; }= DateTime.Now;
		[ScaffoldColumn(false)]
		public Guid? UserGuid { get; set; }= Guid.NewGuid();
		public List<Address> Addresses { get; set; }
	}
}
