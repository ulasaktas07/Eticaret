using System.ComponentModel.DataAnnotations;

namespace Eticaret.WebUI.Models
{
	public class UserEditViewModel
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
	}
}
