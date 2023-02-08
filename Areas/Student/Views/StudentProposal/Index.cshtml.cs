using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class ProposalViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Profesor guía")]
	public string? GuideTeacherName { get; set; }
	[Display(Name = "Estado")]
	public string? ProposalStatus { get; set; }
}