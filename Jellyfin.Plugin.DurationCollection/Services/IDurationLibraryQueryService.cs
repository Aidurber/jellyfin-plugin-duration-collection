using System.Collections.Generic;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.DurationCollection.Services;

public interface IDurationLibraryQueryService
{
    IReadOnlyList<Series> GetSeriesByAverageEpisodeDuration(double minMinutes, double maxMinutes);
}
