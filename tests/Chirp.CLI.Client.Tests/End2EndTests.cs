using System.Diagnostics;
using System.Net;
using Xunit.Abstractions;

namespace Chirp.CLI.Client.Tests;


/// <summary>
/// Contains end-to-end tests for the Chirp CLI Client, verifying both reading and cheeping functionalities. 
/// </summary>
public class EndToEndTests : IAsyncLifetime
{
    private Process _serviceProcess;
    private string _csvPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "testCSVDB.csv"));
    private const string localDatabaseUrl = "http://localhost:5000";

    public async Task InitializeAsync()
    {
        // Arrange: Set up the test environment. 
        Console.WriteLine("Started setup");
        ArrangeDatabase();
        await ArrangeCSVDBServiceAsync();
        Console.WriteLine("Ended setup");
    }

    public async Task DisposeAsync()
    {
        _serviceProcess.Kill();
        await _serviceProcess.WaitForExitAsync();
        _serviceProcess.Dispose();
        File.Delete(_csvPath);
    }


    /// <summary>
    /// Creates a test CSV database with predefined cheeps. 
    /// </summary>
    private void ArrangeDatabase()
    {
        // Define the content of the CSV file with predefined cheeps. 
        const string content = """
                               Author,Message,Timestamp
                               ropf,"Hello, BDSA students!",1690891760
                               adho,"Welcome to the course!",1690978778
                               
                               """;
        File.WriteAllText(_csvPath, content);
    }

    /// <summary>
    /// Starts the CSV database web service using 'dotnet run -- -p {cvsPath} --urls http://localhost:5000'
    /// </summary>
    /// <param name="csvPath"> Path to the CSV databse file. </param>
    /// <returns> The process running the CSV databse web service. </returns>
    /// <exception cref="Exception"> Thrown if the service fails to start within the expected time. </exception>
    /// Async return type (for the group): Inspired by https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/async-return-types
    private async Task ArrangeCSVDBServiceAsync()
    {
        // Configure the process start information for the CSVDBService. 
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run -- -p {_csvPath}",
            UseShellExecute = false, // Required to redirect output streams.
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = "../../../../../src/Chirp.CSVDBService",
            CreateNoWindow = true // Runs the process without creating a window.
        };

        // Intialize the process with the specified start information. 
        var process = new Process { StartInfo = startInfo };

        // Start the CSVDBService process. 
        process.Start();
        
        int counter = 0;
        using HttpClient client = new();
        while (true)
        {
            try
            {
                var response = await client.GetAsync("http://localhost:5000/cheeps");
                response.EnsureSuccessStatusCode();
                _serviceProcess = process;
                return;
            }
            catch (Exception) // The service isn't up yet. wait 0.5 seconds and try again.
            {
                await Task.Delay(500);
                counter++;
            }
            
            if(counter > 100) {
                throw new Exception("Tried to start service process too many times");
            }
        }
    }

    /// <summary    /// >
    /// Tests the reading functionality of the Chirp    ///  Client.
    /// Ensures that the client c    /// an successfully retrieve cheeps from the service.
    /// </summary>
    [Fact]
    public async Task TestReadCheepAsync()
    {
        // Act: Execute the 'read' command of the Chirp CLI Client. 
        using var process = new Process();
        process.StartInfo.FileName = "dotnet";
        process.StartInfo.Arguments = $"run read --database {localDatabaseUrl}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.WorkingDirectory = "../../../../../src/Chirp.CLI.Client";
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        // Capture the standard output from the client. 
        var output = await process.StandardOutput.ReadToEndAsync();

        // wait for the client process to exit.
        await process.WaitForExitAsync();

        // Assert: verify that the output contains the expected cheeps.  
        var cheeps = output.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        Assert.NotEmpty(cheeps); // Ensure that cheeps were retrieved.

        var firstCheep = cheeps[0];
        Assert.StartsWith("ropf", firstCheep); // Verify the author of the first cheep. 
        Assert.EndsWith("Hello, BDSA students!", firstCheep); // Verify the message of the first cheep. 
    }



    /// <summary>
    /// Tests the cheeping functionality of the Chirp CLI Client.
    /// Ensures that the client can successfully send a cheep message and have it stored in the service's database.
    /// </summary>    
    [Fact]
    public void TestCheepAsync()
    {
        // Define the cheep message to send
        const string cheepMessage = "This is a test cheep!";

        // Act: Execute the 'cheep' command of the Chirp CLI Client. 
        using Process process = new();
        process.StartInfo.FileName = "dotnet";
        // Corrected the command from 'read' to 'cheep' and included the cheep message.
        process.StartInfo.Arguments = $"run cheep \"{cheepMessage}\" --database {localDatabaseUrl}";
        process.StartInfo.WorkingDirectory = "../../../../../src/Chirp.CLI.Client";
        process.StartInfo.CreateNoWindow = true;
        process.Start();

        // Wait for the client process to exit.
        process.WaitForExit();
        Thread.Sleep(1000); // Wait for the service to store the cheep.

        // Assert 
        // To verify that the cheep was stored. 
        // Read all lines from the CSV file, skipping the header. 
        var storedCheeps = File.ReadAllLines(_csvPath)
            .Skip(1) // skip header
            .Select(line => line.Split(','))
            .ToList();
      
        var lastCheep = storedCheeps.Last();
        Assert.Equal(Environment.UserName, lastCheep[0]);
        Assert.Equal(cheepMessage, lastCheep[1].Trim('"'));
    }
}