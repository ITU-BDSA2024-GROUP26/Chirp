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


/// <summary>
/// Contains end to end tests for the Chirp CLI Client, verifying both reading and cheeping functionalities. 
/// </summary>
public class EndToEndTests
{
    /// <summary>
    /// Creates a test CSV database with predefined cheeps. 
    /// </summary>
    /// <returns> The path to the created CSV file. </returns> 
    public static string ArrangeDatabase()
    {
        // Define the path for the temporary CSV database.
        var csvPath = "testCSVDB.csv";

        // Define the content of the CSV file with predefined cheeps. 
        var content = @"Author,Message,Timestamp
ropf,""Hello, BDSA students!"",1690891760
adho,""Welcome to the course!"",1690978778";
        File.WriteAllText(csvPath, content);
        return csvPath;
    }

    /// <summary>
    /// Starts the CSV database web service using 'dotnet run -- -p {cvsPath} --urls http://localhost:5000'
    /// </summary>
    /// <param name="csvPath"> Path to the CSV databse file. </param>
    /// <returns> The process running the CSV databse web service. </returns>
    /// <exception cref="Exception"> Thrown if the service fails to start within the expected time. </exception>
    /// Async return type (for the group): Inspired by https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-return-types
    public static async Task<Process> ArrangeCSVDBServiceAsync(string csvPath)
    {
        // Configure the process start information for the CSVDBService. 
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run -- -p {csvPath} --urls http://localhost:5000",
            UseShellExecute = false, // Required to redirect output streams.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = "../../../../../src/Chirp.CSVDBService/",
            CreateNoWindow = true // Runs the process without creating a window.
        };

        // Intialize the process with the specified start information. 
        var process = new Process { StartInfo = startInfo };

        // Start the CSVDBService process. 
        process.Start();

        //Log the standard output of the service for debugging purposes. 
        _ = Task.Run(() =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                string line = process.StandardOutput.ReadLine();
                Console.WriteLine($"[CSVDBService] {line}");
            }
        });

        // Log the standard error of the service for debugging purposes. 
        _ = Task.Run(() =>
        {
            while (!process.StandardError.EndOfStream)
            {
                string line = process.StandardError.ReadLine();
                Console.Error.WriteLine($"[CSVDBService ERROR] {line}");
            }
        });

        // Define a timeout period to wait for the service to become responsive. 
        var timeout = TimeSpan.FromSeconds(10);
        var startTime = DateTime.Now;

        // Continuously attempt to connect to the service until it responds or the timeout is reached.
        while (DateTime.Now - startTime < timeout)
        {
            try
            {
                // Create an HTTP GET request to check if the service is up. 
                var request = (HttpWebRequest)WebRequest.Create("http://localhost:5000/cheeps");
                request.Method = "GET";

                using var response = (HttpWebResponse)request.GetResponse();

                // If the response status is OK, the service is up and running. 
                if (response.StatusCode == HttpStatusCode.OK) return process;
            }
            catch (WebException) { } // Service not up yet, continue waiting.

            // Wait for half a second before retrying.   
            await Task.Delay(500);
            //Thread.Sleep(500);
        }

        // If the service did not start within the timeout, kill the process and throw an exception. 
        process.kill();
        throw new Exception("Failed to start CSVDBService within the expected time.");
    }

    /// <summary>
    /// Tests the reading functionality of the Chirp CLI Client.
    /// Ensures that the client can successfully retrieve cheeps from the service.
    /// </summary>
    [Fact]
    public async Task TestReadCheepAsync()
    {
        // Arrange: Set up the test environment. 
        var csvPath = ArrangeDatabase();
        Process serviceProcess = null;

        try
        {
            // Start the CSVDBService with the test CSV database. 
            serviceProcess = ArrangeCSVDBService(csvPath);
            var output = "";

            // Define the local databse URL where the service is running. 
            var localDatabaseUrl = "http://localhost:5000";


            // Act: Execute the 'read' command of the Chirp CLI Client. 
            using (var clientProcess = new Process())
            {
                clientProcess.StartInfo.FileName = "dotnet";
                clientProcess.StartInfo.Arguments = $"run read --database {localDatabaseUrl}";
                clientProcess.StartInfo.UseShellExecute = false;
                clientProcess.StartInfo.RedirectStandardOutput = true;
                clientProcess.StartInfo.RedirectStandardError = true;
                clientProcess.StartInfo.WorkingDirectory = "../../../../../src/Chirp.CLI.Client";
                clientProcess.StartInfo.CreateNoWindow = true;
                clientProcess.Start();


                // Capture Client error output for debugging. 
                string clientError = await clientProcess.StandardError.ReadToEndAsync();

                // Capture the standard output from the client. 
                output = await clientProcess.StandardOutput.ReadToEndAsync();

                // wait for the client process to exit.
                await clientProcess.WaitForExitAsync();

                // If the client process exited with a non-zero code, throw an exception with the error details.
                if (clientProcess.ExitCode != 0)
                {
                    throw new Exception($"Chirp CLI CLient exited with code {clientProcess.ExitCode}. Error: {clientError}");
                }
            }

            // Assert: verify that the output contains the expected cheeps.  
            var cheeps = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.NotEmpty(cheeps); // Ensure that cheeps were retrieved.

            var firstCheep = cheeps[0];
            Assert.StartsWith("ropf", firstCheep); // Verify the author of the first cheep. 
            Assert.EndsWith("Hello, BDSA students!", firstCheep); // Verify the message of the first cheep. 
        }
        finally
        {
            // Clean up: Ensure that the CSVDBService process is terminated and the test CSV file is deleted.
            if (serviceProcess != null && !serviceProcess.HasExited)
            {
                serviceProcess.Kill();
                await serviceProcess.WaitForExitAsync();
                serviceProcess.Dispose();
            }

            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
        }
    }



    /// <summary>
    /// Tests the cheeping functionality of the Chirp CLI Client.
    /// Ensures that the client can successfully send a cheep message and have it stored in the service's database.
    /// </summary>    
    [Fact]
    public async Task TestCheepAsync()
    {
        // Arrange: Set up the test environment. 
        var csvPath = ArrangeDatabase();
        Process serviceProcess = null;

        try
        {
            // Start the CSVDBService with the test CSV database. 
            serviceProcess = await ArrangeCSVDBServiceAsync(csvPath);
            var output = "";

            // Define the local database URL
            var localDatabaseUrl = "http://localhost:5000";

            // Define the cheep message to send
            var cheepMessage = "This is a test cheep!";

            // Act: Execute the 'cheep' comand of the Chirp CLI Client. 
            using (var clientProcess = new process())
            {
                clientProcess.StartInfo.FileName = "dotnet";
                // Corrected the command from 'read' to 'cheep' and included the cheep message.
                clientProcess.StartInfo.Arguments = $"run cheep \"{cheepMessage}\" --database {localDatabaseUrl}";
                clientProcess.StartInfo.UseShellExecute = false; // Required to redirect output streams.
                clientProcess.StartInfo.RedirectStandardOutput = true;
                clientProcess.StartInfo.RedirectStandardError = true;
                clientProcess.StartInfo.WorkingDirectory = "../../../../../src/Chirp.CLI.Client";
                clientProcess.StartInfo.CreateNoWindow = true;
                clientProcess.Start();

                // Capture Client error output for debugging.
                string clientError = await clientProcess.StandardError.ReadToEndAsync();

                // Capture the standard output from the client.
                output = await clientProcess.StandardOutput.ReadToEndAsync();

                // Wait for the client process to exit.
                await clientProcess.WaitForExitAsync();

                // If the client process exited with a non-zero code, throw an exception with the error details.
                if (clientProcess.ExitCode != 0)
                {
                    throw new Exception($"Chirp CLI Client exited with code {clientProcess.ExitCode}. Error: {clientError}");
                }
            }

            // Assert 
            // To verify that the cheep was stored. 
            // Read all lines from the CSV file, skipping the header. 
            var storedCheeps = File.ReadAllLines(csvPath)
                .Skip(1) // skip header
                .Select(line => line.Split(','))
                .ToList();

            // Check that at least one cheep matches the cheepMessage
            Assert.Contains(storedCheeps, cheepMessage => cheepMessage[1].Trim('"') == cheepMessage);
        }
        finally
        {
            // clean up: Ensure that the CSVDBService process is terminated and the test CSV file is deleted.
            if (serviceProcess != null && !serviceProcess.HasExited)
            {
                serviceProcess.Kill();
                await serviceProcess.WaitForExitAsync();
                serviceProcess.Dispose();
            }

            if (File.Exists(csvPath))
            {
                File.Delete(csvPath);
            }
        }
    }
}