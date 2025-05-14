using System.ComponentModel.DataAnnotations;

namespace ChatSysBackend.Database.Models;

public class Target
{
    [Required] public Guid Id { get; set; }
   [Required] public string Name { get; set; }
   [Required] public string Type { get; set; }
}