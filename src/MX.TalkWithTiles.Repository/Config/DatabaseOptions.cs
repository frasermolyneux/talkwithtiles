namespace MX.TalkWithTiles.Repository.Config;

public sealed class DatabaseOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 5432;
    public string Name { get; set; } = "talkwithtiles";
    public string Username { get; set; } = "talkwithtiles";
    public string PasswordFile { get; set; } = "/run/secrets/database-password";
}
