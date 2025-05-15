namespace ChatSysBackend.Database.Models.Requests;

public class MessageResponse
{
    public string Action { get; set; }
    public UserDTO Sender { get; set; }
    public UserDTO UserReciever { get; set; }
    public GroupchatDTO GroupReciever { get; set; }
    public MessageDTO Message { get; set; }
    public DateTime CreatedDate { get; set; }
}