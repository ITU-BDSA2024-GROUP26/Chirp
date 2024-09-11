using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CLI;

public static class UserInterface
{
    public static void PrintCheeps(IEnumerable<Cheep> cheeps) {
       
        foreach (var cheep in cheeps)
        {
            var dateTime = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();
            Console.WriteLine(cheep.Author +" @ " + dateTime.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) +  ": " + cheep.Message);
        }
        
            //var dateTime = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).ToLocalTime();
            //Console.WriteLine(record.Author +" @ " + dateTime.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) +  ": " + record.Message);
    

}

}