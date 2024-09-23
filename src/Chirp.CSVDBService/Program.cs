using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Cheep> cheepList = new List<Cheep>(100);

app.MapGet("/cheeps/{num}", (int num) => {
    if(num >= cheepList.Count) {
        return cheepList;
    }
    return cheepList.GetRange(0, num);
    });
app.MapGet("/cheeps", () => cheepList);
app.MapPost("/cheep", (Cheep cheep) => {
    cheepList.Add(cheep);
    });
app.Run();


public record Cheep(string Author, string Message, long Timestamp);
