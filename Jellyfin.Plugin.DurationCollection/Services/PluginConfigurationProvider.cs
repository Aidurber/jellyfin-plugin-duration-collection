using System.Collections.Generic;

namespace Jellyfin.Plugin.DurationCollection.Services;

public sealed class PluginConfigurationProvider : IPluginConfigurationProvider
{
    public IReadOnlyList<Configuration.DurationCollection> GetDurationCollections()
        => Plugin.Instance?.Configuration.DurationCollections ?? new List<Configuration.DurationCollection>();
}
