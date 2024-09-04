using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;

const string csvPath = "chirp_cli_db.csv";
// recognises anything inbetween two quotation marks and arbitrary spaces, with a capture group excluding quotation marks 
Regex patMsg = new Regex("(?:\\s*\"+\\s*)(.+)(?:\\s*\"+\\s*)");
// Captures a continuous word with a ',' and spaces behind it 
Regex patName = new Regex("(\\w+)(?:\\s*,\\s*)");
// captures a number of arbitrary length with a ',' and spaces in front
Regex patTime = new Regex("(?:\\s*,\\s*)(\\d+)");

switch (args[0])
{
    case "read":
    {
        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            var records = csv.GetRecords<Cheep>();
            foreach(var record in records) {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).ToLocalTime();
                Console.WriteLine(record.Author +" @ " + dateTime.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) +  ": " + record.Message);
            }
        }

        

        break;
    }
    case "cheep":
    {
        var user = Environment.UserName;
        var message = args[1];
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var entry = $"{user},\"{message}\",{unixTime}";
    
        using var writer = new StreamWriter(csvPath, true);
        writer.WriteLine(entry);
        break;
    }
}


public record Cheep(long Timestamp, string Author, string Message);
