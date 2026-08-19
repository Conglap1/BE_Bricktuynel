# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copy Solution & Csproj files
COPY BrickShowcase.sln .
COPY src/BrickShowcase.Domain/BrickShowcase.Domain.csproj src/BrickShowcase.Domain/
COPY src/BrickShowcase.Application/BrickShowcase.Application.csproj src/BrickShowcase.Application/
COPY src/BrickShowcase.Infrastructure/BrickShowcase.Infrastructure.csproj src/BrickShowcase.Infrastructure/
COPY src/BrickShowcase.Api/BrickShowcase.Api.csproj src/BrickShowcase.Api/

RUN dotnet restore

# Copy all source files and publish
COPY src/ src/
WORKDIR /app/src/BrickShowcase.Api
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BrickShowcase.Api.dll"]
