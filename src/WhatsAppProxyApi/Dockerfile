FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/WhatsAppProxyApi/WhatsAppProxyApi.csproj", "src/WhatsAppProxyApi/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/WhatsAppProxyApi/WhatsAppProxyApi.csproj"
COPY . .
WORKDIR "/src/src/WhatsAppProxyApi"
RUN dotnet build "WhatsAppProxyApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WhatsAppProxyApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhatsAppProxyApi.dll"]
