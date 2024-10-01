namespace DBService;
using SimpleDB;
using System.CommandLine;

public class Program
{
    public static async Task Main(string[] args)
    {
        var rootCommand = new RootCommand();

        // Allow unrecognized arguments to prevent errors when running tests.
        rootCommand.TreatUnmatchedTokensAsErrors = false;

        var csvPathOption = new Option<string>(
        aliases: new[] { "-p", "--path" },
        description: "Path to the CSV file",
        getDefaultValue: () => "csvdb.csv"
        );

        rootCommand.AddGlobalOption(csvPathOption);

        rootCommand.SetHandler(async (string path) =>
        {
            await RunWebApplicationAsync(path);
        },
        csvPathOption);

        await rootCommand.InvokeAsync(args);
    }

    private static async Task RunWebApplicationAsync(string path)
    {
        CSVDatabase<Cheep>.SetPath(path);

        var builder = WebApplication.CreateBuilder();

        builder.Services.AddSingleton<IDatabaseRepository<Cheep>>(CSVDatabase<Cheep>.getInstance());

        var app = builder.Build();

        // Define HTTP endpoints and their corresponding handlers

        app.MapGet("/cheeps", (IDatabaseRepository<Cheep> db) => Results.Ok(db.Read()));


        app.MapGet("/cheeps/{num}", (int num, IDatabaseRepository<Cheep> db) =>
        {
            var cheep = db.Read(num);
            return cheep != null ? Results.Ok(cheep) : Results.NotFound();
        });


        app.MapPost("/cheep", (Cheep cheep, IDatabaseRepository<Cheep> db) =>
        {
            db.Store(cheep);
            return Results.Ok();
        });

        await app.RunAsync();
    }
}

public record Cheep(string Author, string Message, long Timestamp);

/*public record User(int user_id, string username, string email)
{
    public User() : this(-1, "defaultUserName", "defaultEmail") { }
}
*/