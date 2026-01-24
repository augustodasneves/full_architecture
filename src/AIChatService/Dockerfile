FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/AIChatService/AIChatService.csproj", "src/AIChatService/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/AIChatService/AIChatService.csproj"
COPY . .
WORKDIR "/src/src/AIChatService"
RUN dotnet build "AIChatService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AIChatService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AIChatService.dll"]
