using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.ViewModels.SignIn;

public class IndexViewModel {
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña")]
	public string? Password { get; set; }
	[Display(Name = "Recuérdame")]
	public bool RememberMe { get; set; }
}