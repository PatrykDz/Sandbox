FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/NotificationService/NotificationService.Worker/NotificationService.Worker.csproj", "Services/NotificationService/NotificationService.Worker/"]
COPY ["src/Services/NotificationService/NotificationService.Application/NotificationService.Application.csproj", "Services/NotificationService/NotificationService.Application/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]

RUN dotnet restore "Services/NotificationService/NotificationService.Worker/NotificationService.Worker.csproj"

COPY src/ .
RUN dotnet build "Services/NotificationService/NotificationService.Worker/NotificationService.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Services/NotificationService/NotificationService.Worker/NotificationService.Worker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "NotificationService.Worker.dll"]
