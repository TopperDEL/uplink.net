FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG UPLINK_NUGET_VERSION=2.14.3623
WORKDIR /src

COPY ./uplink.NET.Repro.csproj ./
RUN dotnet restore ./uplink.NET.Repro.csproj -p:UplinkNuGetVersion=${UPLINK_NUGET_VERSION}

COPY . ./
RUN dotnet publish ./uplink.NET.Repro.csproj \
    -c Release \
    -o /app/publish \
    -p:UplinkNuGetVersion=${UPLINK_NUGET_VERSION} \
    --no-restore

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish ./

ENTRYPOINT ["dotnet", "uplink.NET.Repro.dll"]
