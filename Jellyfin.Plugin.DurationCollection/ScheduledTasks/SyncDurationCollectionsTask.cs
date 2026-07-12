using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DurationCollection.Services;
using MediaBrowser.Model.Tasks;

namespace Jellyfin.Plugin.DurationCollection.ScheduledTasks;

public sealed class SyncDurationCollectionsTask : IScheduledTask
{
    private readonly IDurationCollectionSyncService _syncService;

    public SyncDurationCollectionsTask(IDurationCollectionSyncService syncService)
    {
        _syncService = syncService;
    }

    public string Name => "Sync Duration Collections";

    public string Key => "SyncDurationCollections";

    public string Description => "Creates and updates collections based on average episode duration.";

    public string Category => "Library";

    public Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
        => _syncService.ExecuteAsync(progress, cancellationToken);

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.IntervalTrigger,
            IntervalTicks = TimeSpan.FromHours(24).Ticks,
        };
    }
}
