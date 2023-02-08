using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.ViewModels;

public class StudentProposalViewModel : ProposalViewModel {
	[Display(Name = "ID del profesor guía")]
	public virtual string? GuideTeacherId { get; set; }
	[Display(Name = "Nombre del profesor guía")]
	public virtual string? GuideTeacherName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del profesor guía"), EmailAddress]
	public virtual string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina")]
	public virtual string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios")]
	public virtual string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización")]
	public virtual string? GuideTeacherSpecialization { get; set; }
	[Display(Name = "Profesores co-guía")]
	public virtual ICollection<string>? AssistantTeachers { get; set; } = new HashSet<string>();
	[Display(Name = "¿Quién rechazó la propuesta?")]
	public virtual string? WhoRejected { get; set; }
	[Display(Name = "Justificación")]
	public string? RejectionReason { get; set; }
}