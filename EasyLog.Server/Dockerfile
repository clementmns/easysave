FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EasyLog.Server/EasyLog.Server.csproj", "EasyLog.Server/"]
RUN dotnet restore "EasyLog.Server/EasyLog.Server.csproj"
COPY . .
WORKDIR "/src/EasyLog.Server"
RUN dotnet build "./EasyLog.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EasyLog.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Create the logs directory as root and give write access to all users,
# so the non-root runtime user can write log files even when a Docker
# volume is mounted on /app/logs.
RUN mkdir -p /app/logs && chmod 777 /app/logs
USER $APP_UID
ENTRYPOINT ["dotnet", "EasyLog.Server.dll"]
