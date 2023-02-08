using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser : IdentityUser {
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.GuideTeacherOfTheStudentProposal))]
	public virtual ICollection<StudentProposal?>? ImGuideTeacherOfTheStudentProposals { get; set; } = new HashSet<StudentProposal?>();
	#endregion
	#region GuideTeacherProposal
	[InverseProperty(nameof(GuideTeacherProposal.GuideTeacherOwnerOfTheGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal?>? GuideTeacherProposalsWhichIOwn { get; set; } = new HashSet<GuideTeacherProposal?>();
	#endregion
}