﻿namespace ChatSysBackend.Database.Models.Requests;

public class CreateUserRequest()
{
    
    public string Name { get; set; }
    public string Surname { get; set; }
    public string NtUser { get; set; }
    public string Email { get; set; }
    
    public string Password { get; set; }
    public string RepeatPassword { get; set; }
    
}