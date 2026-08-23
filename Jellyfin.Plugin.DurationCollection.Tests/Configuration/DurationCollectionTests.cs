using System;
using Xunit;

namespace Jellyfin.Plugin.DurationCollection.Tests.Configuration;

public class DurationCollectionTests
{
    [Fact]
    public void ConstructorUsesShortShowsDefaults()
    {
        var collection = new global::Jellyfin.Plugin.DurationCollection.Configuration.DurationCollection();

        Assert.Equal("Short Shows", collection.Title);
        Assert.Equal(0, collection.MinMinutes);
        Assert.Equal(20, collection.MaxMinutes);
    }

    [Fact]
    public void ConstructorStoresProvidedValues()
    {
        var libraryId = Guid.NewGuid();
        var collection = new global::Jellyfin.Plugin.DurationCollection.Configuration.DurationCollection(
            "Feature Length",
            45,
            90,
            libraryId);

        Assert.Equal("Feature Length", collection.Title);
        Assert.Equal(45, collection.MinMinutes);
        Assert.Equal(90, collection.MaxMinutes);
        Assert.Equal(libraryId, collection.LibraryId);
    }
}
