using System.ComponentModel.DataAnnotations;

namespace Utal.Icc.Sgm.Models;

public abstract class Proposal {
	[Key]
	public string? Id { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public DateTimeOffset? CreatedAt { get; set; }
	public DateTimeOffset? UpdatedAt { get; set; }
	[Timestamp]
	public byte[]? RowVersion { get; set; }
}