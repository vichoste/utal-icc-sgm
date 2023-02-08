using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel : ApplicationViewModel {
	[Display(Name = "Nombre"), Required]
	public virtual string? FirstName { get; set; }
	[Display(Name = "Apellido"), Required]
	public virtual string? LastName { get; set; }
	[Display(Name = "RUT"), Required]
	public virtual string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public virtual string? Email { get; set; }
	[Display(Name = "Desactivado"), Required]
	public virtual bool IsDeactivated { get; set; }
}