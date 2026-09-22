namespace BackEnd.DTOs;

public class EnrollmentPriceDto
{
    public int CourseId { get; set; }

    public decimal CoursePrice { get; set; }

    public decimal? AgreedPrice { get; set; }

    public decimal FinalPrice { get; set; }

    public bool IsFree { get; set; }

    public bool HasOrganizationContract { get; set; }

    public int? PartnerOrganizationId { get; set; }

    public int? CoursePartnerOrganizationId { get; set; }
}
