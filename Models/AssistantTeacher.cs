using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	#region StudentProposal
	[InverseProperty(nameof(StudentProposal.AssistantTeachersOfTheStudentProposal))]
	public virtual ICollection<StudentProposal?>? ImAssistantTeacherOfTheStudentProposals { get; set; } = new HashSet<StudentProposal?>();
	#endregion
}