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
	#region TeacherProposal
	[InverseProperty(nameof(TeacherProposal.StudentsWhichAreInterestedInThisTeacherProposal))]
	public virtual ICollection<TeacherProposal>? TeacherProposalsWhichImInterested { get; set; } = new HashSet<TeacherProposal>();
	#endregion
}