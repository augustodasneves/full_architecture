FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/UserAccountApi/UserAccountApi.csproj", "src/UserAccountApi/"]
COPY ["src/Shared/Shared.csproj", "src/Shared/"]
RUN dotnet restore "src/UserAccountApi/UserAccountApi.csproj"
COPY . .
WORKDIR "/src/src/UserAccountApi"
RUN dotnet build "UserAccountApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserAccountApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserAccountApi.dll"]
