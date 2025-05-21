# Use the official .NET 8 runtime image as the base for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Organization/Organization.Api/Organization.API.csproj", "Organization/Organization.API/"]
COPY ["Organization/Organization.Api.Common/Organization.Api.Common.csproj", "Organization/Organization.Api.Common/"]
COPY ["Organization/Organization.Api.Domain/Organization.Api.Domain.csproj", "Organization/Organization.Api.Domain/"]
COPY ["Organization/Organization.Api.Entities/Organization.Api.Features.csproj", "Organization/Organization.Api.Entities/"]
COPY ["Organization/Organization.Api.Infrastructure/Organization.Api.Infrastructure.csproj", "Organization/Organization.Api.Infrastructure/"]
RUN dotnet restore "Organization/Organization.Api/Organization.Api.csproj"

# Copy the entire source code and build the application
COPY . .
WORKDIR "/src/Organization/Organization.API"
RUN dotnet build "Organization.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Organization.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Organization.API.dll"]