using System.ComponentModel.DataAnnotations;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;

public class EditTeacherViewModel : ApplicationUserViewModel {
	[Display(Name = "Profesor guía")]
	public bool IsGuideTeacher { get; set; }
	[Display(Name = "Profesor co-guía")]
	public bool IsAssistantTeacher { get; set; }
	[Display(Name = "Profesor de curso")]
	public bool IsCourseTeacher { get; set; }
	[Display(Name = "Profesor de comité")]
	public bool IsCommitteeTeacher { get; set; }
}