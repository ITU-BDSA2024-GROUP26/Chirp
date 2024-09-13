using Xunit.Abstractions;

namespace Chirp.CLI.Client.Tests;

public class UnitTests(ITestOutputHelper testOutputHelper)
{
    private readonly ITestOutputHelper _testOutputHelper = testOutputHelper;

    [Theory]
    [InlineData(0, "01/01/70 01:00:00")]
    [InlineData(1, "01/01/70 01:00:01")]
    [InlineData(1690891760, "08/01/23 14:09:20")]
    public void FormatTimestampTest(long unixTime, string expectedFormattedTime)
    {
        _testOutputHelper.WriteLine(UserInterface.FormatTimestamp(unixTime));
        Assert.Equal(expectedFormattedTime, UserInterface.FormatTimestamp(unixTime));
    }
}