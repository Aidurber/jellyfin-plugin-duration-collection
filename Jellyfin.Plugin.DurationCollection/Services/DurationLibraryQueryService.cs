using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.DurationCollection.Services;

public sealed class DurationLibraryQueryService : IDurationLibraryQueryService
{
    private readonly ILibraryManager _libraryManager;

    public DurationLibraryQueryService(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    public IReadOnlyList<BaseItem> GetItemsByDuration(Guid libraryId, double minMinutes, double maxMinutes)
    {
        if (minMinutes < 0 || maxMinutes < minMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(minMinutes), "Duration range must be non-negative and ordered.");
        }

        var library = _libraryManager.GetItemById<CollectionFolder>(libraryId);
        return library?.CollectionType switch
        {
            CollectionType.movies => GetMoviesByDuration(library, minMinutes, maxMinutes),
            CollectionType.tvshows => GetSeriesByAverageEpisodeDuration(library, minMinutes, maxMinutes),
            _ => throw new ArgumentException("The selected library must contain movies or TV shows.", nameof(libraryId)),
        };
    }

    private IReadOnlyList<BaseItem> GetMoviesByDuration(
        CollectionFolder library,
        double minMinutes,
        double maxMinutes)
    {
        return _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Movie],
            IsVirtualItem = false,
            Recursive = true,
            EnableTotalRecordCount = false,
            DtoOptions = new DtoOptions(false) { EnableImages = false },
        }, [library]).OfType<Movie>()
            .Where(movie => movie.RunTimeTicks is > 0)
            .Where(movie =>
            {
                var minutes = movie.RunTimeTicks!.Value / (double)TimeSpan.TicksPerMinute;
                return minutes >= minMinutes && minutes <= maxMinutes;
            })
            .ToList();
    }

    private IReadOnlyList<Series> GetSeriesByAverageEpisodeDuration(
        CollectionFolder library,
        double minMinutes,
        double maxMinutes)
    {
        var allSeries = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Series],
            IsVirtualItem = false,
            Recursive = true,
            EnableTotalRecordCount = false,
            DtoOptions = new DtoOptions(false) { EnableImages = false },
        }, [library]).OfType<Series>();

        return allSeries.Where(series => IsWithinRange(series, minMinutes, maxMinutes)).ToList();
    }

    private bool IsWithinRange(Series series, double minMinutes, double maxMinutes)
    {
        var runtimes = _libraryManager.GetItemList(new InternalItemsQuery
        {
            AncestorWithPresentationUniqueKey = null,
            SeriesPresentationUniqueKey = series.GetPresentationUniqueKey(),
            IncludeItemTypes = [BaseItemKind.Episode],
            IsVirtualItem = false,
            IsMissing = false,
            EnableTotalRecordCount = false,
            DtoOptions = new DtoOptions(false) { EnableImages = false },
        }).OfType<Episode>()
            .Select(episode => episode.RunTimeTicks)
            .Where(ticks => ticks is > 0)
            .Select(ticks => ticks!.Value / (double)TimeSpan.TicksPerMinute)
            .ToList();

        if (runtimes.Count == 0)
        {
            return false;
        }

        var averageMinutes = runtimes.Average();
        return averageMinutes >= minMinutes && averageMinutes <= maxMinutes;
    }
}
