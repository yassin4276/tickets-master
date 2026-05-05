# =========================
# Build Stage
# =========================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution
COPY TicketingPlatform.slnx ./

# Copy project files first for better Docker layer caching
COPY src/Ticketing.Domain/Ticketing.Domain.csproj src/Ticketing.Domain/
COPY src/Ticketing.Application/Ticketing.Application.csproj src/Ticketing.Application/
COPY src/Ticketing.Infrastructure/Ticketing.Infrastructure.csproj src/Ticketing.Infrastructure/
COPY src/Ticketing.API/Ticketing.API.csproj src/Ticketing.API/

# Restore API project dependencies
RUN dotnet restore src/Ticketing.API/Ticketing.API.csproj

# Copy all source code
COPY . .

# Publish API
RUN dotnet publish src/Ticketing.API/Ticketing.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================
# Runtime Stage
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "Ticketing.API.dll"]