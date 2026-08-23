using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Jellyfin.Plugin.DurationCollection.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.DurationCollection.Tests.Services;

public class DurationLibraryQueryServiceTests
{
    private readonly ILibraryManager _libraryManager = Substitute.For<ILibraryManager>();
    private readonly Guid _libraryId = Guid.NewGuid();

    [Fact]
    public void GetItemsByDuration_ResolvesSelectedMovieLibraryAndFiltersByRuntime()
    {
        var libraryId = Guid.NewGuid();
        var shortMovie = new Movie { Id = Guid.NewGuid(), RunTimeTicks = TimeSpan.FromMinutes(20).Ticks };
        var longMovie = new Movie { Id = Guid.NewGuid(), RunTimeTicks = TimeSpan.FromMinutes(21).Ticks };
        var unknownMovie = new Movie { Id = Guid.NewGuid() };
        var library = Library(libraryId, Jellyfin.Data.Enums.CollectionType.movies);
        _libraryManager.GetItemById<CollectionFolder>(libraryId).Returns(library);
        _libraryManager.GetItemList(
                Arg.Is<InternalItemsQuery>(query =>
                    query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Movie)),
                Arg.Is<List<BaseItem>>(parents => parents.Count == 1 && parents[0] == library))
            .Returns([shortMovie, longMovie, unknownMovie]);

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetItemsByDuration(libraryId, 0, 20);

        result.Should().Equal(shortMovie);
    }

    [Fact]
    public void GetItemsByDuration_UsesAverageEpisodeRuntimeForSelectedTvLibrary()
    {
        var libraryId = Guid.NewGuid();
        var series = Series("short", "short-key");
        var library = Library(libraryId, Jellyfin.Data.Enums.CollectionType.tvshows);
        _libraryManager.GetItemById<CollectionFolder>(libraryId).Returns(library);
        _libraryManager.GetItemList(
                Arg.Is<InternalItemsQuery>(query =>
                    query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Series)),
                Arg.Is<List<BaseItem>>(parents => parents.Count == 1 && parents[0] == library))
            .Returns([series]);
        _libraryManager.GetItemList(Arg.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Episode)))
            .Returns([Episode(10), Episode(20)]);

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetItemsByDuration(libraryId, 15, 15);

        result.Should().Equal(series);
    }

    [Fact]
    public void GetItemsByDuration_IncludesInclusiveAverageEpisodeBoundaries()
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
            .GetItemsByDuration(_libraryId, 15, 30);

        result.Should().BeEquivalentTo([shortSeries, longSeries]);
    }

    [Fact]
    public void GetItemsByDuration_ExcludesAverageEpisodeRuntimeOutsideRange()
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
            .GetItemsByDuration(_libraryId, 15, 30);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetItemsByDuration_SkipsSeriesWithoutKnownRuntime()
    {
        var series = Series("unknown", "unknown-key");
        ConfigureLibrary(
            [series],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["unknown-key"] = [new Episode(), new Episode { RunTimeTicks = 0 }],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetItemsByDuration(_libraryId, 0, 30);

        result.Should().BeEmpty();
    }

    [Fact]
    public void GetItemsByDuration_AveragesOnlyKnownPositiveEpisodeRuntimes()
    {
        var series = Series("mixed", "mixed-key");
        ConfigureLibrary(
            [series],
            new Dictionary<string, IReadOnlyList<BaseItem>>
            {
                ["mixed-key"] = [Episode(10), Episode(20), new Episode(), new Episode { RunTimeTicks = 0 }],
            });

        var result = new DurationLibraryQueryService(_libraryManager)
            .GetItemsByDuration(_libraryId, 15, 15);

        result.Should().ContainSingle().Which.Should().Be(series);
    }

    private void ConfigureLibrary(
        IReadOnlyList<Series> series,
        IReadOnlyDictionary<string, IReadOnlyList<BaseItem>> episodesByKey)
    {
        var library = Library(_libraryId, Jellyfin.Data.Enums.CollectionType.tvshows);
        _libraryManager.GetItemById<CollectionFolder>(_libraryId).Returns(library);
        _libraryManager.GetItemList(
                Arg.Is<InternalItemsQuery>(query =>
                    query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Series)),
                Arg.Is<List<BaseItem>>(parents => parents.Count == 1 && parents[0] == library))
            .Returns(series.Cast<BaseItem>().ToList());
        _libraryManager.GetItemList(Arg.Is<InternalItemsQuery>(query =>
                query.IncludeItemTypes.Contains(Jellyfin.Data.Enums.BaseItemKind.Episode)))
            .Returns(call => episodesByKey[call.Arg<InternalItemsQuery>().SeriesPresentationUniqueKey!]);
    }

    private static Series Series(string name, string key)
        => new() { Id = Guid.NewGuid(), Name = name, PresentationUniqueKey = key };

    private static CollectionFolder Library(Guid id, Jellyfin.Data.Enums.CollectionType collectionType)
        => new() { Id = id, CollectionType = collectionType };

    private static Episode Episode(double minutes)
        => new() { Id = Guid.NewGuid(), RunTimeTicks = (long)(minutes * TimeSpan.TicksPerMinute) };
}
