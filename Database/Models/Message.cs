using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSysBackend.Database.Models;

public class Message
{
    [Required] public Guid Id { get; set; }
    
    [ForeignKey("senderID")]
    public User Sender { get; set; } 
    
    [ForeignKey("groupRecieverId")]
    public Groupchat? groupReciever { get; set; } 
    
    [ForeignKey("userRecieverId")]
    public User? userReciever { get; set; } 
    
    [Required] public string content { get; set; }
    [Required] public DateTime createdDate { get; set; } = DateTime.Now;
    
}
