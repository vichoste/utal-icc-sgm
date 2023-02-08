using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel : ApplicationViewModel {
	[Display(Name = "Nombre")]
	public virtual string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public virtual string? LastName { get; set; }
	[Display(Name = "RUT")]
	public virtual string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public virtual string? Email { get; set; }
	[Display(Name = "Desactivado")]
	public virtual bool IsDeactivated { get; set; }
}