using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.GuideTeacher.Views.GuideTeacherProposal;

public class DeleteViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Title { get; set; }
}