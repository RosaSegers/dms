# Use the official .NET 8 runtime image as the base for running the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use the official .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["Auditing/Auditing.Api/Auditing.API.csproj", "Auditing/Auditing.Api/"]
COPY ["Auditing/Auditing.Api.Common/Auditing.Api.Common.csproj", "Auditing/Auditing.Api.Common/"]
COPY ["Auditing/Auditing.Api.Domain/Auditing.Api.Domain.csproj", "Auditing/Auditing.Api.Domain/"]
COPY ["Auditing/Auditing.Api.Entities/Auditing.Api.Features.csproj", "Auditing/Auditing.Api.Entities/"]
COPY ["Auditing/Auditing.Api.Infrastructure/Auditing.Api.Infrastructure.csproj", "Auditing/Auditing.Api.Infrastructure/"]
RUN dotnet restore "Auditing/Auditing.Api/Auditing.API.csproj"

# Copy the entire source code and build the application
COPY . .
WORKDIR "/src/Auditing/Auditing.Api"
RUN dotnet build "Auditing.API.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Auditing.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the runtime image to run the application
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Auditing.API.dll"]