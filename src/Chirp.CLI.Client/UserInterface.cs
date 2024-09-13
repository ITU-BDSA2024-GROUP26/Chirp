using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CLI.Client;

public static class UserInterface
{
    public static void PrintCheeps(IEnumerable<Cheep> cheeps) 
    {
        foreach (var cheep in cheeps)
        {
            Console.WriteLine(cheep.Author +" @ " + FormatTimestamp(cheep.Timestamp) +  ": " + cheep.Message);
        }
    }
    
    public static string FormatTimestamp(long unixTime)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).ToLocalTime()
            .ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture);
    }

}