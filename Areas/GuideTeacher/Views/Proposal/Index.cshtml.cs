using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.Proposal;

public class IndexViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Estudiante")]
	public string? Student { get; set; }
	[Display(Name = "Estado")]
	public string? ProposalStatus { get; set; }
}