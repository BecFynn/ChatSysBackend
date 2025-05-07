

using ChatSysBackend.Database.Models;

public class SendMessageRequest
{
    public Guid senderID { get; set; }
    public Guid receiverID { get; set; }
    public string content { get; set; }
}