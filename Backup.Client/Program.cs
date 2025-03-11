using System.CommandLine;
using Client;

RootCommand rootCommand = new();

rootCommand.MapCommands();

await rootCommand.InvokeAsync(args);

