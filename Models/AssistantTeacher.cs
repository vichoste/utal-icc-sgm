using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.AssistantTeacher1OfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? ImAssistantTeacher1OfTheStudentProposals { get; set; } = new HashSet<StudentProposal>();
	[InverseProperty(nameof(StudentProposal.AssistantTeacher2OfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? ImAssistantTeacher2OfTheStudentProposals { get; set; } = new HashSet<StudentProposal>();
	[InverseProperty(nameof(StudentProposal.AssistantTeacher3OfTheStudentProposal))]
	public virtual ICollection<StudentProposal>? ImAssistantTeacher3OfTheStudentProposals { get; set; } = new HashSet<StudentProposal>();
	#endregion
	#region GuideTeacherProposal
	[InverseProperty(nameof(GuideTeacherProposal.AssistantTeacher1OfTheGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal>? ImAssistantTeacher1OfTheGuideTeacherProposals { get; set; } = new HashSet<GuideTeacherProposal>();
	[InverseProperty(nameof(GuideTeacherProposal.AssistantTeacher2OfTheGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal>? ImAssistantTeacher2OfTheGuideTeacherProposals { get; set; } = new HashSet<GuideTeacherProposal>();
	[InverseProperty(nameof(GuideTeacherProposal.AssistantTeacher3OfTheGuideTeacherProposal))]
	public virtual ICollection<GuideTeacherProposal>? ImAssistantTeacher3OfTheGuideTeacherProposals { get; set; } = new HashSet<GuideTeacherProposal>();
	#endregion
}