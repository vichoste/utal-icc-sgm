using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	[InverseProperty(nameof(Proposal.GuideTeacherOfTheProposal))]
	public virtual ICollection<Proposal?>? ImGuideTeacherOfTheProposals { get; set; } = new HashSet<Proposal?>();
	[InverseProperty(nameof(Proposal.GuideTeacherOwnerOfTheProposal))]
	public virtual ICollection<Proposal?>? ProposalsWhichIOwn { get; set; } = new HashSet<Proposal?>();
}