using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Models;

public partial class ApplicationUser {
	[InverseProperty(nameof(Proposal.GuideTeacherOfTheProposal))]
	public virtual ICollection<Proposal?>? ImGuideTeacherOfTheseProposals { get; set; } = new HashSet<Proposal?>();
}