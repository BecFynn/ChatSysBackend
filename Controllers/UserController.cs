using System.Runtime.InteropServices.JavaScript;
using AutoMapper;

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
    private readonly IMapper _mapper;
    public UserController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDTO[]))]
    public async Task<IActionResult> GetAllUsers()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.UserGroupchats)
                .ToListAsync();

            var userDTOs = _mapper.Map<List<UserDTO>>(users);
            return Ok(userDTOs);
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }

    [HttpGet]
    [Route("{id}")]
    public async Task<IActionResult> GetUser(Guid id)
    {   
        User user = await _context.Users.FindAsync(id);
        return Ok(user);
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
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

        newUser.Avatar =
            $"https://api.dicebear.com/9.x/open-peeps/svg?seed={newUser.NtUser}";
        await _context.SaveChangesAsync();
        return Created("", newUser);
    }
}