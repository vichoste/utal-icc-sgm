using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class GuideTeacherViewModel {
	[Display(Name = "ID")]
	public string? Id { get; set; }
	[Display(Name = "Nombre")]
	public string? GuideTeacherFirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? GuideTeacherLastName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina")]
	public string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? GuideTeacherSpecialization { get; set; }
}