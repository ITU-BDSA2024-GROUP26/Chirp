using System;
using System.Diagnostics;
using System.Globalization;
using Xunit;
using Chirp.CLI;

namespace Chirp.CLI.Client.Tests;


public class EndToEndTests
{
    [Fact]

    public static void TestReadCheep()
    {
        // Arrange
        const string csvPath = "chirp_cli_db3.csv";

        // Act
        string output = "";
        using (var process = new Process())
        {
            process.StartInfo.FileName = "/opt/homebrew/bin/dotnet";
            process.StartInfo.Arguments = "run read";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = "/Users/vicki/Documents/Chirp/src/Chirp.CLI.Client";
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Synchronously read the standard ouput of he spawned process.
            StreamReader reader = process.StandardOutput;
            output = reader.ReadToEnd();
            process.WaitForExit();
        }

        string fstCheep = output.Split('\n')[0];

        // Assert
        Assert.StartsWith("ropf", fstCheep);
        Assert.EndsWith("Hello, BDSA students!", fstCheep);
    }
}
