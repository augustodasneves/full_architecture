FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/PiiUpdateWorker/PiiUpdateWorker.csproj", "src/PiiUpdateWorker/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/PiiUpdateWorker/PiiUpdateWorker.csproj"
COPY . .
WORKDIR "/src/src/PiiUpdateWorker"
RUN dotnet build "PiiUpdateWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PiiUpdateWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PiiUpdateWorker.dll"]
