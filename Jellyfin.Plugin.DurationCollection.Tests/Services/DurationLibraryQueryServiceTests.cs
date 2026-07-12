using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Jellyfin.Plugin.DurationCollection.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.DurationCollection.Tests.Services;

public class DurationLibraryQueryServiceTests
{
    private readonly ILibraryManager _libraryManager = Substitute.For<ILibraryManager>();

    [Fact]
    public void GetSeriesByAverageEpisodeDuration_IncludesInclusiveBoundaries()
    {
        var shortSeries = Series("short", "short-key");
        var longSeries = Series("long", "long-key");
        ConfigureLibrary(
            [shortSeries, longSeries],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["short-key"] = [Episode(10), Episode(20)],
                ["long-key"] = [Episode(20), Episode(40)],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetSeriesByAverageEpisodeDuration(15, 30);

        result.Should().BeEquivalentTo([shortSeries, longSeries]);
    }

    [Fact]
    public void GetSeriesByAverageEpisodeDuration_ExcludesOutsideRange()
    {
        var below = Series("below", "below-key");
        var above = Series("above", "above-key");
        ConfigureLibrary(
            [below, above],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["below-key"] = [Episode(14.99)],
                ["above-key"] = [Episode(30.01)],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetSeriesByAverageEpisodeDuration(15, 30);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSeriesByAverageEpisodeDuration_SkipsSeriesWithoutKnownRuntime()
    {
        var series = Series("unknown", "unknown-key");
        ConfigureLibrary(
            [series],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["unknown-key"] = [new Episode(), new Episode { RunTimeTicks = 0 }],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetSeriesByAverageEpisodeDuration(0, 30);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetSeriesByAverageEpisodeDuration_AveragesOnlyKnownPositiveRuntimes()
    {
        var series = Series("mixed", "mixed-key");
        ConfigureLibrary(
            [series],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["mixed-key"] = [Episode(10), Episode(20), new Episode(), new Episode { RunTimeTicks = 0 }],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetSeriesByAverageEpisodeDuration(15, 15);

        result.Should().ContainSingle().Which.Should().Be(series);
    }

    private void ConfigureLibrary(
        IReadOnlyList<Series> series,
        IReadOnlyDictionary<string, IReadOnlyList<BaseItem>> episodesByKey)
    {
        _libraryManager.GetItemList(Arg.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Series)))
            .Returns(series.Cast<BaseItem>().ToList());
        _libraryManager.GetItemList(Arg.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Episode)))
            .Returns(call => episodesByKey[call.Arg<InternalItemsQuery>().SeriesPresentationUniqueKey!]);
    }

    private static Series Series(string name, string key)
        => new() { Id = Guid.NewGuid(), Name = name, PresentationUniqueKey = key };

    private static Episode Episode(double minutes)
        => new() { Id = Guid.NewGuid(), RunTimeTicks = (long)(minutes * TimeSpan.TicksPerMinute) };
}
