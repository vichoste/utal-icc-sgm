using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	public string? StudentUniversityId { get; set; }
	public string? StudentRemainingCourses { get; set; }
	public bool StudentIsDoingThePractice { get; set; }
	public bool StudentIsWorking { get; set; }
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.StudentOwnerOfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? StudentProposalsWhichIOwn { get; set; } = new HashSet<StudentProposal>();
	#endregion
	#region GuideTeacherProposal
	[InverseProperty(nameof(GuideTeacherProposal.StudentsWhichAreInterestedInThisGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal>? GuideTeacherProposalsWhichImInterested { get; set; } = new HashSet<GuideTeacherProposal>();
	[InverseProperty(nameof(GuideTeacherProposal.StudentWhichIsAssignedToThisGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal>? GuideTeacherProposalsWhichIHaveBeenAssigned { get; set; } = new HashSet<GuideTeacherProposal>();
	#endregion
}