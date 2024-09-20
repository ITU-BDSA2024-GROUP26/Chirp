using System.Collections;
using System.CommandLine;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;
using Chirp.CLI.Client;
using System.Net.Http;
using System.Net.Http.Json;

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

var databaseOption = new Option<string>(
    aliases: ["-d", "--database"],
    description: "Database url",
    getDefaultValue: () => "https://bdsagroup26chirpremotedb.azurewebsites.net"
);

rootCommand.AddGlobalOption(databaseOption);


CSVDatabase<Cheep>.SetPath(csvPath);



readCommand.SetHandler(async (databaseUrl) =>
{
    using HttpClient client = new HttpClient();
    string response = await client.GetStringAsync(databaseUrl + "/cheeps");
    Console.WriteLine(response);
    // var records = CSVDatabase<Cheep>.getInstance().Read();
    // UserInterface.PrintCheeps(records);
},
databaseOption);

cheepCommand.SetHandler((string cheepMessage, string databaseUrl) =>
{
    var user = Environment.UserName;
    var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    // test case for individual cheep
    Cheep output = new(user, $"\"{cheepMessage}\"", unixTime);
    CSVDatabase<Cheep>.getInstance().Store(output);
},
cheepArgument,
databaseOption);

await rootCommand.InvokeAsync(args);

public record Cheep(string Author, string Message, long Timestamp);
