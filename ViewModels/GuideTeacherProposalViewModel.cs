using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public partial class ProposalViewModel {
	[Display(Name = "ID del profesor guía")]
	public string? GuideTeacherId { get; set; }
	[Display(Name = "Nombre del profesor guía")]
	public string? GuideTeacherName { get; set; }
	[Display(Name = "Requisitos")]
	public string? Requirements { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del profesor guía"), EmailAddress]
	public string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina")]
	public string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public string? GuideTeacherSpecialization { get; set; }
	[Display(Name = "Estudiantes interesados")]
	public ICollection<string>? StudentsWhoAreInterestedInThisProposal { get; set; } = new HashSet<string>();
}