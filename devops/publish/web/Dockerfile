FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

RUN apt update && apt install -y apt-transport-https wget ca-certificates curl python3 \
    && ln -sf /usr/bin/python3 /usr/bin/python \
    && rm -rf /var/lib/apt/lists/*

# Install Node.js for client-side asset build (LESS/Tailwind).
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash - \
    && apt install -y nodejs \
    && rm -rf /var/lib/apt/lists/* \
    && node -v && npm -v

# Install .NET WebAssembly AOT tools
RUN dotnet workload install wasm-tools

COPY . /app
WORKDIR /app

RUN dotnet publish PassKee.Web.Server/PassKee.Web.Server.csproj --configuration=${ASPNETCORE_ENVIRONMENT} -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV ASPNETCORE_URLS=http://+:80

WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "PassKee.Web.Server.dll"]
