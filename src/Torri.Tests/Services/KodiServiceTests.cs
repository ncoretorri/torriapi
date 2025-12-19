using Microsoft.Extensions.Logging;
using NSubstitute;
using Torri.Models;
using Torri.Services;

namespace Torri.Tests.Services;

public class KodiServiceTests
{
    [Fact]
    public void GenerateKodiData()
    {
        var masks = new List<SerieMask>
        {
            new()
            {
                Mask = "@&&"
            }
        };
        masks.ForEach(x => x.GenerateRegex(Substitute.For<ILogger>()));
        var service = new KodiService(null!, null!);
        var data = service.ProcessFileName(masks, "404.mkv");

        data.season.ShouldBe(4);
        data.filename.ShouldBe("S04E04.mkv");
    }

    [Fact]
    public void GenerateKodiDataFixSeason()
    {
        var masks = new List<SerieMask>
        {
            new()
            {
                FixSeason = 6,
                Mask = "&&"
            }
        };
        masks.ForEach(x => x.GenerateRegex(Substitute.For<ILogger>()));
        var service = new KodiService(null!, null!);
        var data = service.ProcessFileName(masks, "24.mkv");

        data.season.ShouldBe(6);
        data.filename.ShouldBe("S06E24.mkv");
    }

    [Theory]
    [InlineData("@&&-&&")]
    [InlineData("@&&-##")]
    [InlineData("@##-&&")]
    public void GenerateKodiDataMoreEpisodes(string mask)
    {
        var masks = new List<SerieMask>
        {
            new()
            {
                Mask = mask
            }
        };
        masks.ForEach(x => x.GenerateRegex(Substitute.For<ILogger>()));
        var service = new KodiService(null!, null!);
        var data = service.ProcessFileName(masks, "512-13.mkv");

        data.season.ShouldBe(5);
        data.filename.ShouldBe("S05E12E13.mkv");
    }
}