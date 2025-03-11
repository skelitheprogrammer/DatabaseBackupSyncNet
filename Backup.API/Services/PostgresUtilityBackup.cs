using Backup.Abstractions;
using CliWrap;

namespace Backup.API.Services;

public class PostgresUtilityBackup : DatabaseBackupBase
{
    public override async Task Backup(ConnectionSettings connectionSettings, BackupType backupType, string path)
    {
        switch (backupType)
        {
            case BackupType.LOGICAL: await LogicalBackup(); break;
            case BackupType.PHYSICAL: await PhysicalBackup(); break;
            default: throw new ArgumentOutOfRangeException(nameof(backupType), backupType, null);
        }

        async Task LogicalBackup()
        {
            FileAttributes attributes = PathUtilities.GetPathType(path);

            string arguments = attributes switch
            {
                FileAttributes.Directory =>
                    $"-U {connectionSettings.Username} -h {connectionSettings.Host} -p {connectionSettings.Port} -c -d {connectionSettings.DatabaseName} -Fd -f {path}",
                FileAttributes.Normal =>
                    $"-U {connectionSettings.Username} -h {connectionSettings.Host} -p {connectionSettings.Port} -c -d {connectionSettings.DatabaseName} -f {path}",
                FileAttributes.Archive => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException("Invalid path")
            };
            
            await Cli
                .Wrap("pg_dump")
                .WithArguments(arguments)
                .ExecuteAsync();
        }

        async Task PhysicalBackup()
        {
            FileAttributes attributes = PathUtilities.GetPathType(path);


            string arguments = attributes switch
            {
                FileAttributes.Directory =>
                    $"-U {connectionSettings.Username} -h {connectionSettings.Host} -p {connectionSettings.Port} -D {path}",
                FileAttributes.Normal => throw new ArgumentException("Path can't be a file"),
                FileAttributes.Archive => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException("Invalid path")
            };

            await Cli
                .Wrap("pg_basebackup")
                .WithArguments(arguments)
                .ExecuteAsync();
        }
    }

    public override async Task Restore(ConnectionSettings connectionSettings, string path)
    {
        string arguments =
            $"-U {connectionSettings.Username} -h {connectionSettings.Host} -p {connectionSettings.Port} -c -d {connectionSettings.DatabaseName} -f {path}";

        await Cli
            .Wrap("pg_restore")
            .WithArguments(arguments)
            .ExecuteAsync();
    }
}