using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class CreatePartnerOrganizationAccountDto
{
    [Required]
    public string FullName { get; set; } = string.Empty;

    [Required]
    public string Mobile { get; set; } = string.Empty;

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}
