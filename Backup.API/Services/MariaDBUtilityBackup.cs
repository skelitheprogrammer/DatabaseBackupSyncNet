using Backup.Abstractions;
using CliWrap;

namespace Backup.API.Services;

public class MariaDBUtilityBackup : DatabaseBackupBase
{
    public override async Task Backup(ConnectionSettings connectionSettings, BackupType backupType, string path)
    {

        switch (backupType)
        {
            case BackupType.LOGICAL: await Logical(); break;
            case BackupType.PHYSICAL: await Physical(); break;
            default:
                throw new ArgumentOutOfRangeException(nameof(backupType), backupType, null);
        }

        async Task Physical()
        {
            string arguments = PathUtilities.GetPathType(path) switch
            {
                FileAttributes.Normal => throw new NotSupportedException("Can't be a file"),
                FileAttributes.Directory =>
                    $"--backup --target-dir={path} -u {connectionSettings.Username} -p -h {connectionSettings.Host} -P {connectionSettings.Port} --databases {connectionSettings.DatabaseName}",
                _ => throw new ArgumentOutOfRangeException("Invalid path")
            };

            await Cli.Wrap("mariadb-backup")
                .WithArguments(arguments)
                .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
                .ExecuteAsync();
        }

        async Task Logical()
        {
            FileAttributes attributes = PathUtilities.GetPathType(path);

            string arguments = attributes switch
            {
                FileAttributes.Directory => throw new NotSupportedException(),
                FileAttributes.Normal =>
                    $"-u {connectionSettings.Username} -h {connectionSettings.Host} -p -P {connectionSettings.Port} --add-drop-database {connectionSettings.DatabaseName} > {path}",
                FileAttributes.Archive => throw new NotSupportedException(),
                _ => throw new ArgumentOutOfRangeException("Invalid path")
            };

            await Cli
                .Wrap("mariadb-dump")
                .WithArguments(arguments)
                .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
                .ExecuteAsync();
        }
    }

    public override async Task Restore(ConnectionSettings connectionSettings, string path)
    {
        string command;
        string arguments;

        switch (PathUtilities.GetPathType(path))
        {
            case FileAttributes.Normal:
            {
                command = "mariadb-dump";

                arguments =
                    $"-u {connectionSettings.Username} -h {connectionSettings.Host} -p -P {connectionSettings.Port} {connectionSettings.DatabaseName} < {path}";
                break;
            }

            case FileAttributes.Directory:
            {
                command = "mariadb-backup";

                arguments =
                    $"--copy-back --target-dir={path} -u {connectionSettings.Username} -p -h {connectionSettings.Host} -P {connectionSettings.Port} --databases {connectionSettings.DatabaseName}";

                Console.WriteLine("preparing for restore");

                await Cli
                    .Wrap(command)
                    .WithArguments(
                        $"--prepare --target-dir={path} -u {connectionSettings.Username} -p -h {connectionSettings.Host} -P {connectionSettings.Port} --databases {connectionSettings.DatabaseName}")
                    .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
                    .ExecuteAsync();

                break;
            }

            default:
                throw new ArgumentOutOfRangeException("Invalid path");
        }


        await Cli
            .Wrap(command)
            .WithArguments(arguments)
            .WithStandardInputPipe(PipeSource.FromStream(Console.OpenStandardInput()))
            .ExecuteAsync();
    }
}