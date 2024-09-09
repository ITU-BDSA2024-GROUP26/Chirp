using System.Collections;
using System.CommandLine;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
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
var cheepCommand = new Command("cheep", "Second level subcommand");
rootCommand.Add(cheepCommand);
var cheepArgument = new Argument<string>("Cheep Message", description: "message"); 
cheepCommand.Add(cheepArgument);


readCommand.SetHandler(() =>
{
    ReadCsvFile(csvPath);
});

cheepCommand.SetHandler((cheepMessage) =>
{
    var user = Environment.UserName;
    var message = cheepMessage;
    var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
    WriteToCsvFile(user, message, unixTime);
}, cheepArgument);


void ReadCsvFile(string csvFilePath)
{
    using (var reader = new StreamReader(csvFilePath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<Cheep>();
        UserInterface.PrintCheeps(records);
        
    }
}


void WriteToCsvFile(string user, string message, long timestamp)
{
    Cheep output = new(user, $"\"{message}\"", timestamp);

    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) {
        ShouldQuote = (args) => false
    };

    using var writer = new StreamWriter(csvPath, true);
    using var csvWriter = new CsvWriter(writer, csvConfig);
    csvWriter.WriteRecord<Cheep>(output);
    csvWriter.NextRecord();
}


public record Cheep(string Author, string Message, long Timestamp);
