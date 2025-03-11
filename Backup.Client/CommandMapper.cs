using System.CommandLine;
using Backup.Abstractions;
using Backup.API.Services;

namespace Client;

public static class CommandMapper
{
    public static void MapCommands(this RootCommand rootCommand)
    {
        Argument<DatabaseType> databaseType = new("database-type");
        Argument<string> username = new("username");
        Argument<string> host = new("host");
        Argument<string> port = new("port");
        Argument<string> databaseName = new("database-name");
        Argument<string> path = new("path");

        MapBackupCommand();
        MapRestoreCommand();

        void MapBackupCommand()
        {
            Option<BackupType> backupType = new("--backup-type");

            Command backup = new("backup")
            {
                databaseType,
                backupType,
                username,
                host,
                port,
                databaseName,
                path,
            };

            backup.SetHandler(async (dt, u, h, pt, bt, dbn, p) =>
                {
                    IDatabaseBackup backup = dt switch
                    {
                        DatabaseType.MARIA => new MariaDBUtilityBackup(),
                        DatabaseType.POSTGRES => new PostgresUtilityBackup(),
                        _ => throw new ArgumentOutOfRangeException(nameof(dt), dt, null)
                    };

                    await backup.Backup(new ConnectionSettings(u, h, pt, dbn), bt, p);
                },
                databaseType,
                username,
                host,
                port,
                backupType,
                databaseName,
                path);

            rootCommand.Add(backup);
        }

        void MapRestoreCommand()
        {
            Command restore = new("restore")
            {
                databaseType,
                username,
                host,
                port,
                databaseName,
                path,
            };

            restore.SetHandler((dt, u, h, pt, dbn, p) =>
                {
                    IDatabaseRestore restore = dt switch
                    {
                        DatabaseType.MARIA => new MariaDBUtilityBackup(),
                        DatabaseType.POSTGRES => new PostgresUtilityBackup(),
                        _ => throw new ArgumentOutOfRangeException(nameof(dt), dt, null)
                    };

                    restore.Restore(new ConnectionSettings(u, h, pt, dbn), p);
                },
                databaseType,
                username,
                host,
                port,
                databaseName,
                path);

            rootCommand.Add(restore);
        }
    }
}