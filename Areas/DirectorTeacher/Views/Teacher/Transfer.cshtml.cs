using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class TransferViewModel {
	[Display(Name = "ID del director de carrera actual"), Required]
	public string? CurrentDirectorTeacherId { get; set; }
	[Display(Name = "ID del nuevo director de carrera"), Required]
	public string? NewDirectorTeacherId { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress, Required]
	public string? NewDirectorTeacherEmail { get; set; }
}