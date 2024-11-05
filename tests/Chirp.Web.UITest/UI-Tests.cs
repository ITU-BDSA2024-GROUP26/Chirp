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
        // Launch the server we will send requests to 
        // Note that this requires quite a bit of setup: 
        // The binaries(from dotnet publish) of the razor Pages project need to be in the bin/debug/net8.0 folder of this project 
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        server = Process.Start("dotnet", "Chirp.Web.dll");

        Thread.Sleep(1500); // give the server a little time, otherwise the first test can start before it's live
    }

    [OneTimeTearDown]
    public void Cleanup() 
    {
        server.Kill();
        server.Dispose();
    }

    // Order needs to be >1, since order 0 seems to be parallel with onetimesetup(ie can get crashes due to server not being live)
    [Test, Order(1)]
    public async Task TestAuthorTimeline()
    {
        // need to run this here and not in a [Setup] tagged method 
        // because we have no guarantee that [OnetimeSetup] will run before [Setup]
        // so unavoidable code duplication(might be a smart way I don't know)
        await Page.GotoAsync("http://localhost:5000"); 

        await Page.Locator("p").Filter(new() { HasText = "Jacqualine Gilcoine They were" }).GetByRole(AriaRole.Link).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Jacqualine Gilcoine's Timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Jacqualine Gilcoine They were")).ToBeVisibleAsync();

    }

    [Test, Order(2)]
    public async Task TestRegisterUser() 
    {
        await Page.GotoAsync("http://localhost:5000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByPlaceholder("name@example.com").ClickAsync();
        await Page.GetByPlaceholder("name@example.com").FillAsync("qwe@example.com");
        await Page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
        await Page.GetByLabel("Password", new() { Exact = true }).FillAsync("Qwe$$213");
        await Page.GetByLabel("Confirm Password").ClickAsync(new LocatorClickOptions
        {
            Modifiers = new[] { KeyboardModifier.ControlOrMeta },
        });
        await Page.GetByLabel("Confirm Password").FillAsync("Qwe$$213");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Click here to confirm your" }).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Public Timeline" }).ClickAsync();
    }

    [Test, Order(3)]
    public async Task TestLogin() 
    {
        await Page.GotoAsync("http://localhost:5000"); 
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByPlaceholder("name@example.com").ClickAsync();
        await Page.GetByPlaceholder("name@example.com").FillAsync("qwe@example.com");
        await Page.GetByPlaceholder("password").ClickAsync();
        await Page.GetByPlaceholder("password").FillAsync("Qwe$$213");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();


    }

    [Test, Order(4)]
    public async Task TestLogout() 
    {
        await Page.GotoAsync("http://localhost:5000"); 
        
        // LOGIN, know this will work cause of previous test
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByPlaceholder("name@example.com").ClickAsync();
        await Page.GetByPlaceholder("name@example.com").FillAsync("qwe@example.com");
        await Page.GetByPlaceholder("password").ClickAsync();
        await Page.GetByPlaceholder("password").FillAsync("Qwe$$213");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        // OUR LOGOUT TEST
        await Page.GetByRole(AriaRole.Link, new() { Name = "Logout" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Click here to Logout" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("You have successfully logged")).ToBeVisibleAsync();
    }
}