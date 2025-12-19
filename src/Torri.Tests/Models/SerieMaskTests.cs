using Microsoft.Extensions.Logging;
using NSubstitute;
using Torri.Models;

namespace Torri.Tests.Models;

public class SerieMaskTests
{
    [Fact]
    public void Asd()
    {
        var mask = new SerieMask
        {
            Mask = "@##-&&"
        };

        mask.GenerateRegex(Substitute.For<ILogger>());
        var match = mask.Regex.Match("404-05");
        match.Success.ShouldBeTrue();

        match.Groups["s"].Value.ShouldBe("4");
        match.Groups["e0"].Value.ShouldBe("04");
        match.Groups["e1"].Value.ShouldBe("05");
    }
}