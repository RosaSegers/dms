# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# Copy csproj and restore
COPY ApiGateway.csproj ./
RUN dotnet restore

# Copy the rest of the project and publish
COPY . ./
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

# Copy the published output
COPY --from=build /app/publish .

# Expose HTTP and HTTPS ports
EXPOSE 80
EXPOSE 443

# Start the application
ENTRYPOINT ["dotnet", "ApiGateway.dll"]