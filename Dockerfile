#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5261

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG ENVIRONMENT=Production
ARG BUILD_CONFIGURATION=Release

# Install Node.js
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

WORKDIR /src
COPY ["KDG.Boilerplate.Server/*.csproj", "KDG.Boilerplate.Server/"]
COPY ["Migrations/*.csproj", "Migrations/"]
COPY ["kdg.boilerplate.client/*.esproj", "kdg.boilerplate.client/"]
COPY ["appsettings*.json", "./"]

RUN dotnet restore "./KDG.Boilerplate.Server/KDG.Boilerplate.Server.csproj"
COPY . .

WORKDIR "/src/kdg.boilerplate.client"
RUN npm install
RUN npm run build

WORKDIR "/src/KDG.Boilerplate.Server"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
# Remove the conflicting index.html from wwwroot before publishing
RUN rm -f /src/KDG.Boilerplate.Server/wwwroot/index.html
RUN dotnet publish "./KDG.Boilerplate.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/appsettings*.json .
COPY --from=publish /src/Migrations/scripts ./scripts
ENV ASPNETCORE_URLS=http://+:5261
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "KDG.Boilerplate.Server.dll"]

FROM build AS local
WORKDIR "/src/KDG.Boilerplate.Server"
RUN mkdir -p /root/.aspnet/https/ && \
    dotnet dev-certs https -ep /root/.aspnet/https/aspnetapp.pfx -p password && \
    dotnet dev-certs https --trust

ENV ASPNETCORE_URLS=https://+:5261
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=password
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/aspnetapp.pfx
ENV ASPNETCORE_ENVIRONMENT=Development

EXPOSE 5261
ENTRYPOINT ["dotnet","watch","run"]
