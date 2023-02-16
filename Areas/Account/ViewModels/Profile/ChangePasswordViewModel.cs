using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.ViewModels.Profile;

public class ChangePasswordViewModel {
	[DataType(DataType.Password), Display(Name = "Contraseña actual"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? CurrentPassword { get; set; }
	[DataType(DataType.Password), Display(Name = "Nueva contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? NewPassword { get; set; }
	[Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar nueva contraseña")]
	public string? ConfirmNewPassword { get; set; }
}