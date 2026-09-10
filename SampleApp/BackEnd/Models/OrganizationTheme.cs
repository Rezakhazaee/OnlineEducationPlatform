namespace BackEnd.Models;

public class OrganizationTheme
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public string PrimaryColor { get; set; } = "#568fa8";

    public string SecondaryColor { get; set; } = "#263d4a";

    public bool IsActive { get; set; } = true;
}
