using System;

namespace Jellyfin.Plugin.DurationCollection.Configuration;

public class DurationCollection
{
    public DurationCollection()
        : this("Short Shows", 0, 20)
    {
    }

    public DurationCollection(
        string title, double minMinutes, double maxMinutes, Guid libraryId = default)
    {
        Title = title;
        MinMinutes = minMinutes;
        MaxMinutes = maxMinutes;
        LibraryId = libraryId;
    }

    public Guid LibraryId { get; set; }

    public string Title { get; set; }

    public double MinMinutes { get; set; }

    public double MaxMinutes { get; set; }
}
