// Taken from Helge's slides:

using Microsoft.AspNetCore.Mvc.Testing;
using Chirp.Razor;
using System.Linq;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.Sqlite;
using Chirp.Infrastructure.Repositories;
using Chirp.Core.Entities;

namespace Chirp.Infrastructure.Tests;

public class TestAPI : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _fixture;
    private readonly HttpClient _client;

    public TestAPI(WebApplicationFactory<Program> fixture)
    {
        _fixture = fixture.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {

                // ADD in-memory database for testing
                services.AddDbContext<CheepDBContext>(options =>
                        options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));

                services.AddScoped<ICheepRepository, CheepRepository>();

                // Ensure ChirpRepository is using the in-memory databse
                var serviceProvider = services.BuildServiceProvider();

                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var db = scopedServices.GetRequiredService<CheepDBContext>();

                db.Database.EnsureCreated();

                // Seed the in-memory database with test data
                DbInitializer.SeedDatabase(db);
            });
        });


        _fixture = fixture;
        _client = _fixture.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = true, HandleCookies = true });

    }

//     [Fact]
//     public async void CanSeePublicTimeline()
//     {
//         var response = await _client.GetAsync("/");
//         response.EnsureSuccessStatusCode();
//         var content = await response.Content.ReadAsStringAsync();
//
//         Assert.Contains("Chirp!", content);
//         Assert.Contains("Public Timeline", content);
//     }
//
//     [Theory]
//     [InlineData("Helge")]
//     [InlineData("Adrian")]
//     public async void CanSeePrivateTimeline(string author)
//     {
//         var response = await _client.GetAsync($"/{author}");
//         response.EnsureSuccessStatusCode();
//         var content = await response.Content.ReadAsStringAsync();
//
//         Assert.Contains("Chirp!", content);
//         Assert.Contains($"{author}'s Timeline", content);
//     }
//
//     [Theory]
//     [InlineData("Random user")]
//     public async Task NoCheepsAvailable(string author)
//     {
//         var response = await _client.GetAsync($"/{author}");
//         response.EnsureSuccessStatusCode();
//         var content = await response.Content.ReadAsStringAsync();
//
//         Assert.Contains("There are no cheeps so far.", content);
//     }
//
//     /*
//     // This test does not work. 
//     [Fact]
//     public async Task DifferentPages()
//     {
//         var response = await _client.GetAsync("/?page=0");
//         response.EnsureSuccessStatusCode();
//         var content1 = await response.Content.ReadAsStringAsync();
//
//         response = await _client.GetAsync("/?page=1");
//         response.EnsureSuccessStatusCode();
//         var content2 = await response.Content.ReadAsStringAsync();
//
//         Assert.NotEqual(content1, content2);
//     }*/
//
//     [Fact]
//     public async void IsPage1SameAsDefaultTimeline()
//     {
//         var responseHomePage = await _client.GetAsync("/");
//         var content1 = await responseHomePage.Content.ReadAsStringAsync();
//         responseHomePage.EnsureSuccessStatusCode();
//
//         var responseFirstPage = await _client.GetAsync("/?page=1");
//         var content2 = await responseFirstPage.Content.ReadAsStringAsync();
//         responseFirstPage.EnsureSuccessStatusCode();
//
//         Assert.Contains("Chirp!", content1);
//         Assert.Contains("Public Timeline", content1);
//         Assert.Contains("Chirp!", content2);
//         Assert.Contains("Public Timeline", content2);
//         Assert.Equal(content1, content2);
//     }
}

