using System;
using System.Diagnostics;
using System.Globalization;
using Xunit;
using Chirp.CLI;
using System.Net.Mime;
using Xunit.Abstractions;
using System.Runtime.InteropServices;
using System.IO;
using System.Net;

namespace Chirp.CLI.Client.Tests;


public class EndToEndTests
{
    public static string ArrangeDatabase() 
    {
        var csvPath = "testCSVDB.csv";
        var content = @"Author,Message,Timestamp
ropf,""Hello, BDSA students!"",1690891760
adho,""Welcome to the course!"",1690978778";
        File.WriteAllText(csvPath, content);
        return csvPath;
    }

    public static Process ArrangeCSVDBService(string csvPath)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run -- -p {csvPath}",
            UseShellExecute = false,
            WorkingDirectory = "../../../../../src/Chirp.CSVDBService"
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        // Repeatedly try to send a request to the service until it responds (meaning the service is up and running)
        while (true)
        {
             try
            {
                var request = (HttpWebRequest)WebRequest.Create("http://localhost:5000/cheeps");
                request.Method = "GET";
    
                using var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK) return process;
            }
            catch (Exception) {}
            Thread.Sleep(500);
        }
    }

    [Fact]
    public void TestReadCheep()
    {
        // Arrange
        var csvPath = ArrangeDatabase();
        Process serviceProcess = null;

        try
        {
            serviceProcess = ArrangeCSVDBService(csvPath);
            var output = "";
            using (var clientProcess = new Process())
            {
                clientProcess.StartInfo.FileName = "dotnet";
                clientProcess.StartInfo.Arguments = "run read";
                clientProcess.StartInfo.UseShellExecute = false;
                clientProcess.StartInfo.WorkingDirectory = "../../../../../src/Chirp.CLI.Client";
                clientProcess.StartInfo.RedirectStandardOutput = true;
                clientProcess.Start();

                // Synchronously read the standard ouput of he spawned process.
                using var reader = clientProcess.StandardOutput;
                output = reader.ReadToEnd();
                clientProcess.WaitForExit();
            }

            var firstCheep = output.Split('\n')[0];
            Assert.StartsWith("ropf", firstCheep);
            Assert.EndsWith("Hello, BDSA students!", firstCheep);
        }
        finally
        {
            serviceProcess?.Kill();
            serviceProcess?.WaitForExit();
            serviceProcess?.Dispose();
            File.Delete(csvPath);
        }
    }
}