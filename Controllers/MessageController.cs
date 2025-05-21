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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetMessagesReponse))]
    public async Task<IActionResult> GetMessages([FromQuery] Guid target, int count = 20)
    {
        try
        {
            count = Math.Min(count, 50); // Limit to max 50

            // First try to find the target as a user
            var userTarget = await _context.Users
                .Where(u => u.Id == target)
                .FirstOrDefaultAsync();

            // If not a user, try to find it as a group
            var groupTarget = userTarget == null
                ? await _context.Groupchats.Where(g => g.Id == target).FirstOrDefaultAsync()
                : null;

            if (userTarget == null && groupTarget == null)
            {
                return NotFound("Target nicht gefunden");
            }

            string targetName = userTarget != null ? userTarget.DisplayName : groupTarget!.Name;
            string targetType = userTarget != null ? "user" : "group";

            // Get messages related to the target
            var messages = await _context.Messages
                .Where(m => (userTarget != null && m.userReciever != null && m.userReciever.Id == target)
                            || (groupTarget != null && m.groupReciever != null && m.groupReciever.Id == target))
                .OrderByDescending(m => m.createdDate)
                .Include(m => m.Sender)
                .Include(m => m.userReciever)
                .Include(m => m.groupReciever)
                .Take(count)
                .ToListAsync();

            messages.Reverse();

            var messageDtos = _mapper.Map<List<MessageDTO>>(messages);

            var myTarget = new Target
            {
                Id = target,
                Name = targetName,
                Type = targetType
            };

            var myRes = new GetMessagesReponse
            {
                Target = myTarget,
                Messages = messageDtos
            };

            return Ok(myRes);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return StatusCode(500, "Fehler beim Abrufen der Nachrichten");
        }
    }


    
    [Route("/Send")]
    [HttpPost]

    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MessageResponse))]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest req)
    {
        User? sender = _context.Users.FirstOrDefault((x) => x.Id == req.senderID);
        Groupchat? _groupReciever = _context.Groupchats
            .Include(g => g.Users)
            .FirstOrDefault(x => x.Id == req.receiverID);
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
            createdDate = DateTime.Now
        };
        
        await _context.Messages.AddAsync(newMessage);
        await _context.SaveChangesAsync();
        
        
        var messageResponse = new MessageResponse
        {
            Action = "message",
            Sender = _mapper.Map<UserDTO>(sender),
            UserReciever = _userReciever != null ? _mapper.Map<UserDTO>(_userReciever) : null,
            GroupReciever = _groupReciever != null ? _mapper.Map<GroupchatDTO>(_groupReciever) : null,
            Message = _mapper.Map<MessageDTO>(newMessage),
            CreatedDate = DateTime.Now
        };

        string json = JsonSerializer.Serialize(messageResponse, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await _webSocketManager.BroadcastAsync(json);
        
        
        //Emit to all with websockets
        return Created("", "Message send successfully");
    }
}