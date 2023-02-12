using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	public string? StudentUniversityId { get; set; }
	public string? StudentRemainingCourses { get; set; }
	public bool? StudentIsDoingThePractice { get; set; }
	public bool? StudentIsWorking { get; set; }
	#region StudentProposal
	[InverseProperty(nameof(Proposal.StudentOfTheProposal))]
	public virtual ICollection<Proposal?>? ImStudentOfTheseProposals { get; set; } = new HashSet<Proposal?>();
	#endregion
	#region GuideTeacherProposal
	[InverseProperty(nameof(Proposal.StudentsWhoAreInterestedInThisProposal))]
	public virtual ICollection<Proposal?>? ProposalsWhichImInterested { get; set; } = new HashSet<Proposal?>();
	#endregion
}