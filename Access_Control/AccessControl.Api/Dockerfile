# Use the official .NET 8 runtime image as the base for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Access_Control/AccessControl.Api/AccessControl.Api.csproj", "Access_Control/AccessControl.Api/"]
COPY ["Access_Control/AccessControl.Api.Common/AccessControl.Api.Common.csproj", "Access_Control/AccessControl.Api.Common/"]
COPY ["Access_Control/AccessControl.Api.Domain/AccessControl.Api.Domain.csproj", "Access_Control/AccessControl.Api.Domain/"]
COPY ["Access_Control/AccessControl.Api.Entities/AccessControl.Api.Features.csproj", "Access_Control/AccessControl.Api.Entities/"]
COPY ["Access_Control/AccessControl.Api.Infrastructure/AccessControl.Api.Infrastructure.csproj", "Access_Control/AccessControl.Api.Infrastructure/"]
RUN dotnet restore "Access_Control/AccessControl.Api/AccessControl.Api.csproj"

# Copy the entire source code and build the application
COPY . .
WORKDIR "/src/Access_Control/AccessControl.Api"
RUN dotnet build "AccessControl.Api.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "AccessControl.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AccessControl.Api.dll"]