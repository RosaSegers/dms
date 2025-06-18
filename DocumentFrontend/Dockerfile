# Base image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Build and restore the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy and restore the project
COPY DocumentFrontend.csproj ./
RUN dotnet restore "DocumentFrontend.csproj"

# Copy all source
COPY . .

# Build the application
WORKDIR /src
RUN dotnet build "DocumentFrontend.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "DocumentFrontend.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Use environment variable for Gateway config
ENV Gateway="http://localhost:5285"

ENTRYPOINT ["dotnet", "DocumentFrontend.dll"]
