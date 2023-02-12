﻿using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser : IdentityUser {
	public enum Roles {
		DirectorTeacher,
		CommitteeTeacher,
		CourseTeacher,
		GuideTeacher,
		AssistantTeacher,
		Teacher,
		EngineerStudent,
		CompletedStudent,
		ThesisStudent,
		Student
	}
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public bool IsDeactivated { get; set; }
	[InverseProperty(nameof(Proposal.WhoRejected))]
	public virtual ICollection<Proposal?>? IRejectedTheseStudentProposals { get; set; } = new HashSet<Proposal?>();
}