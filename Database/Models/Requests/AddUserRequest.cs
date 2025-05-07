namespace ChatSysBackend.Database.Models.Requests;

public class AddUserRequest
{
    public Guid groupId { get; set; }
    public Guid userid { get; set; }

}