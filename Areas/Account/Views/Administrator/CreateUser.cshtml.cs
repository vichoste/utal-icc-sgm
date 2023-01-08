using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Administrator;

public class CreateUserViewModel {
	[Display(Name = "Nombre"), Required]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido"), Required]
	public string? LastName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Email { get; set; }
	[DataType(DataType.Password), Display(Name = "Contraseña"), Required, StringLength(100, ErrorMessage = "La contraseña debe tener un mínimo de 6 carácteres", MinimumLength = 6)]
	public string? Password { get; set; }
	[Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden"), DataType(DataType.Password), Display(Name = "Confirmar contraseña"), Required]
	public string? ConfirmPassword { get; set; }
	[Display(Name = "Roles"), Required]
	public List<RoleViewModel>? Roles { get; set; }

	public class RoleViewModel {
		public string? Name { get; set; }
		public bool IsSelected { get; set; }
	}
}