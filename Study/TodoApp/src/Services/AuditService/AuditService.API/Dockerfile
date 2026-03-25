FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/AuditService/AuditService.API/AuditService.API.csproj", "Services/AuditService/AuditService.API/"]
COPY ["src/Services/AuditService/AuditService.Application/AuditService.Application.csproj", "Services/AuditService/AuditService.Application/"]
COPY ["src/Services/AuditService/AuditService.Domain/AuditService.Domain.csproj", "Services/AuditService/AuditService.Domain/"]
COPY ["src/Services/AuditService/AuditService.Infrastructure/AuditService.Infrastructure.csproj", "Services/AuditService/AuditService.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj", "Shared/Shared.BuildingBlocks/"]
RUN dotnet restore "Services/AuditService/AuditService.API/AuditService.API.csproj"
COPY src/ .
RUN dotnet publish "Services/AuditService/AuditService.API/AuditService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
ENTRYPOINT ["dotnet", "AuditService.API.dll"]
