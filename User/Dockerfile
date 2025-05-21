# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["User.API/User.API.csproj", "User.API/"]
COPY ["User.Api.Common/User.Api.Common.csproj", "User.Api.Common/"]
COPY ["User.Api.Domain/User.Api.Domain.csproj", "User.Api.Domain/"]
COPY ["User.Api.Entities/User.Api.Features.csproj", "User.Api.Entities/"]
COPY ["User.Api.Infrastructure.csproj", "User.Api.Infrastructure/"]
RUN dotnet restore "User.API/User.API.csproj"

# Copy the entire solution
COPY . .

# Build the application
WORKDIR "/src/User.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]