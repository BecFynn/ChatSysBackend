using System.Runtime.InteropServices.JavaScript;
using ChatSysBackend.Database.Models;
using ChatSysBackend.Database.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSysBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private DataContext _context;
    public UserController(DataContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            return Ok(_context.Users.ToList());
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateUserRequest req)
    {
        var newUser = new User()
        {
            Id = Guid.NewGuid(),
            Name = req.Name,
            Surname = req.Surname,
            DisplayName = req.DisplayName,
            NtUser = req.NtUser,
            Email = req.Email,
            //CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(newUser);
        await _context.SaveChangesAsync();
        return Created("", newUser);
    }
}