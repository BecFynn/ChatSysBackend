using System.Text.RegularExpressions;
using ChatSysBackend.Database.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatSysBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private DataContext _context;

    public GroupController(DataContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            return Ok(_context.Groupchats.ToList());
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string Groupname)
    {
        var newGroupchat = new Groupchat()
        {
            GroupChatId = Guid.NewGuid(),
            Name = Groupname,
            CreatedDate = DateTime.Now

        };
        await _context.Groupchats.AddAsync(newGroupchat);
        await _context.SaveChangesAsync();
        return Created("",newGroupchat);
    }
}