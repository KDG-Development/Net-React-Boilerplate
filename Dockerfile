#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

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
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "KDG.Boilerplate.Server.dll"]

# Generate dev HTTPS cert using SDK (to reuse in APM stage)
FROM build AS certs
WORKDIR "/src/KDG.Boilerplate.Server"
RUN mkdir -p /root/.aspnet/https/ && \
    dotnet dev-certs https -ep /root/.aspnet/https/aspnetapp.pfx -p password && \
    dotnet dev-certs https --trust

# Optional APM-enabled final stage (build with --target final-apm)
FROM base AS final-apm
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/appsettings*.json .
COPY --from=publish /src/Migrations/scripts ./scripts

# Install Site24x7 APM Insight .NET Core agent (Linux)
RUN apt-get update && \
    apt-get install -y wget ca-certificates unzip && \
    wget https://staticdownloads.site24x7.com/apminsight/agents/dotnet/apminsight-dotnetcoreagent-linux.sh && \
    chmod +x apminsight-dotnetcoreagent-linux.sh && \
    ./apminsight-dotnetcoreagent-linux.sh -Destination "/opt/apminsight/dotnet" -LicenseKey "REPLACE_AT_RUNTIME" && \
    rm apminsight-dotnetcoreagent-linux.sh && \
    rm -rf /var/lib/apt/lists/*

# Required env vars for the agent
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER="{9D363A5F-ED5F-4AAC-B456-75AFFA6AA0C8}"
ENV CORECLR_SITE24X7_HOME="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent"
ENV CORECLR_PROFILER_PATH_64="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/x64/libClrProfilerAgent.so"
ENV CORECLR_PROFILER_PATH_32="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/x86/libClrProfilerAgent.so"
ENV DOTNET_STARTUP_HOOKS="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/netstandard2.0/DotNetAgent.Loader.dll"
ENV MANAGEENGINE_COMMUNICATION_MODE="direct"

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Ensure agent always reads license from environment by removing any stale conf on start
RUN printf '#!/bin/sh\nfind /opt/apminsight/dotnet/ApmInsightDotNetCoreAgent -type f -name apminsight.conf -delete\nexec dotnet KDG.Boilerplate.Server.dll\n' > /apm-entrypoint.sh && chmod +x /apm-entrypoint.sh

ENTRYPOINT ["/apm-entrypoint.sh"]

FROM base AS final-apm-local
WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=publish /src/appsettings*.json .
COPY --from=publish /src/Migrations/scripts ./scripts

# Install Site24x7 APM Insight .NET Core agent (Linux)
RUN apt-get update && \
    apt-get install -y wget ca-certificates unzip && \
    wget https://staticdownloads.site24x7.com/apminsight/agents/dotnet/apminsight-dotnetcoreagent-linux.sh && \
    chmod +x apminsight-dotnetcoreagent-linux.sh && \
    ./apminsight-dotnetcoreagent-linux.sh -Destination "/opt/apminsight/dotnet" -LicenseKey "REPLACE_AT_RUNTIME" && \
    rm apminsight-dotnetcoreagent-linux.sh && \
    rm -rf /var/lib/apt/lists/*

# Required env vars for the agent
ENV CORECLR_ENABLE_PROFILING=1
ENV CORECLR_PROFILER="{9D363A5F-ED5F-4AAC-B456-75AFFA6AA0C8}"
ENV CORECLR_SITE24X7_HOME="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent"
ENV CORECLR_PROFILER_PATH_64="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/x64/libClrProfilerAgent.so"
ENV CORECLR_PROFILER_PATH_32="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/x86/libClrProfilerAgent.so"
ENV DOTNET_STARTUP_HOOKS="/opt/apminsight/dotnet/ApmInsightDotNetCoreAgent/netstandard2.0/DotNetAgent.Loader.dll"
ENV MANAGEENGINE_COMMUNICATION_MODE="direct"

# Enable HTTPS for local APM testing using dev cert
COPY --from=certs /root/.aspnet/https/aspnetapp.pfx /root/.aspnet/https/aspnetapp.pfx
ENV ASPNETCORE_URLS=https://+:5261
ENV ASPNETCORE_Kestrel__Certificates__Default__Password=password
ENV ASPNETCORE_Kestrel__Certificates__Default__Path=/root/.aspnet/https/aspnetapp.pfx
ENV ASPNETCORE_ENVIRONMENT=Production

# Ensure agent always reads license from environment by removing any stale conf on start
RUN printf '#!/bin/sh\nfind /opt/apminsight/dotnet/ApmInsightDotNetCoreAgent -type f -name apminsight.conf -delete\nexec dotnet KDG.Boilerplate.Server.dll\n' > /apm-entrypoint-apm-local.sh && chmod +x /apm-entrypoint-apm-local.sh

ENTRYPOINT ["/apm-entrypoint-apm-local.sh"]

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
