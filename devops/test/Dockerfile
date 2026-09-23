FROM ubuntu:noble
WORKDIR /app
USER root

RUN cat /etc/os-release

RUN apt update --fix-missing
RUN apt install -y apt-transport-https wget ca-certificates iputils-ping telnet curl gnupg2 software-properties-common

# Configure time
ARG DEBIAN_FRONTEND=noninteractive
ENV TZ=Etc/UTC
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone
RUN apt-get install -y tzdata lsb-release

# Install .Net Core 7.0
RUN add-apt-repository ppa:dotnet/backports

# Install packages
RUN apt update 
RUN apt-get install -y net-tools netcat-traditional sudo
#RUN apt-get default-jre
RUN apt-get install -y postgresql postgresql-contrib
RUN apt-get install -y dotnet-sdk-10.0 

# Configure Postgres
ENV PGTZ=Etc/UTC

# Create dirs
RUN mkdir /tmp/test

ENV ASPNETCORE_URLS = "http://+:80" 
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true  
ENV DOTNET_CLI_HOME=/tmp/DOTNET_CLI_HOME
ENV HOME=/tmp

# Prepare project files
COPY . /app

ENTRYPOINT ["tail", "-f", "/dev/null"]
