using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Account.Views.Profile;

public class TeacherViewModel {
	[Display(Name = "Oficina")]
	public string? TeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? TeacherSpecialization { get; set; }
}