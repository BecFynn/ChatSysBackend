using System.Text.Json;
using AutoMapper;

using ChatSysBackend.Database.Models;
using ChatSysBackend.Database.Models.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatSysBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class MessageController : ControllerBase
{
    private DataContext _context;
    private readonly IMapper _mapper;
    private readonly WebsocketManager _webSocketManager;
    public MessageController(DataContext context, IMapper mapper, WebsocketManager webSocketManager)
    {
        _context = context;
        _mapper = mapper;
        _webSocketManager = webSocketManager;

    }
    
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var messages = await _context.Messages
                .Include(x => x.groupReciever)
                .Include(x => x.userReciever)
                .Include(x => x.Sender)
                .ToListAsync();
            
            return Ok(messages);
        }
        catch (Exception ex)
        {
            return NotFound("Fehler");
        }
    }

    
    [Route("/Send")]
    [HttpPost]

    public async Task<IActionResult> Send([FromBody] SendMessageRequest req)
    {
        User? sender = _context.Users.FirstOrDefault((x) => x.Id == req.senderID);
        Groupchat? _groupReciever =  _context.Groupchats.FirstOrDefault((x) => x.Id == req.receiverID);
        User? _userReciever = _context.Users.FirstOrDefault((x) => x.Id == req.receiverID);

        if (sender == null)
        {
            return BadRequest("Sender not found");
        }

        if (_userReciever == null && _groupReciever == null)
        {
            return BadRequest("Receiver not found");
        }

        if (_groupReciever != null)
        {
            var foundSender = _groupReciever.Users.Find((x) => x.Id == sender.Id);
            if (foundSender == null)
            {
                return BadRequest("Sender not in Groupchat");
            }
        }
        
        var newMessage = new Message()
        {
            Id = Guid.NewGuid(),
            Sender = sender,
            
            userReciever = _userReciever,
            groupReciever = _groupReciever,
            
            content = req.content,
        };
        
        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();
        
        var messageObject = new
        {
            action = "message",
            sender = sender,
            userReciever = _userReciever,
            groupReciever = _groupReciever,
            content = req.content,
        };
        string json = JsonSerializer.Serialize(messageObject);
        await _webSocketManager.BroadcastAsync(json);
        
        
        //Emit to all with websockets
        return Created("", "Message send successfully");
    }
}