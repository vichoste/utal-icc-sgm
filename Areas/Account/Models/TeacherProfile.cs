using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Utal.Icc.Sgm.Areas.Account.Models;

public class TeacherProfile {
	[Key]
	public Guid Id { get; set; }
	public string? Office { get; set; }
	public string? Schedule { get; set; }
	public string? Specialization { get; set; }
	[ForeignKey(nameof(Models.ApplicationUser.TeacherProfile))]
	public virtual ApplicationUser? ApplicationUser { get; set; }
}