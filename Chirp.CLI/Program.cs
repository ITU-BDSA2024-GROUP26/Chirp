using System.Globalization;
using System.Text.RegularExpressions;

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
        var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        using var reader = new StreamReader(csvPath);
        reader.ReadLine();
        while (reader.ReadLine() is { } line)
        {
            if(!patName.IsMatch(line) || !patMsg.IsMatch(line) || !patTime.IsMatch(line)) { continue; }
            //var splitLine = csvParser.Split(line);
            var user = patName.Match(line).Groups[1].Value;
            var unixTime = long.Parse(patTime.Match(line).Groups[1].Value);
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
            var formattedTime = dateTime.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
            var message = patMsg.Match(line).Groups[1].Value;
            Console.WriteLine($"{user} @ {formattedTime}: {message}");
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
