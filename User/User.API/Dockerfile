# Use the official .NET 8 SDK image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["User/User.API/User.API.csproj", "User/User.API/"]
COPY ["User/User.Api.Common/User.Api.Common.csproj", "User/User.Api.Common/"]
COPY ["User/User.Api.Domain/User.Api.Domain.csproj", "User/User.Api.Domain/"]
COPY ["User/User.Api.Entities/User.Api.Features.csproj", "User/User.Api.Entities/"]
COPY ["User/User.Api.Infrastructure/User.Api.Infrastructure.csproj", "User/User.Api.Infrastructure/"]
RUN dotnet restore "User/User.API/User.API.csproj"

# Copy the entire solution
COPY . .

# Build the application
WORKDIR "/src/User/User.API"
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "User.API.dll"]