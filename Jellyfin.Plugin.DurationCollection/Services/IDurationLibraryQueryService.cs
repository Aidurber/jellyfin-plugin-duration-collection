using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace Jellyfin.Plugin.DurationCollection.Services;

public interface IDurationLibraryQueryService
{
    IReadOnlyList<BaseItem> GetItemsByDuration(Guid libraryId, double minMinutes, double maxMinutes);
}
