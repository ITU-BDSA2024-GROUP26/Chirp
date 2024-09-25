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

    app.MapGet("/cheeps", () => CSVDatabase<Cheep>.getInstance().Read());
    app.MapGet("/cheeps/{num}", (int num) => CSVDatabase<Cheep>.getInstance().Read(num));
    app.MapPost("/cheep", (Cheep cheep) => CSVDatabase<Cheep>.getInstance().Store(cheep));
    await app.RunAsync();
},
csvPathOption);

await rootCommand.InvokeAsync(args);

public record Cheep(string Author, string Message, long Timestamp);
