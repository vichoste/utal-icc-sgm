namespace Utal.Icc.Sgm.Models;

public partial class Proposal {
	public string? Requirements { get; set; }
	public virtual ApplicationUser? GuideTeacherOfTheProposal { get; set; }
	public virtual ICollection<ApplicationUser?>? StudentsWhoAreInterestedInThisProposal { get; set; } = new HashSet<ApplicationUser?>();
	public virtual ApplicationUser? StudentWhoIsAssignedToThisProposal { get; set; }
}