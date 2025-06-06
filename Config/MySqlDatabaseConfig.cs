﻿namespace ChatSysBackend.Controllers.Config;

public class MySqlDatabaseConfig
{
    public string Host { get; set; }
    public int Port { get; set; } = 3306;
    public string Database { get; set; }
    public string User { get; set; }
    public string Password { get; set; }
    public string ConnectionString => $"Server={Host};Port={Port};Database={Database};Uid={User};Pwd={Password};";
}