﻿using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Areas.Student.Views.StudentProposal;

public class ConvertViewModel {
	[Display(Name = "ID"), Required]
	public string? Id { get; set; }
	[Display(Name = "Título"), Required]
	public string? Title { get; set; }
	[Display(Name = "Descripción"), Required]
	public string? Description { get; set; }
	[Display(Name = "Nombre del profesor guía"), Required]
	public string? GuideTeacherName { get; set; }
	[DataType(DataType.EmailAddress), Display(Name = "E-mail del profesor guía"), EmailAddress]
	public string? GuideTeacherEmail { get; set; }
	[Display(Name = "Oficina"), Required]
	public string? GuideTeacherOffice { get; set; }
	[Display(Name = "Horarios"), Required]
	public string? GuideTeacherSchedule { get; set; }
	[Display(Name = "Especialización"), Required]
	public string? GuideTeacherSpecialization { get; set; }
	[Display(Name = "Primer profesor co-guía"), Required]
	public string? AssistantTeacher1 { get; set; }
	[Display(Name = "Segundo profesor co-guía"), Required]
	public string? AssistantTeacher2 { get; set; }
	[Display(Name = "Tercer profesor co-guía"), Required]
	public string? AssistantTeacher3 { get; set; }
	[Display(Name = "Creado"), Required]
	public DateTimeOffset? CreatedAt { get; set; }
	[Display(Name = "Actualizado"), Required]
	public DateTimeOffset? UpdatedAt { get; set; }
}