FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY ["BlazorChatApp/Nuget.config", "."]
COPY ["BlazorChatApp/BlazorChatApp.csproj", "BlazorChatApp/"]
RUN dotnet restore "BlazorChatApp/BlazorChatApp.csproj" --configfile Nuget.config 
COPY . .
WORKDIR "/src/BlazorChatApp"
RUN dotnet build "BlazorChatApp.csproj" -c Release -o /app/build 
RUN dotnet publish "BlazorChatApp.csproj" -c Release -o /app/publish 

FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl
COPY --from=build /app/publish .
RUN ls -la
RUN ls -la wwwroot
RUN dotnet --list-runtimes
EXPOSE 5001
# HEALTHCHECK --interval=30s --timeout=3s --start-period=10s CMD curl --fail http://localhost:5001/health || exit 1
ENTRYPOINT ["dotnet", "BlazorChatApp.dll"]