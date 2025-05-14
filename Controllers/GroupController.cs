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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupchatDTO[]))]

    public async Task<IActionResult> GetAllGroups()
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

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GroupchatDTO))]

    public async Task<IActionResult> GetGroup([FromRoute] Guid id)
    {
        var groupChat = await _context.Groupchats
            .Include(g => g.Users) // Include related Users
            .FirstOrDefaultAsync(g => g.Id == id);
        var groupchatDto = _mapper.Map<GroupchatDTO>(groupChat);

        return Ok(groupchatDto);
    }
    
    
    [HttpPost]
    public async Task<IActionResult> CreateGroup([FromBody] CreateGroupRequest req)
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
    public async Task<IActionResult> AddUserToGroup([FromBody] AddUserRequest req)
    {
        User user  = _context.Users.FirstOrDefault((x) => x.Id == req.userid);
        Groupchat group = _context.Groupchats.FirstOrDefault((x) => x.Id == req.groupId);
        group.Users.Add(user);
        await _context.SaveChangesAsync();
        return Created("", group);
    }

    [HttpDelete("removeUser")]
    public async Task<IActionResult> RemoveUserFromGroup([FromBody] AddUserRequest req)
    {
        User? user = _context.Users.FirstOrDefault((x) => x.Id == req.userid);
        Groupchat? group = _context.Groupchats
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

