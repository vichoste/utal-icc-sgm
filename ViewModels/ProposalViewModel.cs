using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public abstract class ProposalViewModel : ApplicationViewModel {
	[Display(Name = "Título")]
	public virtual string? Title { get; set; }
	[Display(Name = "Descripción")]
	public virtual string? Description { get; set; }
}