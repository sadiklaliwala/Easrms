# Build stage
ARG CONFIGURATION=Release
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj files first for better layer caching
COPY ["Easrms.API/Easrms.API.csproj", "Easrms.API/"]
COPY ["Easrms.Application/Easrms.Application.csproj", "Easrms.Application/"]
COPY ["Easrms.Common/Easrms.Common.csproj", "Easrms.Common/"]
COPY ["Easrms.Domain/Easrms.Domain.csproj", "Easrms.Domain/"]
COPY ["Easrms.Infrastructure/Easrms.Infrastructure.csproj", "Easrms.Infrastructure/"]

RUN dotnet restore "Easrms.API/Easrms.API.csproj"

# Copy remaining sources and publish
COPY . .
WORKDIR /src/Easrms.API
RUN dotnet publish "Easrms.API.csproj" -c $CONFIGURATION -o /app/publish --no-restore \
    /p:PublishTrimmed=true /p:PublishReadyToRun=true

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Set production environment and listen on port 80
ENV ASPNETCORE_URLS=http://+:80 \
    ASPNETCORE_ENVIRONMENT=Production

# Copy published app and switch to non-root user for security
COPY --from=build /app/publish .
RUN groupadd -r app && useradd -r -g app app && chown -R app:app /app
USER app

EXPOSE 80

ENTRYPOINT ["dotnet", "Easrms.API.dll"]