using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.ViewModels.Profile;

public class ChangePasswordViewModel {
	[DataType(DataType.Password), Display(Name = "Contraseña actual"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6), Required]
	public string? CurrentPassword { get; set; }
	[DataType(DataType.Password), Display(Name = "Nueva contraseña"), StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6), Required]
	public string? NewPassword { get; set; }
	[Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar nueva contraseña"), Required]
	public string? ConfirmNewPassword { get; set; }
}