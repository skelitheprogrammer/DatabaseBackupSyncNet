using Backup.Abstractions;

namespace Backup.API.Services;

public abstract class DatabaseBackupBase : IDatabaseBackup, IDatabaseRestore
{
    public abstract Task Backup(ConnectionSettings connectionSettings, BackupType backupType, string path);
    public abstract Task Restore(ConnectionSettings connectionSettings, string path);
}