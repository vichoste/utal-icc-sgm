using System.ComponentModel.DataAnnotations;

using Utal.Icc.Sgm.ViewModels;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;

public class IndexTeacherViewModel : ApplicationUserViewModel {
	[Display(Name = "Director de carrera")]
	public bool IsDirectorTeacher { get; set; }
}