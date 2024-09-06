using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using SimpleDB;

const string csvPath = "chirp_cli_db.csv";
// recognises anything inbetween two quotation marks and arbitrary spaces, with a capture group excluding quotation marks 
Regex patMsg = new Regex("(?:\\s*\"+\\s*)(.+)(?:\\s*\"+\\s*)");
// Captures a continuous word with a ',' and spaces behind it 
Regex patName = new Regex("(\\w+)(?:\\s*,\\s*)");
// captures a number of arbitrary length with a ',' and spaces in front
Regex patTime = new Regex("(?:\\s*,\\s*)(\\d+)");

IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(csvPath);

switch (args[0])
{
    case "read":
    {
        var records = database.Read();
        foreach(var record in records) {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).ToLocalTime();
            Console.WriteLine(record.Author +" @ " + dateTime.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) +  ": " + record.Message);
        }

        

        break;
    }
    case "cheep":
    {
        var user = Environment.UserName;
        var message = args[1];
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        Cheep output = new(user, $"\"{message}\"", unixTime);

        database.Store(output);
        
        break;
    }
}


public record Cheep(string Author, string Message, long Timestamp);
