FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["NotificationService.DatabaseMigrator/NotificationService.DatabaseMigrator.csproj", "NotificationService.DatabaseMigrator/"]
RUN dotnet restore "NotificationService.DatabaseMigrator/NotificationService.DatabaseMigrator.csproj"
COPY . .
WORKDIR "/src/NotificationService.DatabaseMigrator"
RUN dotnet build "./NotificationService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./NotificationService.DatabaseMigrator.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.DatabaseMigrator.dll"]