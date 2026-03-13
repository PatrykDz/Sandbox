FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["src/Services/TodoService/TodoService.API/TodoService.API.csproj", "Services/TodoService/TodoService.API/"]
COPY ["src/Services/TodoService/TodoService.Application/TodoService.Application.csproj", "Services/TodoService/TodoService.Application/"]
COPY ["src/Services/TodoService/TodoService.Domain/TodoService.Domain.csproj", "Services/TodoService/TodoService.Domain/"]
COPY ["src/Services/TodoService/TodoService.Infrastructure/TodoService.Infrastructure.csproj", "Services/TodoService/TodoService.Infrastructure/"]
COPY ["src/Shared/Shared.Contracts/Shared.Contracts.csproj", "Shared/Shared.Contracts/"]
COPY ["src/Shared/Shared.BuildingBlocks/Shared.BuildingBlocks.csproj", "Shared/Shared.BuildingBlocks/"]

RUN dotnet restore "Services/TodoService/TodoService.API/TodoService.API.csproj"

COPY src/ .
RUN dotnet build "Services/TodoService/TodoService.API/TodoService.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Services/TodoService/TodoService.API/TodoService.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "TodoService.API.dll"]
