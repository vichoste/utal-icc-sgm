using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.SignIn;

public class IndexViewModel {
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña"), Required]
	public string? Password { get; set; }
	[Display(Name = "Recuérdame")]
	public bool RememberMe { get; set; }
}