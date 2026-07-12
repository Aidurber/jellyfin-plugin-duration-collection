using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.DurationCollection.Services;

public interface IDurationCollectionSyncService
{
    Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken);
}
