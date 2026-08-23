using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DurationCollection.Services;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Jellyfin.Plugin.DurationCollection.Tests.Services;

public class DurationCollectionSyncServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesCollectionWithMatchingSeries()
    {
        var collectionManager = Substitute.For<ICollectionManager>();
        var libraryManager = Substitute.For<ILibraryManager>();
        var queryService = Substitute.For<IDurationLibraryQueryService>();
        var configurationProvider = Substitute.For<IPluginConfigurationProvider>();
        var logger = Substitute.For<ILogger<DurationCollectionSyncService>>();
        var libraryId = Guid.NewGuid();
        var configuration = new global::Jellyfin.Plugin.DurationCollection.Configuration.DurationCollection(
            "Short Shows",
            0,
            20,
            libraryId);
        var series = new Series { Id = Guid.NewGuid(), Name = "Short Show" };
        var boxSet = new BoxSet { Id = Guid.NewGuid(), Name = configuration.Title };
        configurationProvider.GetDurationCollections().Returns([configuration]);
        queryService.GetItemsByDuration(libraryId, 0, 20).Returns([series]);
        libraryManager.GetItemList(Arg.Any<InternalItemsQuery>()).Returns(new List<BaseItem>());
        collectionManager.CreateCollectionAsync(Arg.Is<CollectionCreationOptions>(options =>
                options.Name == configuration.Title && options.IsLocked))
            .Returns(boxSet);

        var service = new DurationCollectionSyncService(
            collectionManager,
            libraryManager,
            queryService,
            configurationProvider,
            logger);

        await service.ExecuteAsync(Substitute.For<IProgress<double>>(), CancellationToken.None);

        await collectionManager.Received(1).AddToCollectionAsync(
            boxSet.Id,
            Arg.Is<IReadOnlyList<Guid>>(ids => ids.Count == 1 && ids[0] == series.Id));
    }

    [Fact]
    public async Task ExecuteAsync_IgnoresLegacyConfigurationWithoutLibrary()
    {
        var collectionManager = Substitute.For<ICollectionManager>();
        var libraryManager = Substitute.For<ILibraryManager>();
        var queryService = Substitute.For<IDurationLibraryQueryService>();
        var configurationProvider = Substitute.For<IPluginConfigurationProvider>();
        configurationProvider.GetDurationCollections().Returns(
        [
            new global::Jellyfin.Plugin.DurationCollection.Configuration.DurationCollection("Short Shows", 0, 20),
        ]);
        var service = new DurationCollectionSyncService(
            collectionManager,
            libraryManager,
            queryService,
            configurationProvider,
            Substitute.For<ILogger<DurationCollectionSyncService>>());

        await service.ExecuteAsync(Substitute.For<IProgress<double>>(), CancellationToken.None);

        queryService.DidNotReceiveWithAnyArgs().GetItemsByDuration(default, default, default);
    }
}
