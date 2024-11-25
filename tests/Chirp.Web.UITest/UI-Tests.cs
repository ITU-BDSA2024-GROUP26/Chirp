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

        await Page.GetByRole(AriaRole.Link, new() { Name = "Adrian" }).ClickAsync();

        //await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Adrian" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Adrian's Timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Adrian Hej, velkommen til kurset.")).ToBeVisibleAsync();
    }

    [Test, Order(2)]
    public async Task TestRegisterUser()
    {
        await Page.GotoAsync("http://localhost:5000/");
        await Page.GetByRole(AriaRole.Link, new() { Name = "Register" }).ClickAsync();
        await Page.GetByPlaceholder("username", new() { Exact = true }).ClickAsync();
        await Page.GetByPlaceholder("username", new() { Exact = true }).FillAsync("qwe");
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
    public async Task TestFollow()
    {
        // arrange 
        await TestLogin();
        // act
        await Page.Locator("li").Filter(new() { HasText = "Adrian [Follow] Hej," }).GetByRole(AriaRole.Button).ClickAsync();

        // assert
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "Adrian" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" })).ToBeVisibleAsync();
    }

    [Test, Order(5)]
    public async Task TestUnfollow()
    {
        // Arrange
        await TestLogin();

        // Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" }).ClickAsync();

        // Assert
        await Expect(Page.Locator("li").Filter(new() { HasText = "Adrian [Follow] Hej," }).GetByRole(AriaRole.Button)).ToBeVisibleAsync();
    }

    [Test, Order(6)]
    public async Task TestFollowPrivateTimeline()
    {
        await Page.GotoAsync("http://localhost:5000");

        await TestLogin();

        await Page.Locator("li").Filter(new() { HasText = "Adrian [Follow] Hej," }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Expect(Page.GetByText("Adrian [Unfollow] Hej,")).ToBeVisibleAsync();
    }

    [Test, Order(7)]
    public async Task TestUnfollowPrivateTimeline()
    {
        await Page.GotoAsync("http://localhost:5000");
        await TestLogin();

        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" }).ClickAsync();
        await Expect(Page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
    }

    [Test, Order(8)]
    public async Task TestOwnCheepsShowUpPrivateTimeline()
    {
        await Page.GotoAsync("http://localhost:5000");
        await TestLogin();
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).FillAsync("test message");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
        await Expect(Page.GetByText("qwe test message")).ToBeVisibleAsync();
    }

    [Test, Order(9)]
    public async Task TestLogout()
    {
        // Arrange part, logging in is already expected to work due to previous test passing
        await Page.GotoAsync("http://localhost:5000");
        await TestLogin();

        // act 
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        // Assert
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
    }

    [Test, order(10)]
    public async Task ForgetmeTestLogout()
    {
        // Arrange part, logging in is already expected to work due to previous test passing 
        await Page.GotoAsync("http://localhost:5000");
        await TestLogin();
        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();//change "my timeline" to "About me", once it is working

        // Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "Forget Me!" }).ClickAsync();

        // Assert
        //await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();   
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
    }
}