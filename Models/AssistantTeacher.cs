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
	#region TeacherProposal
	[InverseProperty(nameof(TeacherProposal.AssistantTeacher1OfTheTeacherProposal))]
	public virtual ICollection<TeacherProposal>? ImAssistantTeacher1OfTheTeacherProposals { get; set; } = new HashSet<TeacherProposal>();
	[InverseProperty(nameof(TeacherProposal.AssistantTeacher2OfTheTeacherProposal))]
	public virtual ICollection<TeacherProposal>? ImAssistantTeacher2OfTheTeacherProposals { get; set; } = new HashSet<TeacherProposal>();
	[InverseProperty(nameof(TeacherProposal.AssistantTeacher3OfTheTeacherProposal))]
	public virtual ICollection<TeacherProposal>? ImAssistantTeacher3OfTheTeacherProposals { get; set; } = new HashSet<TeacherProposal>();
	#endregion
}