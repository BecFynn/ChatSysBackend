namespace ChatSysBackend.Controllers.Database.DTO;

public class GroupchatDTO
{
    public Guid GroupChatId { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<UserDTO> Users { get; set; } = new List<UserDTO>();
}