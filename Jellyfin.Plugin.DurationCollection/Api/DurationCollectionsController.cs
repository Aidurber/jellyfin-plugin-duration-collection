using System;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.DurationCollection.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.DurationCollection.Api;

[ApiController]
[Authorize(Policy = "RequiresElevation")]
[Route("DurationCollections")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class DurationCollectionsController : ControllerBase
{
    private readonly IDurationCollectionSyncService _syncService;

    public DurationCollectionsController(IDurationCollectionSyncService syncService)
    {
        _syncService = syncService;
    }

    [HttpPost("Sync")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Sync(CancellationToken cancellationToken)
    {
        await _syncService.ExecuteAsync(new Progress<double>(), cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
