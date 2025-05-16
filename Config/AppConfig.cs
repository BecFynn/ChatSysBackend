namespace ChatSysBackend.Controllers.Config;

public class AppConfig
{
    public MySqlDatabaseConfig Database { get; set; }
    public OAuthConfig OAuth { get; set; }
}