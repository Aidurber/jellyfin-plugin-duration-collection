.PHONY: build test publish

DOTNET ?= dotnet

build:
	$(DOTNET) build Jellyfin.Plugin.DurationCollection.sln

test:
	$(DOTNET) test Jellyfin.Plugin.DurationCollection.sln

publish:
	$(DOTNET) publish Jellyfin.Plugin.DurationCollection/Jellyfin.Plugin.DurationCollection.csproj --configuration Release
