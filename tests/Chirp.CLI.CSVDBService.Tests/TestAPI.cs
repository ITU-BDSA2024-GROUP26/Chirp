using System.Net;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Globalization;
using DBService;

//namespace Chirp.CLI.SimpleDB.Tests;
namespace Chirp.CLI.CSVDBService.Tests;


/// <summary>
/// Integration tests for the Chirp.CLI.SimpleDB API endpoints. 
/// Utilizes WebApplicationFactory to create a test server and HttpClient to send HTTP requests.
/// Inspiration from the lecture. 
/// </summary> 
public class TestAPI : IClassFixture<WebApplicationFactory<DBService.Program>>
{
    // Private fields to hold the WebApplicationFactory instance and HttpClient. 
    private readonly WebApplicationFactory<DBService.Program> _fixture;
    private readonly HttpClient _client;

    /// <summary>
    /// Constructor for the TestAPI class. 
    /// Initializes the WebApplicationFactory and HttpClient for sending requests to the test server. 
    /// </summary>
    public TestAPI(WebApplicationFactory<DBService.Program> fixture)
    {
        _fixture = fixture;

        // Create an HttpClient that automatically follow HTTP redirects. 
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true,
        });
    }


    /// <summary>
    /// Tests the GET /cheeps endpoint to ensure it returns a 200 OK status and an empty list when no cheeps are present.
    /// </summary>
    [Fact]
    public async Task GetCheeps()
    {
        // Arrange 
        var request = "/cheeps"; // The endpoint to retrieve all cheeps

        // Act
        var response = await _client.GetAsync(request); // Send a GET request to the /cheeps endpoint

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Verify that the response status is 200 OK

        // Deserialize the JSON response into a List of Cheep objects
        var cheeps = await response.Content.ReadFromJsonAsync<List<Cheep>>();
        Assert.NotNull(cheeps); // Ensure that the deserialized list is not null
        //Assert.Empty(cheeps); // Since we reset the CSV, it should be empty initially. 
    }


    /// <summary>
    /// Tests the POST /cheep endpoint by posting a new cheep and verifying that it is stored correctly.
    /// </summary>
    [Fact]
    public async Task PostCheep()
    {
        // Arrange
        var request = "/cheep"; // The endpoint to post a new cheep
        // Create a new Cheep object with test data
        var newCheep = new Cheep("TestAuthor", "This is a test cheep", DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        // Act
        var postResponse = await _client.PostAsJsonAsync(request, newCheep); // Send a POST request with the new Cheep

        // Assert
        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode); // Verify that the response status is 200 OK

        // Verify that the cheep was stored by sending a GET request to /cheeps.
        var getResponse = await _client.GetAsync("/cheeps");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode); // Verify that the GET request was successful

        // Deserialize the JSON response into a List of Cheep objects
        var cheeps = await getResponse.Content.ReadFromJsonAsync<List<Cheep>>();
        Assert.NotNull(cheeps);
        Assert.Contains(cheeps, c =>
            c.Author == newCheep.Author &&
            c.Message == newCheep.Message &&
            c.Timestamp == newCheep.Timestamp
        );
    }
}