using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit;

namespace Chirp.Web.UITest;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    [SetUp]
    public void Setup()
    {
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
        await Page.GotoAsync("http://localhost:5273/");

        // act
        await Page.Locator("p").Filter(new() { HasText = "Jacqualine Gilcoine They were" }).GetByRole(AriaRole.Link).ClickAsync();

        // assert 
        await Expect(Page.Locator("h2")).ToContainTextAsync("Jacqualine Gilcoine's Timeline");
        await Expect(Page.Locator("#messagelist")).ToContainTextAsync("Jacqualine Gilcoine His initials were L. L. How do you think this steak is rather reserved, and your Krusenstern. — 08/01/23 13:15:54");
    }

}