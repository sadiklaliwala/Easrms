FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update && \
    apt-get install -y libkrb5-3 libgssapi-krb5-2 && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Easrms-backend.slnx", "."]
COPY ["Easrms.API/Easrms.API.csproj", "Easrms.API/"]
COPY ["Easrms.Application/Easrms.Application.csproj", "Easrms.Application/"]
COPY ["Easrms.Domain/Easrms.Domain.csproj", "Easrms.Domain/"]
COPY ["Easrms.Infrastructure/Easrms.Infrastructure.csproj", "Easrms.Infrastructure/"]
COPY ["Easrms.Common/Easrms.Common.csproj", "Easrms.Common/"]

RUN dotnet restore "Easrms.API/Easrms.API.csproj"

COPY . .

RUN dotnet publish "Easrms.API/Easrms.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "Easrms.API.dll"]
