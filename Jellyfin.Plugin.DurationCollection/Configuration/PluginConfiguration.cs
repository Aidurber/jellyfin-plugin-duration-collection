using System.Diagnostics.CodeAnalysis;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.DurationCollection.Configuration;

public class PluginConfiguration : BasePluginConfiguration
{
    public PluginConfiguration()
    {
        DurationCollections = [];
    }

    [SuppressMessage("Usage", "CA2227:Change collection properties to read only", Justification = "Required for XML serialization")]
    public List<DurationCollection> DurationCollections { get; set; }
}
