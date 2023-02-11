using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ProposalViewModel : ApplicationViewModel {
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Descripción")]
	public string? Description { get; set; }
	[Display(Name = "Estado de la propuesta")]
	public string? ProposalStatus { get; set; }
	[Display(Name = "Profesores co-guía")]
	public ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
}