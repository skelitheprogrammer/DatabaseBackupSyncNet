namespace Backup.Abstractions;

public interface IDatabaseRestore
{
    Task Restore(ConnectionSettings connectionSettings, string path);
}