FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY ["BlazorChatApp/Nuget.config", "."]
COPY BlazorChatApp/*.csproj ./BlazorChatApp/

# Restore dependencies
RUN dotnet restore BlazorChatApp/BlazorChatApp.csproj --configfile Nuget.config

# Copy the rest of the source code
COPY . .

# Build and publish the app
WORKDIR /src/BlazorChatApp
RUN dotnet publish "BlazorChatApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl
COPY --from=build /app/publish .

RUN ls -la
RUN ls -la wwwroot

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
# HEALTHCHECK --interval=30s --timeout=3s --start-period=10s CMD curl --fail http://localhost:5001/health || exit 1

ENTRYPOINT ["dotnet", "BlazorChatApp.dll"]