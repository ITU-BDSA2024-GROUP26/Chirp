using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Microsoft.AspNetCore.Mvc.Testing;
using Chirp.Web;
using NUnit;
using NUnit.Framework.Internal;
using System.Diagnostics;

namespace Chirp.Web.UITest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    private Process server;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        server = Process.Start("dotnet", "Chirp.Web.dll");
    }

    [OneTimeTearDown]
    public void Cleanup() 
    {
        server.Kill();
        server.Dispose();
    }


    [Test]
    public void Test1()
    {
        Assert.Pass();
    }
    
    [Test]
    public async Task TestAuthorTimeline()
    {
        // arrange
        Console.WriteLine("Starting test author timeline");
        //Thread.Sleep(3000);
        await Page.GotoAsync("http://localhost:5000");

        // act
        await Page.Locator("p").Filter(new() { HasText = "Roger Histand You are here" }).GetByRole(AriaRole.Link).ClickAsync();

        // assert 
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Roger Histand's Timeline" })).ToBeVisibleAsync();
        }



}