using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public abstract class ApplicationViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Fecha de creación"), Required]
	public virtual DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Fecha de actualización"), Required]
	public virtual DateTimeOffset? UpdatedAt { get; set; }
}