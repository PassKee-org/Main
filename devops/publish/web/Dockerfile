FROM nginx:latest AS base

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS dot_net

ARG ENVIRONMENT=Production
ENV ASPNETCORE_ENVIRONMENT=${ENVIRONMENT}
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV ASPNETCORE_URLS=http://+:80  

# Change timezone to local time
ENV TZ=UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone

COPY . /app
WORKDIR /app/PassKee.Web

RUN apt update
RUN apt install -y apt-transport-https wget ca-certificates curl

# Install Node.js
RUN apt update && apt install -y curl
RUN curl -fsSL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt install -y nodejs
RUN node -v && npm -v

RUN echo "{}" > appsettings.Local.json
RUN dotnet publish --configuration=${ASPNETCORE_ENVIRONMENT} -o "/var/www" -r linux-x64 --self-contained true /p:WasmApplicationEnvironmentName=${ASPNETCORE_ENVIRONMENT}
RUN ls /var/www/ /var/www/wwwroot/ /var/www/wwwroot/lib

FROM base
WORKDIR /usr/share/nginx/html
COPY ./devops/publish_native/web/nginx.conf /etc/nginx/nginx.conf 
COPY --from=dot_net /var/www/ /var/www/
RUN ls -a /var/www/ /var/www/wwwroot/ /var/www/wwwroot/lib
EXPOSE 80
