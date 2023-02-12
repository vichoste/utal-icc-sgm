namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public virtual ApplicationUser? StudentOfTheProposal { get; set; }
	public virtual ApplicationUser? WhoRejected { get; set; }
	public string? Reason { get; set; }
}