using System.Collections;
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

switch (args[0])
{
    case "read":
    {
        ReadCsvFile(csvPath);
        break;
    }
    case "cheep":
    {
        var user = Environment.UserName;
        var message = args[1];
        var unixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        
        WriteToCsvFile(user, message, unixTime);
        break;
    }
}


void ReadCsvFile(string csvFilePath)
{
    using (var reader = new StreamReader(csvFilePath))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<Cheep>();
        UserInterface.PrintCheeps(records);
        
        //foreach(var record in records)
        //{
            //UserInterface.printCheeps(record); 
       // }
        
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
