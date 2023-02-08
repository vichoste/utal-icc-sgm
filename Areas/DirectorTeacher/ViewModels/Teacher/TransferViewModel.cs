using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.ViewModels.Teacher;

public class TransferViewModel {
	[Display(Name = "ID del director de carrera actual")]
	public string? CurrentDirectorTeacherId { get; set; }
	[Display(Name = "ID del nuevo director de carrera")]
	public string? NewDirectorTeacherId { get; set; }
	[Display(Name = "Nombre del nuevo director de carrera")]
	public string? NewDirectorTeacherName { get; set; }
}