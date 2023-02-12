using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	[InverseProperty(nameof(Proposal.AssistantTeachersOfTheProposal))]
	public virtual ICollection<Proposal?>? ImAssistantTeacherOfTheseProposals { get; set; } = new HashSet<Proposal?>();
}