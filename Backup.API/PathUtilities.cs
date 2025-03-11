namespace Backup.API;

public static class PathUtilities
{
    public static FileAttributes GetPathType(string path) =>
        Path.HasExtension(path)
            ? FileAttributes.Normal
            : FileAttributes.Directory;
}