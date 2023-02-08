using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Oficina")]
	public string? TeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? TeacherSpecialization { get; set; }
}