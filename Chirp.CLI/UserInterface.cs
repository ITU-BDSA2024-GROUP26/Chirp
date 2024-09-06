using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace Chirp.CLI;

public static class UserInterface
{
    public static void printCheeps(Cheep record) {

            var dateTime = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).ToLocalTime();
            Console.WriteLine(record.Author +" @ " + dateTime.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture) +  ": " + record.Message);
    

}

}