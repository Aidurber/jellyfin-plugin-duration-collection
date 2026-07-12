using System;
using System.Collections.Generic;
using System.Linq;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Dto;

namespace Jellyfin.Plugin.DurationCollection.Services;

public sealed class DurationLibraryQueryService : IDurationLibraryQueryService
{
    private readonly ILibraryManager _libraryManager;

    public DurationLibraryQueryService(ILibraryManager libraryManager)
    {
        _libraryManager = libraryManager;
    }

    public IReadOnlyList<Series> GetSeriesByAverageEpisodeDuration(double minMinutes, double maxMinutes)
    {
        if (minMinutes < 0 || maxMinutes < minMinutes)
        {
            throw new ArgumentOutOfRangeException(nameof(minMinutes), "Duration range must be non-negative and ordered.");
        }

        var allSeries = _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.Series],
            IsVirtualItem = false,
            Recursive = true,
            EnableTotalRecordCount = false,
            DtoOptions = new DtoOptions(false) { EnableImages = false },
        }).OfType<Series>();

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
