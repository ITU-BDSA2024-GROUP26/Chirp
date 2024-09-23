using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
//CSVDatabase<Cheep>.SetPath("csvdb.csv");
List<Cheep> cheepList = new List<Cheep>(100);

app.MapGet("/cheeps", () => cheepList);
app.MapPost("/cheep", (Cheep cheep) => {
    cheepList.Add(cheep);
    });
app.Run();

public record Cheep(string Author, string Message, long Timestamp);
