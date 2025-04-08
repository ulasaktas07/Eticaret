using System.ComponentModel.DataAnnotations;

namespace Eticaret.WebUI.Models
{
	public class LoginViewModel
	{
		[Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		public string Email { get; set; }
		[Display(Name = "Şifre"), Required(ErrorMessage = "{0} Alanı Boş Geçilemez!")]
		[DataType(DataType.Password)]
		public string Password { get; set; }
		public string? ReturnUrl { get; set; }
		public bool RememberMe { get; set; }
	}
}
