using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.GuideTeacherProposal;

public class IndexViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Título")]
	public string? Title { get; set; }
	[Display(Name = "Estado")]
	public string? ProposalStatus { get; set; }
}