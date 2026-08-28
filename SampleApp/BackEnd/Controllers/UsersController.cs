using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using BackEnd.Models;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }


    // دریافت لیست کاربران
    [HttpGet]
    public async Task<List<User>> Get()
    {
        return await _context.Users.ToListAsync();
    }


    // ثبت کاربر جدید
    [HttpPost]
    public async Task<ActionResult<User>> Create(User user)
    {
        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return user;
    }
}