using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatSysBackend.Database.Models;

public class MessageDTO
{
    [Required] public Guid Id { get; set; }
    
    [ForeignKey("senderID")]
    public UserDTO_Short Sender { get; set; } 
    
    [ForeignKey("groupRecieverId")]
    public GroupchatDTO_Short? groupReciever { get; set; } 
    
    [ForeignKey("userRecieverId")]
    public UserDTO_Short? userReciever { get; set; } 
    
    [Required] public string content { get; set; }
    [Required] public DateTime createdDate { get; set; } = DateTime.Now;
    
}
