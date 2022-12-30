namespace Utal.Icc.Sgm.Models;

public class TeacherProfile {
	public string? Office { get; set; }
	public string? Schedule { get; set; }
	public string? Specialization { get; set; }
	public virtual ApplicationUser? ApplicationUser { get; set; }
}