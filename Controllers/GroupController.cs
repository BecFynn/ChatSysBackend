using System.Text.RegularExpressions;
using AutoMapper;

using ChatSysBackend.Database.Models;
using ChatSysBackend.Database.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSysBackend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GroupController : ControllerBase
{
    private DataContext _context;
    private readonly IMapper _mapper;
    public GroupController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var groupChats = await _context.Groupchats
                .Include(gc => gc.Users)
                .ToListAsync();

            var groupChatDTOs = _mapper.Map<List<GroupchatDTO>>(groupChats);
            return Ok(groupChatDTOs);
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }

    
    
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateGroupRequest req)
    {   
        User groupCreator  = _context.Users.FirstOrDefault((x) => x.Id == req.creatorId); 
        var newGroupchat = new Groupchat()
        {
            Id = Guid.NewGuid(),
            Name = req.groupname,
            //CreatedDate = DateTime.Now
        };
        newGroupchat.Users.Add(groupCreator);
        await _context.Groupchats.AddAsync(newGroupchat);
        await _context.SaveChangesAsync();
        
        var groupchatDto = _mapper.Map<GroupchatDTO>(newGroupchat);
        return Created("", groupchatDto);
    }

    [HttpPost("addUser")]
    public async Task<IActionResult> AddUser([FromBody] AddUserRequest req)
    {
        User user  = _context.Users.FirstOrDefault((x) => x.Id == req.userid);
        Groupchat group = _context.Groupchats.FirstOrDefault((x) => x.Id == req.groupId);
        group.Users.Add(user);
        await _context.SaveChangesAsync();
        return Created("", group);
    }

    [HttpDelete("removeUser")]
    public async Task<IActionResult> RemoveUser([FromBody] AddUserRequest req)
    {
        User user = _context.Users.FirstOrDefault((x) => x.Id == req.userid);
        Groupchat group = _context.Groupchats
            .Include(g => g.Users)
            .FirstOrDefault(x => x.Id == req.groupId);
        
        if (user == null || group == null)
        {
            return NotFound();
        }
        group.Users.Remove(user);
        await _context.SaveChangesAsync();
        return Created("", group);
    }
}

