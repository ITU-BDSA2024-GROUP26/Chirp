using System.Globalization;

namespace Chirp.CLI.Client.Tests;

public class UnitTests
{
    [Theory]
    [InlineData(0, "01/01/70 00:00:00")]
    [InlineData(1, "01/01/70 00:00:01")]
    [InlineData(1690891760, "08/01/23 12:09:20")]
    public void FormatTimestampTest(long unixTime, string expectedFormattedTime)
    {
        Assert.Equal(expectedFormattedTime, DateTimeOffset.FromUnixTimeSeconds(unixTime).ToUniversalTime()
            .ToString("MM/dd/yy HH:mm:ss", CultureInfo.InvariantCulture));
    }
}