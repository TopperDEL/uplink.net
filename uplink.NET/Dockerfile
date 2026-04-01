FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish /src/uplink.NET.TestServer/uplink.NET.TestServer.csproj -c Release -o /app/server
RUN dotnet build /src/uplink.NET.Test/uplink.NET.Test.csproj -c Release --no-restore

FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /app
ENV ASPNETCORE_ENVIRONMENT=Production     ASPNETCORE_URLS=http://0.0.0.0:8080     UPLINK_TEST_ASSEMBLY_PATH=/app/tests/uplink.NET.Test.dll
COPY --from=build /app/server ./server
COPY --from=build /src/uplink.NET.Test/bin/Release/net8.0 ./tests
EXPOSE 8080
ENTRYPOINT ["dotnet", "/app/server/uplink.NET.TestServer.dll"]
