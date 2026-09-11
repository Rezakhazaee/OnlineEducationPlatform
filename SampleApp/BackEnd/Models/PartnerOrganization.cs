namespace BackEnd.Models;

public class PartnerOrganization
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? ContactMobile { get; set; }

    public string? ContractNumber { get; set; }

    public DateTime? ContractStartDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
