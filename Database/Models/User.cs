using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace ChatSysBackend.Database.Models;

public class User
{
    [Required] public Guid Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Surname { get; set; }
    [Required] public string DisplayName { get; set; }
    [Required] public string NtUser { get; set; }
    [Required] public string Email { get; set; }
    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string? Avatar{ get; set; }
    public List<Groupchat> UserGroupchats { get; set; } = new List<Groupchat>(); 
}