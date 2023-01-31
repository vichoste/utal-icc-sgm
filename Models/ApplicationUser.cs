using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	public enum Roles {
		DirectorTeacher, // RANK ROLE
		CommitteeTeacher, // RANK ROLE
		CourseTeacher, // RANK ROLE
		GuideTeacher, // RANK ROLE
		AssistantTeacher, // RANK ROLE
		Teacher, // MAIN ROLE
		EngineerStudent, // RANK ROLE
		CompletedStudent, // RANK ROLE
		ThesisStudent, // RANK ROLE
		Student // MAIN ROLE
	}
	#region Common
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	#endregion
	#region Student
	public string? StudentUniversityId { get; set; }
	public string? StudentRemainingCourses { get; set; }
	public bool StudentIsDoingThePractice { get; set; }
	public bool StudentIsWorking { get; set; }
	[InverseProperty(nameof(StudentProposal.StudentOwnerOfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? StudentProposalsWhichIOwn { get; set; } = new HashSet<StudentProposal>();
	#endregion
	#region Teacher
	public string? TeacherOffice { get; set; }
	public string? TeacherSchedule { get; set; }
	public string? TeacherSpecialization { get; set; }
	#endregion
	#region AssistantTeacher
	[InverseProperty(nameof(StudentProposal.AssistantTeacherOfTheStudentProposal1))]
	public virtual StudentProposal? AssistantTeacherOfTheStudentProposals1 { get; set; }
	[InverseProperty(nameof(StudentProposal.AssistantTeacherOfTheStudentProposal2))]
	public virtual StudentProposal? AssistantTeacherOfTheStudentProposals2 { get; set; }
	[InverseProperty(nameof(StudentProposal.AssistantTeacherOfTheStudentProposal3))]
	public virtual StudentProposal? AssistantTeacherOfTheStudentProposals3 { get; set; }
	#endregion
	#region GuideTeacher
	[InverseProperty(nameof(StudentProposal.GuideTeacherOfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? StudentProposalsWhereImAGuideTeacherCandidate { get; set; } = new HashSet<StudentProposal>();
	#endregion
}