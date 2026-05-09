using System.ComponentModel.DataAnnotations;

public class CreateAdminDto
{
    [Required]
    public string Name { get; set; }
    [EmailAddress]
    public string Email { get; set; }
    public string Password { get; set; }
    public string Access { get; set; } 
}