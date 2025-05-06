namespace ChatSysBackend.Database.Models;

public class Groupchat
{
    public Guid GroupChatId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<User> Members { get; set; } = new List<User>();
}