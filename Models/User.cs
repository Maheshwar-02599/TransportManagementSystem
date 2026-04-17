using System.ComponentModel.DataAnnotations.Schema;

public class User
{
	public int Id { get; set; }
	public string Username { get; set; }
	public string Password { get; set; } // Hashed in DB
	public string Role { get; set; }

	[NotMapped]
	public string? NewPassword { get; set; }
}