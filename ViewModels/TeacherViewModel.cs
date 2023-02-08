using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Oficina")]
	public virtual string? TeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public virtual string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public virtual string? TeacherSpecialization { get; set; }
}