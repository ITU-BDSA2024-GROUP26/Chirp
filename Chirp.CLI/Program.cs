using System.Collections;
using System.CommandLine;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
using Chirp.CLI;

const string csvPath = "chirp_cli_db.csv";
// recognises anything inbetween two quotation marks and arbitrary spaces, with a capture group excluding quotation marks 
Regex patMsg = new Regex("(?:\\s*\"+\\s*)(.+)(?:\\s*\"+\\s*)");
// Captures a continuous word with a ',' and spaces behind it 
Regex patName = new Regex("(\\w+)(?:\\s*,\\s*)");
// captures a number of arbitrary length with a ',' and spaces in front
Regex patTime = new Regex("(?:\\s*,\\s*)(\\d+)");

// inspired by https://learn.microsoft.com/en-us/dotnet/standard/commandline/define-commands
var rootCommand = new RootCommand();
var readCommand = new Command("read", "First-level subcommand");
rootCommand.Add(readCommand);
var cheepCommand = new Command("cheep", "First-level subcommand");
rootCommand.Add(cheepCommand);
var cheepArgument = new Argument<string>("Cheep Message", description: "message"); 
cheepCommand.Add(cheepArgument);
IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(csvPath);



readCommand.SetHandler(() =>
{
    var records = database.Read();
    UserInterface.PrintCheeps(records);
});

cheepCommand.SetHandler((cheepMessage) =>
{
    var user = Environment.UserName;
    var message = args[1];
    var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    // test case for individual cheep
    Cheep output = new(user, $""{message}"", unixTime);
    database.Store(output);
}, cheepArgument);

await rootCommand.InvokeAsync(args);

public record Cheep(string Author, string Message, long Timestamp);
