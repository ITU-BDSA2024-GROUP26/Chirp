using System.Globalization;
using System.Text.RegularExpressions;

const string csvPath = "chirp_cli_db.csv";
// test comment 
switch (args[0])
{
    case "read":
    {
        var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
        using var reader = new StreamReader(csvPath);
        reader.ReadLine();
        while (reader.ReadLine() is { } line)
        {
            var splitLine = csvParser.Split(line);
            var user = splitLine[0];
            var unixTime = long.Parse(splitLine[^1]);
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime();
            var formattedTime = dateTime.ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
            var message = splitLine[1].Substring(1, splitLine[1].Length - 2);
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
