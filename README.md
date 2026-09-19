# Duration Collections

Personal Jellyfin plugin creating duration-based collections from a selected Movies or TV Shows library.

Each configured range is inclusive. Movies use their runtime. TV series use the average runtime of episodes with known positive durations; series with no known episode runtimes are skipped.

## Compatibility

Version **2.x** requires Jellyfin **12** and .NET 10. This is a breaking compatibility update; Jellyfin 10.11 users must stay on plugin **1.x**, whose releases remain in the catalog.

Current build targets Jellyfin `12.0.0`. Existing plugin identity and configuration are preserved.

## Build

Install .NET 10 SDK, then run:

```sh
make test
make publish
```

Published output is under `Jellyfin.Plugin.DurationCollection/bin/Release/net10.0/publish/`.

## Install

### Plugin catalog

1. Open `Dashboard -> Plugins -> Catalog -> Settings`.
2. Add repository URL `https://raw.githubusercontent.com/Pixel-Collectiv/jellyfin-plugin-duration-collection/main/manifest.json`.
3. Install `Duration Collections` from catalog and restart Jellyfin.
4. Open `Dashboard -> Plugins -> Duration Collections` and configure a source library and duration range for each collection.
5. Save, then use `Sync duration collections` or run scheduled task `Sync Duration Collections`.

### Manual installation

1. Create a `Duration Collections` directory inside Jellyfin's plugin directory.
2. Copy `Jellyfin.Plugin.DurationCollection.dll` from published output into it.
3. Restart Jellyfin.

Common Linux plugin root: `/var/lib/jellyfin/plugins/`.
