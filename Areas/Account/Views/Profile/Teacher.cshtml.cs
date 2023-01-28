using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Profile;

public class TeacherViewModel {
	[Display(Name = "ID"), Required]
	public Guid? Id { get; set; }
	[Display(Name = "Oficina")]
	public string? Office { get; set; }
	[Display(Name = "Horarios")]
	public string? Schedule { get; set; }
	[Display(Name = "Especialización")]
	public string? Specialization { get; set; }
}