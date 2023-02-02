using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.Proposal;

public class SendViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? Title { get; set; }
	[Display(Name = "ID del profesor guía"), Required]
	public string? GuideTeacherId { get; set; }
}