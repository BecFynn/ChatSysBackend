using System.ComponentModel.DataAnnotations;

namespace ChatSysBackend.Database.Models.Requests;

public class GetMessagesReponse
{
    [Required] public Target Target { get; set; }
    [Required] public List<MessageDTO> Messages { get; set; }
}
