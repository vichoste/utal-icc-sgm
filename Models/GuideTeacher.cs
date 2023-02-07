using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser : IdentityUser {
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.GuideTeacherOfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? ImGuideTeacherOfTheStudentProposals { get; set; } = new HashSet<StudentProposal>();
	#endregion
	#region TeacherProposal
	[InverseProperty(nameof(TeacherProposal.GuideTeacherOwnerOfTheTeacherProposal))]
	public virtual ICollection<TeacherProposal>? TeacherProposalsWhichIOwn { get; set; } = new HashSet<TeacherProposal>();
	#endregion
}