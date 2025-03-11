namespace Backup.Abstractions;

public interface IDatabaseBackup
{
    Task Backup(ConnectionSettings connectionSettings, BackupType backupType, string path);
}