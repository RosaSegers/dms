# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["User/User.API/User.API.csproj", "User/User.API/"]
COPY ["User/User.Api.Entities/User.Api.Features.csproj", "User/User.Api.Entities/"]
COPY ["User/User.Api.Infrastructure/User.Api.Infrastructure.csproj", "User/User.Api.Infrastructure/"]
COPY ["User/User.Api.Domain/User.Api.Domain.csproj", "User/User.Api.Domain/"]
COPY ["User/User.Api.Common/User.Api.Common.csproj", "User/User.Api.Common/"]
RUN dotnet restore "./User/User.API/User.API.csproj"
COPY . .
WORKDIR "/src/User/User.API"
RUN dotnet build "./User.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./User.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]