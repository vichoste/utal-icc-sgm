using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.GuideTeacherProposal;

public class DeleteViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Title { get; set; }
}