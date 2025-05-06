using System.Text.RegularExpressions;

namespace ChatSysBackend.Database.Models;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateOnly CreatedDate { get; set; }
    public List<Group> Groups { get; set; }=new List<Group>(); 
}