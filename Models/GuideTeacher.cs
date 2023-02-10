using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.GuideTeacherOfTheStudentProposal))]
	public virtual ICollection<StudentProposal?>? ImGuideTeacherOfTheStudentProposals { get; set; } = new HashSet<StudentProposal?>();
	[InverseProperty(nameof(StudentProposal.GuideTeacherWhoRejectedThisStudentProposal))]
	public virtual ICollection<StudentProposal?>? IRejectedTheseStudentProposals { get; set; } = new HashSet<StudentProposal?>();
	#endregion
	#region GuideTeacherProposal
	[InverseProperty(nameof(GuideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal?>? GuideTeacherProposalsWhichIOwn { get; set; } = new HashSet<GuideTeacherProposal?>();
	#endregion
}