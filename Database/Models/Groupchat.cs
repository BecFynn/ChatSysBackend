namespace ChatSysBackend.Database.Models;

public class Groupchat
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public List<User> Users { get; set; } = new List<User>();
}