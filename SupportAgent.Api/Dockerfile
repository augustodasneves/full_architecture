FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SupportAgent.Api/SupportAgent.Api.csproj", "SupportAgent.Api/"]
COPY ["SupportAgent.Application/SupportAgent.Application.csproj", "SupportAgent.Application/"]
COPY ["SupportAgent.Domain/SupportAgent.Domain.csproj", "SupportAgent.Domain/"]
COPY ["SupportAgent.Infrastructure/SupportAgent.Infrastructure.csproj", "SupportAgent.Infrastructure/"]
RUN dotnet restore "SupportAgent.Api/SupportAgent.Api.csproj"
COPY . .
WORKDIR "/src/SupportAgent.Api"
RUN dotnet build "SupportAgent.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SupportAgent.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportAgent.Api.dll"]
