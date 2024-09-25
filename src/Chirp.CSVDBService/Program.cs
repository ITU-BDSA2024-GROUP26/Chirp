using SimpleDB;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Cheep> cheepList = new List<Cheep>(100);
CSVDatabase<Cheep>.SetPath("chirp_csv_db.csv");
IDatabaseRepository<Cheep> db = CSVDatabase<Cheep>.getInstance();

app.MapGet("/cheeps/{num}", (int num) => { return db.Read(num); });
app.MapGet("/cheeps", () => db.Read());
app.MapPost("/cheep", (Cheep cheep) => db.Store(cheep));
app.Run();


public record Cheep(string Author, string Message, long Timestamp);
