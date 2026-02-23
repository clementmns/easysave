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

## Create a non-privileged user that the app will run under.
## Ensure write permissions are enabled for the repository folder to allow log generation, otherwise the script will not work.
## See https://docs.docker.com/go/dockerfile-user-best-practices/
ARG UID=12345678
RUN adduser --uid "${UID}" appuser

# Switch to the non-privileged user to run the application.
USER appuser

# Create the logs directory as root and give ownership to the non-root user,
# so the non-root runtime user can write log files even when a Docker
# volume is mounted on /app/logs.
RUN mkdir -p /app/logs && chown appuser:appuser /app/logs



ENTRYPOINT ["dotnet", "EasyLog.Server.dll"]
