namespace BackEnd.DTOs;

public class UpdateOrganizationThemeDto
{
    public string PrimaryColor { get; set; } = "#568fa8";

    public string SecondaryColor { get; set; } = "#263d4a";

    public bool IsActive { get; set; } = true;
}
