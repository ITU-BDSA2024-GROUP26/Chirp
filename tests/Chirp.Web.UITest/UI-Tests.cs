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
    private Process _server;

    [OneTimeSetUp]
    public void OneTimeSetUp() 
    {
        // Launch the server we will send requests to 
        // Note that this requires quite a bit of setup: 
        // The binaries(from dotnet publish) of the razor Pages project need to be in the bin/debug/net8.0 folder of this project 
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _server = Process.Start("dotnet", "Chirp.Web.dll");

        Thread.Sleep(20000); // give the server a little time, otherwise the first test can start before it's live
    }

    [OneTimeTearDown]
    public void Cleanup() 
    {
        _server.Kill();
        _server.Dispose();
    }

    // Order needs to be >1, since order 0 seems to be parallel with onetimesetup(ie can get crashes due to server not being live)
    [Test, Order(1)]
    public async Task TestAuthorTimeline()
    {
        // need to run this here and not in a [Setup] tagged method 
        // because we have no guarantee that [OnetimeSetup] will run before [Setup]
        // so unavoidable code duplication(might be a smart way I don't know)
        await Page.GotoAsync("http://localhost:5000"); 

        await Page.Locator("p").Filter(new() { HasText = "Adrian Hej, velkommen til kurset." }).GetByRole(AriaRole.Link).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Adrian Hej, velkommen til kurset.")).ToBeVisibleAsync();

    }

    [Test, Order(2)]
    public async Task TestRegisterUser() 
    {
        await Page.GotoAsync("http://localhost:5000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByPlaceholder("username", new() { Exact = true}).ClickAsync();
        await Page.GetByPlaceholder("username", new() { Exact = true}).FillAsync("qwe");
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
        await Page.GetByRole(AriaRole.Link, new() { Name = "Public Timeline" }).ClickAsync();
    }

    [Test, Order(3)]
    public async Task TestLogin() 
    {
        await Page.GotoAsync("http://localhost:5000"); 
        
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByPlaceholder("username").ClickAsync();
        await Page.GetByPlaceholder("username").FillAsync("qwe");
        await Page.GetByPlaceholder("password").ClickAsync();
        await Page.GetByPlaceholder("password").FillAsync("Qwe$$213");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Logout" })).ToBeVisibleAsync();


    }

    [Test, Order(4)]
    public async Task TestLogout() 
    {
        // Arrange part, logging in is already expected to work due to previous test passing
        await Page.GotoAsync("http://localhost:5000"); 
        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByPlaceholder("username").ClickAsync();
        await Page.GetByPlaceholder("username").FillAsync("qwe");
        await Page.GetByPlaceholder("password").ClickAsync();
        await Page.GetByPlaceholder("password").FillAsync("Qwe$$213");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        // act 
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        // Assert
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();

    }
}