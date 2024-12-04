using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;

namespace Web.UITest;

[NonParallelizable]
public class UiTests : PageTest
{
    private Process _server;
    private readonly HttpClient _client = new();
    private const string BaseUrl = "http://localhost:5000";
    private const string TestPassword = "Qwe$$213";

    private async Task<IPage> Register(string username, IPage? page = null)
    {
        // If no page argument is passed, open one.
        if (page == null)
        {
            var context = await Browser.NewContextAsync();
            page = await context.NewPageAsync();
        }

        await page.GotoAsync(BaseUrl + "/Identity/Account/Register");
        await page.GetByPlaceholder("username", new() { Exact = true }).ClickAsync();
        await page.GetByPlaceholder("username", new() { Exact = true }).FillAsync(username);
        await page.GetByPlaceholder("name@example.com").ClickAsync();
        await page.GetByPlaceholder("name@example.com").FillAsync(username + "@test.com");
        await page.GetByLabel("Password", new() { Exact = true }).ClickAsync();
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync(TestPassword);
        await page.GetByLabel("Confirm Password").ClickAsync(new LocatorClickOptions
        {
            Modifiers = [KeyboardModifier.ControlOrMeta],
        });
        await page.GetByLabel("Confirm Password").FillAsync(TestPassword);
        await page.GetByRole(AriaRole.Button, new() { Name = "Register" }).ClickAsync();
        
        return page;
    }

    private async Task Login(IPage page, string username, string password = TestPassword)
    {
        await page.GotoAsync(BaseUrl + "/Identity/Account/Login");
        await page.GetByRole(AriaRole.Link, new() { Name = "Login" }).ClickAsync();
        await page.GetByPlaceholder("username").ClickAsync();
        await page.GetByPlaceholder("username").FillAsync(username);
        await page.GetByPlaceholder("password").ClickAsync();
        await page.GetByPlaceholder("password").FillAsync(password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Log in" }).ClickAsync();
    }
    
    private async Task ToggleFollowingOnCurrentPage(IPage page, string username)
    {
        var cheep = page.Locator("li").Filter(new LocatorFilterOptions
        {
            Has = page.Locator("a", new PageLocatorOptions { HasTextString = username })
        });
        
        var followButton = cheep.GetByRole(AriaRole.Button).Filter(new LocatorFilterOptions
        {
            HasTextRegex = new Regex(@"\[?(Follow|Unfollow)\]?")
        });
        
        await followButton.ClickAsync();
    }

    private async Task ToggleFollowingAndReturn(IPage page, string username)
    {
        var currentUrl = page.Url;
        await page.GotoAsync(BaseUrl + "/username");
        
        var followButton = Page.GetByRole(AriaRole.Button).Filter(new LocatorFilterOptions
        {
            HasTextRegex = new Regex(@"\[?(Follow|Unfollow)\]?")
        });
        
        await followButton.ClickAsync();
    }

    private async Task CheepOnCurrentPage(IPage page, string message)
    {
        await Page.GetByRole(AriaRole.Textbox).ClickAsync();
        await Page.GetByRole(AriaRole.Textbox).FillAsync(message);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Share" }).ClickAsync();
    }
    
    [OneTimeSetUp]
    public async Task OneTimeSetUpAsync()
    {
        // Launch the server we will send requests to 
        // Note that this requires quite a bit of setup: 
        // The binaries(from dotnet publish) of the razor Pages project need to be in the bin/debug/net8.0 folder of this project 
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        _server = Process.Start("dotnet", "Web.dll");

        // Before returning, wait for the server to start up by pinging it.
        using var client = new HttpClient();
        const int maxRetries = 20;
        var delay = TimeSpan.FromSeconds(1);

        for (var i = 0; i <= maxRetries; i++)
        {
            try
            {
                await client.GetAsync(BaseUrl);
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
    public void OneTimeTeardown()
    {
        _server.Kill();
        _server.Dispose();
        _client.Dispose();
    }

    [SetUp]
    public async Task SetUpAsync()
    {
        var resetResponse = await _client.PostAsync(BaseUrl + "/api/development/reset", new StringContent(""));
        resetResponse.EnsureSuccessStatusCode();
        
        var seedResponse = await _client.PostAsync(BaseUrl + "/api/development/seed", new StringContent(""));
        seedResponse.EnsureSuccessStatusCode();
    }

    [Test]
    public async Task Register_ValidUserDetails_NavigatesToWelcomePage()
    {
        await Register("test", Page);
        
        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "What's on your mind, test" })).ToBeVisibleAsync();
    }


    [Test]
    public async Task Login_ValidUserDetails_NavigatesToWelcomePage()
    {
        await Register("test");
        await Login(Page, "test");
       
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "logout [test]" })).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task Logout_NavigatesToLoggedOutPublicTimeline()
    {
        await Register("test", Page);
        await Page.GetByRole(AriaRole.Button, new() { Name = "Logout" }).ClickAsync();
         
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About me" })).ToBeHiddenAsync();
    }
    
    [Test]
    public async Task Navbar_LoggedOut_DisplaysCorrectly()
    {
        await Page.GotoAsync(BaseUrl);
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "public timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "register" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "login" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About me" })).ToBeHiddenAsync();
    
    }
    
    [Test]
    public async Task Navbar_LoggedIn_DisplaysCorrectly()
    {
        await Register("test", Page);
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "public timeline" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "About me" })).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "logout [test]" })).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task ClickingAuthorLink_NavigatesToAuthorsPage()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.GetByRole(AriaRole.Link, new() { Name = "Adrian" }).ClickAsync();
        await Expect(Page).ToHaveURLAsync(BaseUrl + "/Adrian");
    }
    
     [Test]
     public async Task AuthorTimeline_NoCheeps_ShowsCorrectly()
     {
         await Register("test");
         
         await Page.GotoAsync(BaseUrl + "/test");
         
         await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "test's Timeline" })).ToBeVisibleAsync();
         await Expect(Page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
     }

     [Test]
     public async Task AuthorTimeline_Cheeps_ShowsCheeps()
     {
         await Page.GotoAsync(BaseUrl + "/Adrian");
         
         await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Adrian" })).ToBeVisibleAsync();
         await Expect(Page.GetByText("Hej, velkommen til kurset.")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task NotFollowingAuthor_ShowsFollowButton()
     {
         await Register("test", Page);
         await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "[Follow]" }).First).ToBeVisibleAsync();
     }

     [Test]
     public async Task FollowingAuthor_ShowsUnfollowButton()
     {
         await Register("test", Page);
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         await Expect(Page.GetByRole(AriaRole.Button, new() { Name = "[Unfollow]" })).ToBeVisibleAsync();
     }

     [Test]
     public async Task PrivateTimeline_NoFollowingNoCheeps_ShowsNoCheeps()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
         
         await Expect(Page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
     }

     [Test]
     public async Task PrivateTimeline_Following_ShowsCheeps()
     {
         await Register("test", Page);
         
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
         
         await Expect(Page.GetByText("Adrian [Unfollow] Hej,")).ToBeVisibleAsync();
         
     }

     [Test]
     public async Task PrivateTimeline_FollowingUnfollowing_ShowsNoCheeps()
     {
         await Register("test", Page);
         
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         
         await Expect(Page.GetByText("There are no cheeps so far.")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task PublicTimeline_Cheeps_ShowsOwnCheeps()
     {
         await Register("test", Page);
         await CheepOnCurrentPage(Page, "test message");
         await Expect(Page.GetByText("test message")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task PrivateTimeline_Cheeps_ShowsOwnCheeps()
     {
         await Register("test", Page);
         await Page.GetByRole(AriaRole.Link, new() { Name = "my timeline" }).ClickAsync();
         await CheepOnCurrentPage(Page, "test message");
         await Expect(Page.GetByText("test message")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task AboutMePage_DisplaysUserInformation()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();

         
         await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "My Information" })).ToBeVisibleAsync();
         await Expect(Page.GetByText($"Username: test")).ToBeVisibleAsync();
         await Expect(Page.GetByText($"Email: test@test.com")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task AboutMePage_NoFollowing_ShowsNoFollowingMessage()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         
         await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "Following" })).ToBeVisibleAsync();
         await Expect(Page.GetByText("You are not following anyone.")).ToBeVisibleAsync();
     }
     
     [Test]
     public async Task AboutMePage_Following_ShowsFollowingList()
     {
         await Register("test", Page);
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         
         await Expect(Page.GetByText("Adrian")).ToBeVisibleAsync();
     }

     [Test]
     public async Task AboutMePage_FollowingUnfollowing_ShowsNoFollowingMessage()
     {
         await Register("test", Page);
         await ToggleFollowingOnCurrentPage(Page, "Adrian");
         await ToggleFollowingOnCurrentPage(Page, "Adrian");

         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         
         await Expect(Page.GetByText("You are not following anyone.")).ToBeVisibleAsync();
         await Expect(Page.GetByText("Adrian")).ToBeHiddenAsync();
     }
     
     [Test]
     public async Task AboutMePage_DownloadButtonWorks()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         var downloadTask = Page.WaitForDownloadAsync();
         await Page.GetByRole(AriaRole.Button, new() { Name = "Download Your Data" }).ClickAsync();
         var download = await downloadTask;
         
         Assert.That(download, Is.Not.Null);
     }
     
     
     [Test]
     public async Task AboutMe_ForgetMeButtonRedirectsToLoggedOutPublicTimeline()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         await Page.GetByRole(AriaRole.Button, new() { Name = "Forget Me!" }).ClickAsync();
         
         await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Login" })).ToBeVisibleAsync();
         await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Register" })).ToBeVisibleAsync();
     }

     [Test]
     public async Task AboutMe_ForgetMeButtonDeletesUser()
     {
         await Register("test", Page);
         
         await Page.GetByRole(AriaRole.Link, new() { Name = "About me" }).ClickAsync();
         await Page.GetByRole(AriaRole.Button, new() { Name = "Forget Me!" }).ClickAsync();
         await Login(Page, "test");
         
         await Expect(Page.GetByText("Invalid login attempt.")).ToBeVisibleAsync();
     }
}