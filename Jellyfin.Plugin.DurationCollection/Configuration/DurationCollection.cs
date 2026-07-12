namespace Jellyfin.Plugin.DurationCollection.Configuration;

public class DurationCollection
{
    public DurationCollection()
        : this("Short Shows", 0, 20)
    {
    }

    public DurationCollection(string title, double minMinutes, double maxMinutes)
    {
        Title = title;
        MinMinutes = minMinutes;
        MaxMinutes = maxMinutes;
    }

    public string Title { get; set; }

    public double MinMinutes { get; set; }

    public double MaxMinutes { get; set; }
}
