FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EasySave.LogServer/EasySave.LogServer.csproj", "EasySave.LogServer/"]
RUN dotnet restore "EasySave.LogServer/EasySave.LogServer.csproj"
COPY . .
WORKDIR "/src/EasySave.LogServer"
RUN dotnet build "./EasySave.LogServer.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./EasySave.LogServer.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EasySave.LogServer.dll"]
