FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG UPLINK_NUGET_VERSION=2.14.3736
WORKDIR /src

COPY ./uplink.NET.Repro.csproj ./
RUN dotnet restore ./uplink.NET.Repro.csproj --property:UplinkNuGetVersion=${UPLINK_NUGET_VERSION}

COPY . ./
RUN dotnet publish ./uplink.NET.Repro.csproj \
    -c Release \
    -o /app/publish \
    --property:UplinkNuGetVersion=${UPLINK_NUGET_VERSION} \
    --no-restore
RUN override="/src/docker-overrides/storj_uplink.so"; \
    target="/app/publish/runtimes/linux-x64/native/storj_uplink.so"; \
    if [ -f "$override" ]; then \
        echo "Injecting local storj_uplink.so override from $override"; \
        mkdir -p "$(dirname "$target")"; \
        cp "$override" "$target"; \
    else \
        echo "No local storj_uplink.so override found at $override; using NuGet-provided native library."; \
    fi

FROM mcr.microsoft.com/dotnet/runtime:9.0
RUN apt-get update \
    && apt-get install -y --no-install-recommends gdb \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "uplink.NET.Repro.dll"]
