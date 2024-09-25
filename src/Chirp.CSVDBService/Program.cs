using SimpleDB;
using System.CommandLine;

    var rootCommand = new RootCommand();
    var csvPathOption = new Option<string>(
    aliases: new[] { "-p", "--path" },
    description: "Path to the CSV file",
    getDefaultValue: () => "csvdb.csv"
);
rootCommand.AddGlobalOption(csvPathOption);

rootCommand.SetHandler(async (string path) =>
{
    CSVDatabase<Cheep>.SetPath(path);
    var builder = WebApplication.CreateBuilder(args);
    var app = builder.Build();
    IDatabaseRepository<Cheep> database = CSVDatabase<Cheep>.getInstance();

    app.MapGet("/cheeps", () => database.Read());
    app.MapGet("/cheeps/{num}", (int num) => database.Read(num));
    app.MapPost("/cheep", (Cheep cheep) => database.Store(cheep));
    await app.RunAsync();
},
csvPathOption);

await rootCommand.InvokeAsync(args);

public record Cheep(string Author, string Message, long Timestamp);
