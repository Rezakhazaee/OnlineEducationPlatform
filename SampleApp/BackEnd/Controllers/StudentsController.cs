using BackEnd.DTOs;
using BackEnd.Data;
using BackEnd.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Support")]
public class StudentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public StudentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<StudentDto>>> Get()
    {
        var students = await _context.Students
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FirstName = s.FirstName,
                LastName = s.LastName,
                NationalCode = s.NationalCode,
                BirthDate = s.BirthDate,
                Mobile = s.Mobile,
                Address = s.Address,
                GuardianName = s.GuardianName,
                GuardianMobile = s.GuardianMobile,
                OrganizationId = s.OrganizationId,
                MarketingUserId = s.MarketingUserId,
                SupportUserId = s.SupportUserId,
                CreatedDate = s.CreatedDate
            })
            .ToListAsync();

        return Ok(students);
    }

    [HttpPost]
    public async Task<ActionResult<StudentDto>> Create(CreateStudentDto dto)
    {
        var student = new Student
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            NationalCode = dto.NationalCode,
            BirthDate = dto.BirthDate,
            Mobile = dto.Mobile,
            Address = dto.Address,
            GuardianName = dto.GuardianName,
            GuardianMobile = dto.GuardianMobile,
            OrganizationId = dto.OrganizationId,
            MarketingUserId = dto.MarketingUserId,
            SupportUserId = dto.SupportUserId
        };

        _context.Students.Add(student);

        await _context.SaveChangesAsync();

        var result = new StudentDto
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            NationalCode = student.NationalCode,
            BirthDate = student.BirthDate,
            Mobile = student.Mobile,
            Address = student.Address,
            GuardianName = student.GuardianName,
            GuardianMobile = student.GuardianMobile,
            OrganizationId = student.OrganizationId,
            MarketingUserId = student.MarketingUserId,
            SupportUserId = student.SupportUserId,
            CreatedDate = student.CreatedDate
        };

        return Ok(result);
    }
}