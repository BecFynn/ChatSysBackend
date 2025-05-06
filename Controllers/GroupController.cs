using System.Text.RegularExpressions;
using AutoMapper;
using ChatSysBackend.Controllers.Database.DTO;
using ChatSysBackend.Database.Models;
using Microsoft.AspNetCore.Mvc;

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
            return Ok(_context.Groupchats.ToList());
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string groupname, Guid creatorId)
    {   
        User groupCreator  = _context.Users.FirstOrDefault((x) => x.Id == creatorId); 
        var newGroupchat = new Groupchat()
        {
            GroupChatId = Guid.NewGuid(),
            Name = groupname,
            CreatedDate = DateTime.Now
        };
        newGroupchat.Users.Add(groupCreator);
        await _context.Groupchats.AddAsync(newGroupchat);
        await _context.SaveChangesAsync();
        
        var groupchatDto = _mapper.Map<GroupchatDTO>(newGroupchat);
        return Created("", groupchatDto);
    }
}