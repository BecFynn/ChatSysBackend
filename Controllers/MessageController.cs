using AutoMapper;
using ChatSysBackend.Controllers.Database.DTO;
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

    public MessageController(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;

    }
    
    [Route("/Send")]
    [HttpPost]

    public async Task<IActionResult> Send([FromBody] SendMessageRequest req)
    {
        User sender = _context.Users.FirstOrDefault((x) => x.Id == req.senderID);
        Groupchat _groupReciever =  _context.Groupchats.FirstOrDefault((x) => x.Id == req.receiverID);
        User _userReciever = _context.Users.FirstOrDefault((x) => x.Id == req.receiverID);

        if (sender == null || (_groupReciever == null && _userReciever == null))
        {
            return BadRequest("Sender or Receiver doesn't exist");
        }
        
        
        
        var foundSender = _groupReciever.Users.Find((x) => x.Id == sender.Id);
        if (foundSender == null)
        {
            return BadRequest("Sender not in Groupchat");
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
        
        //Emit to all with websockets
        return Created("", "Message send successfully");
    }
}