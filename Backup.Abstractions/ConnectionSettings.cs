namespace Backup.Abstractions;

public readonly struct ConnectionSettings
{
    public ConnectionSettings(string username, string host, string port, string databaseName)
    {
        Username = username;
        Host = host;
        Port = port;
        DatabaseName = databaseName;
    }

    public readonly string Username;
    public readonly string Host;
    public readonly string Port;
    public readonly string DatabaseName;
}