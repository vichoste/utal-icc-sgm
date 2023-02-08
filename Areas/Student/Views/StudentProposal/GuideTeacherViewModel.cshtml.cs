using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class GuideTeacherViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[Display(Name = "Oficina")]
	public string? TeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? TeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? TeacherSpecialization { get; set; }
}