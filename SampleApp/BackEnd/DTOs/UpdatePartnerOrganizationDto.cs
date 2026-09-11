using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs;

public class UpdatePartnerOrganizationDto
{
    [Required(ErrorMessage = "نام سازمان الزامی است")]
    [MinLength(2, ErrorMessage = "نام سازمان باید حداقل ۲ کاراکتر باشد")]
    public string Name { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? ContactMobile { get; set; }

    public string? ContractNumber { get; set; }

    public DateTime? ContractStartDate { get; set; }

    public DateTime? ContractEndDate { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
