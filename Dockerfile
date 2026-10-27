FROM mcr.microsoft.com/dotnet/sdk:10.0.402 AS build
WORKDIR /source
COPY . .
RUN dotnet publish src/MX.TalkWithTiles.Web/MX.TalkWithTiles.Web.csproj \
    --configuration Release --output /publish --nologo

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_EnableDiagnostics=0 \
    TALKWITHTILES_RUNTIME_FILE=/run/secrets/runtime-configuration
USER 1654:1654
EXPOSE 8080
HEALTHCHECK --interval=15s --timeout=5s --start-period=30s --retries=3 \
    CMD ["dotnet", "MX.TalkWithTiles.Web.dll", "--healthcheck"]
ENTRYPOINT ["dotnet", "MX.TalkWithTiles.Web.dll"]
