using Xunit;

namespace Jellyfin.Plugin.DurationCollection.Tests.Configuration;

public class PluginConfigurationTests
{
    [Fact]
    public void ConstructorInitializesEmptyDurationCollections()
    {
        var configuration = new global::Jellyfin.Plugin.DurationCollection.Configuration.PluginConfiguration();

        Assert.Empty(configuration.DurationCollections);
    }
}
