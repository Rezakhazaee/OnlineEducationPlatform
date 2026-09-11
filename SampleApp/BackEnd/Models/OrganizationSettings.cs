namespace BackEnd.Models;

public class OrganizationSettings
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public string PrimaryColor { get; set; } = "#568fa8";

    public string SecondaryColor { get; set; } = "#263d4a";

    public bool ThemeIsActive { get; set; } = true;
}
