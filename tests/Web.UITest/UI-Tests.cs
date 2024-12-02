using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Diagnostics;

namespace Web.UITest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    private Process _server;

    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Launch the server we will send requests to 
        // Note that this requires quite a bit of setup: 
        // The binaries(from dotnet publish) of the razor Pages project need to be in the bin/debug/net8.0 folder of this project 
        System.Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _server = Process.Start("dotnet", "Web.dll");

        // Before returning, wait for the server to start up by pinging it.
        using var client = new HttpClient();
        const int maxRetries = 20;
        var delay = TimeSpan.FromSeconds(1);

        for (var i = 0; i <= maxRetries; i++)
        {
            try
            {
                await client.GetAsync("http://localhost:5000/");
                return;
            }
            catch (Exception e)
            {
                await Task.Delay(delay);
            }
        }
        throw new Exception("Server did not start within the expected time.");
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
        await TestLogin();

        await Page.Locator("li").Filter(new() { HasText = "Adrian [Follow] Hej," }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Expect(Page.GetByText("Adrian [Unfollow] Hej,")).ToBeVisibleAsync();
    }

    [Test, Order(7)]
    public async Task TestUnfollowPrivateTimeline()
    {
        await TestLogin();

        await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" }).ClickAsync();
        await Expect(Page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
    }

    [Test, Order(8)]
    public async Task TestOwnCheepsShowUpPrivateTimeline()
    {
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
        await TestLogin();

        // act 
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();

        // Assert
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
    }

    [Test, Order(10)]
    public async Task TestLoggedOutAboutMeButtonHidden()
    {
        await Page.GotoAsync("http://localhost:5000");
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About me" })).ToBeHiddenAsync();
    }

    [Test, Order(11)]
    public async Task TestLoggedInAboutMeButtonVisible()
    {
        await TestLogin();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About me" })).ToBeVisibleAsync();
    }

    [Test, Order(12)]
    public async Task TestAboutMePage()
    {
        await TestLogin();
        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "My Information" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Username: qwe")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Email: qwe@example.com")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Following" })).ToBeVisibleAsync();
        await Expect(Page.GetByText("You are not following anyone.")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Your Cheeps" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Listitem)).ToContainTextAsync("test message Posted on");
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Download Your Data" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Download Your Data" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "Forget Me!" })).ToBeVisibleAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "public timeline" }).ClickAsync();
        await Page.Locator("li").Filter(new() { HasText = "Adrian [Follow] Hej," }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
        await Expect(Page.Locator("ul").Filter(new() { HasText = "Adrian" })).ToBeVisibleAsync();
    }

    [Test, Order(13)]
    public async Task ForgetMeTestLogout()
    {
        // Arrange part, logging in is already expected to work due to previous test passing 
        await MakeHelgeFollowQwe();

        await TestLogin();
        await Page.GetByRole(AriaRole.Link, new() { Name = "about me" }).ClickAsync();//change "my timeline" to "About me", once it is working

        // Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "Forget Me!" }).ClickAsync();

        // Assert
        //await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Public Timeline" })).ToBeVisibleAsync();   
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
    }

    [Test, Order(14)]
    public async Task DownloadInfo()
    {
        await TestLogin();
        await Page.GetByRole(AriaRole.Link, new() { Name = "about me" }).ClickAsync();//change "my timeline" to "About me", once it is working

        // Act
        await Page.GetByRole(AriaRole.Button, new() { Name = "Download Your Data" }).ClickAsync();

        // Assert
        //Assert.IsTrue((await Page.ContentAsync()).Contains("Following:"));
    }

    private async Task MakeHelgeFollowQwe()
    {
        await Page.GotoAsync("http://localhost:5000");

        await Page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await Page.GetByPlaceholder("username").ClickAsync();
        await Page.GetByPlaceholder("username").FillAsync("Helge");
        await Page.GetByPlaceholder("password").ClickAsync();
        await Page.GetByPlaceholder("password").FillAsync("LetM31n!");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();

        await Page.Locator("li").Filter(new() { HasText = "Qwe [Follow] test" }).GetByRole(AriaRole.Button).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
    }
}

