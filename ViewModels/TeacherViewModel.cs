using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ApplicationUserViewModel {
	[Display(Name = "Oficina"), Required]
	public virtual string? TeacherOffice { get; set; }
	[Display(Name = "Horarios"), Required]
	public virtual string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización"), Required]
	public virtual bool TeacherSpecialization { get; set; }
}