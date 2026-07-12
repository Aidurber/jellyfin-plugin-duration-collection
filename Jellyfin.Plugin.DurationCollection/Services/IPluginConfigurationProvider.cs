using System.Collections.Generic;
using Jellyfin.Plugin.DurationCollection.Configuration;

namespace Jellyfin.Plugin.DurationCollection.Services;

public interface IPluginConfigurationProvider
{
    IReadOnlyList<Configuration.DurationCollection> GetDurationCollections();
}
