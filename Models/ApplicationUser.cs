using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	#region Common
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	public string? Rut { get; set; }
	#endregion
	#region Student
	public string? StudentUniversityId { get; set; }
	public string? StudentRemainingCourses { get; set; }
	public bool StudentIsDoingThePractice { get; set; }
	public bool StudentIsWorking { get; set; }
	[InverseProperty("StudentOwnerOfTheStudentProposal")]
	public virtual ICollection<StudentProposal>? StudentProposalsWhichIOwn { get; set; }
	#endregion
	#region Teacher
	public string? TeacherOffice { get; set; }
	public string? TeacherSchedule { get; set; }
	public string? TeacherSpecialization { get; set; }
	#endregion
	#region AssistantTeacher
	[InverseProperty("AssistantTeachersCandidatesOfTheStudentProposal")]
	public virtual ICollection<StudentProposal>? StudentProposalsWhereImAAssistantTeacherCandidate { get; set; }
	#endregion
	#region GuideTeacher
	[InverseProperty("GuideTeacherCandidateOfTheStudentProposal")]
	public virtual ICollection<StudentProposal>? StudentProposalsWhereImAGuideTeacherCandidate { get; set; }
	#endregion
}