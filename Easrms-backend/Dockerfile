FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Easrms-backend.slnx", "."]
COPY ["Easrms.API/Easrms.API.csproj", "Easrms.API/"]
COPY ["Easrms.Application/Easrms.Application.csproj", "Easrms.Application/"]
COPY ["Easrms.Domain/Easrms.Domain.csproj", "Easrms.Domain/"]
COPY ["Easrms.Infrastructure/Easrms.Infrastructure.csproj", "Easrms.Infrastructure/"]
COPY ["Easrms.Common/Easrms.Common.csproj", "Easrms.Common/"]

RUN dotnet restore

COPY . .

RUN dotnet publish "Easrms.API/Easrms.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Easrms.API.dll"]