using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.DirectorTeacher.Views.Teacher;

public class EditViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Nombre")]
	public string? FirstName { get; set; }
	[Display(Name = "Apellido")]
	public string? LastName { get; set; }
	[Display(Name = "RUT")]
	public string? Rut { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail"), EmailAddress]
	public string? Email { get; set; }
	[Display(Name = "Profesor guía")]
	public bool IsGuideTeacher { get; set; }
	[Display(Name = "Profesor co-guía")]
	public bool IsAssistantTeacher { get; set; }
	[Display(Name = "Profesor de curso")]
	public bool IsCourseTeacher { get; set; }
	[Display(Name = "Profesor de comité")]
	public bool IsCommitteeTeacher { get; set; }
	[Display(Name = "Creado")]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado")]
	public DateTimeOffset? UpdatedAt { get; set; }
}