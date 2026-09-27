# Stage 1: Build and Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project definition files first for optimized Docker layer caching
COPY ["src/Coin.Domain/Coin.Domain.csproj", "src/Coin.Domain/"]
COPY ["src/Coin.Application/Coin.Application.csproj", "src/Coin.Application/"]
COPY ["src/Coin.Infrastructure/Coin.Infrastructure.csproj", "src/Coin.Infrastructure/"]
COPY ["src/Coin.API/Coin.API.csproj", "src/Coin.API/"]
COPY ["Coin.slnx", "./"]

RUN dotnet restore "src/Coin.API/Coin.API.csproj"

# Copy remaining source code and publish release artifacts
COPY src/ src/
WORKDIR "/src/src/Coin.API"
RUN dotnet publish "Coin.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Minimal, secure production runtime (Ubuntu Chiseled)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

COPY --from=build /app/publish .

# Run as non-root user (app UID 1654 built-in to chiseled images)
USER $APP_UID
ENTRYPOINT ["dotnet", "Coin.API.dll"]
