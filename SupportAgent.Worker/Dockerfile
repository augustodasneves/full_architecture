FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SupportAgent.Worker/SupportAgent.Worker.csproj", "SupportAgent.Worker/"]
COPY ["SupportAgent.Application/SupportAgent.Application.csproj", "SupportAgent.Application/"]
COPY ["SupportAgent.Domain/SupportAgent.Domain.csproj", "SupportAgent.Domain/"]
COPY ["SupportAgent.Infrastructure/SupportAgent.Infrastructure.csproj", "SupportAgent.Infrastructure/"]
RUN dotnet restore "SupportAgent.Worker/SupportAgent.Worker.csproj"
COPY . .
WORKDIR "/src/SupportAgent.Worker"
RUN dotnet build "SupportAgent.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SupportAgent.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportAgent.Worker.dll"]
