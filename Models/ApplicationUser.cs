using System.ComponentModel.DataAnnotations.Schema;

using Microsoft.AspNetCore.Identity;

namespace Utal.Icc.Sgm.Models;

public class ApplicationUser : IdentityUser {
	public string? FirstName { get; set; }
	public string? LastName { get; set; }
	[NotMapped]
	public int UsernameChangeLimit { get; set; } = 10;
	public byte[]? ProfilePicture { get; set; }
}