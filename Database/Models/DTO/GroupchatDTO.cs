﻿public class GroupchatDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }

    public List<UserDTO_Short> Users { get; set; } = new();
}