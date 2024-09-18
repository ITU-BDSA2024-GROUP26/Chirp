using System;
using System.Diagnostics;
using System.Globalization;
using Xunit;
using Chirp.CLI;

namespace Chirp.CLI.Client.Tests;


public class EndToEndTests
{
    public static void ArrangeDatabase() 
    {
        FileInfo fInfo = new FileInfo("../../../../../scripts/makeDB.sh");
        using (var process = new Process()) 
        {
            process.StartInfo.FileName = "/bin/bash";
            process.StartInfo.Arguments = $"\"{fInfo.FullName}\"";
            process.Start();
        }
    }

    [Fact]
    public static void TestReadCheep()
    {
        // Arrange
        ArrangeDatabase();
        const string csvPath = "../chirp_cli_db3.csv";

        // Act
        string output = "";
        using (var process = new Process())
        {
            process.StartInfo.FileName = "/usr/bin/dotnet";
            process.StartInfo.Arguments = "run read";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.WorkingDirectory = "../../..";
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