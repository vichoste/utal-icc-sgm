using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public abstract class ApplicationViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Fecha de creación")]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Fecha de actualización")]
	public DateTimeOffset? UpdatedAt { get; set; }
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public string? IRejectedTheseStudentProposals { get; set; }
}