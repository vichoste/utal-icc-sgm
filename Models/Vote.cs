using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public class Vote {
	#region Common
	[Key]
	public string? Id { get; set; }
	public bool IsApproved { get; set; }
	public string? Reason { get; set; }
	public ApplicationUser? WhoVoted { get; set; }
	public Memoir? Memoir { get; set; }
	#endregion
}