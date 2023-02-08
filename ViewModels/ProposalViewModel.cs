using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public abstract class ProposalViewModel : ApplicationViewModel {
	[Display(Name = "Título"), Required]
	public virtual string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public virtual string? Description { get; set; }
}