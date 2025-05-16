namespace ChatSysBackend.Controllers.Config;

public class OAuthConfig
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TenantId { get; set; }
    public string MetaDataAddress { get; set; }
    public string Scopes { get; set; }
}