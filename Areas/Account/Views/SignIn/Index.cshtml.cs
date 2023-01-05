using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Utal.Icc.Sgm.Areas.Account.Views.SignIn;

public class IndexModel : PageModel {
	[Display(Name = "E-mail"), EmailAddress, Required]
	public string? Email { get; set; }
	[Display(Name = "Contraseña"), Required]
	public string? Password { get; set; }
	[Display(Name = "Recuérdame")]
	public bool RememberMe { get; set; }
}