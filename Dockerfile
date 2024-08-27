FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Create a non-root user 'appuser' with a specific UID and GID
RUN groupadd -r -g 1001 appuser && \
    useradd -r -u 1001 -g appuser -s /bin/false -c "Application User" appuser

USER appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZD Article Grabber.csproj", "."]
RUN dotnet restore "./ZD Article Grabber.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./ZD Article Grabber.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
RUN dotnet publish "./ZD Article Grabber.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZD Article Grabber.dll"]
