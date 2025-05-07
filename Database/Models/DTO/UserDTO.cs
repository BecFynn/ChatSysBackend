public class UserDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string DisplayName { get; set; }
    public string NtUser { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<GroupchatDTO_Short> UserGroupchats { get; set; } = new();
}