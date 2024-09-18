using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
CSVDatabase<Cheep>.SetPath("csvdb.csv");

app.MapGet("/cheeps", () => CSVDatabase<Cheep>.getInstance().Read());
app.MapPost("/cheep", (Cheep cheep) => {
    CSVDatabase<Cheep>.getInstance().Store(cheep);
    });
app.Run();

public record Cheep(string Author, string Message, long Timestamp);
