using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Enums;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.DurationCollection.Services;

public sealed class DurationCollectionSyncService : IDurationCollectionSyncService
{
    private static readonly string[] ManagedTag = ["durationcollection"];
    private readonly ICollectionManager _collectionManager;
    private readonly ILibraryManager _libraryManager;
    private readonly IDurationLibraryQueryService _libraryQueryService;
    private readonly IPluginConfigurationProvider _configurationProvider;
    private readonly ILogger<DurationCollectionSyncService> _logger;

    public DurationCollectionSyncService(
        ICollectionManager collectionManager,
        ILibraryManager libraryManager,
        IDurationLibraryQueryService libraryQueryService,
        IPluginConfigurationProvider configurationProvider,
        ILogger<DurationCollectionSyncService> logger)
    {
        _collectionManager = collectionManager;
        _libraryManager = libraryManager;
        _libraryQueryService = libraryQueryService;
        _configurationProvider = configurationProvider;
        _logger = logger;
    }

    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        var configurations = _configurationProvider.GetDurationCollections()
            .Where(configuration => configuration.LibraryId != Guid.Empty)
            .ToList();
        for (var index = 0; index < configurations.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var configuration = configurations[index];

            try
            {
                await SyncCollectionAsync(configuration, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                _logger.LogError(exception, "Failed to sync duration collection {CollectionName}", configuration.Title);
            }

            progress.Report((index + 1) * 100d / configurations.Count);
        }
    }

    private async Task SyncCollectionAsync(
        Configuration.DurationCollection configuration,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.Title))
        {
            throw new ArgumentException("Collection title is required.", nameof(configuration));
        }

        var wantedItems = _libraryQueryService
            .GetItemsByDuration(configuration.LibraryId, configuration.MinMinutes, configuration.MaxMinutes)
            .ToList();
        var collection = FindCollection(configuration.Title);

        if (collection is null)
        {
            collection = await _collectionManager.CreateCollectionAsync(new CollectionCreationOptions
            {
                Name = configuration.Title,
                IsLocked = true,
            }).ConfigureAwait(false);
            collection.Tags = ManagedTag;
            await _libraryManager.UpdateItemAsync(
                collection,
                collection.GetParent(),
                ItemUpdateType.MetadataEdit,
                cancellationToken).ConfigureAwait(false);
        }

        var wantedIds = wantedItems.Select(item => item.Id).ToHashSet();
        var existingIds = collection.GetLinkedChildren().Select(item => item.Id).ToHashSet();
        var toRemove = existingIds.Except(wantedIds).ToArray();
        var toAdd = wantedIds.Except(existingIds).ToArray();

        if (toRemove.Length > 0)
        {
            await _collectionManager.RemoveFromCollectionAsync(collection.Id, toRemove).ConfigureAwait(false);
        }

        if (toAdd.Length > 0)
        {
            await _collectionManager.AddToCollectionAsync(collection.Id, toAdd).ConfigureAwait(false);
        }
    }

    private BoxSet? FindCollection(string name)
        => _libraryManager.GetItemList(new InternalItemsQuery
        {
            IncludeItemTypes = [BaseItemKind.BoxSet],
            CollapseBoxSetItems = false,
            Recursive = true,
            Tags = ManagedTag,
            Name = name,
        }).OfType<BoxSet>().FirstOrDefault();
}
