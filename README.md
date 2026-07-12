# Duration Collections

Personal Jellyfin plugin creating collections from TV series average episode duration.

Each configured range is inclusive. Episodes without a positive known runtime are excluded from the average. Series with no known episode runtimes are skipped.

## Compatibility

Current build targets Jellyfin `10.11.11` and .NET 9. Jellyfin plugin package versions must match the installed server version. Change both Jellyfin package references and `targetAbi` before building for another server version.

## Build

Install .NET 9 SDK, then run:

```sh
make test
make publish
```

Published output is under `Jellyfin.Plugin.DurationCollection/bin/Release/net9.0/publish/`.

## Install

### Plugin catalog

1. Open `Dashboard -> Plugins -> Catalog -> Settings`.
2. Add repository URL `https://raw.githubusercontent.com/Aidurber/jellyfin-plugin-duration-collection/main/manifest.json`.
3. Install `Duration Collections` from catalog and restart Jellyfin.
4. Open `Dashboard -> Plugins -> Duration Collections` and configure duration ranges.
5. Save, then use `Sync duration collections` or run scheduled task `Sync Duration Collections`.

### Manual installation

1. Create a `Duration Collections` directory inside Jellyfin's plugin directory.
2. Copy `Jellyfin.Plugin.DurationCollection.dll` from published output into it.
3. Restart Jellyfin.

Common Linux plugin root: `/var/lib/jellyfin/plugins/`.
