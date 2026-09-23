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

# Fix error: The configured user limit (128) on the number of inotify instances has been reached
#RUN echo "fs.inotify.max_user_watches=524288" >> /etc/sysctl.conf
#RUN echo "fs.inotify.max_user_instances=524288" >> /etc/sysctl.conf
#RUN sysctl -p

# Install .Net Core 7.0
#RUN curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add -
#RUN curl https://packages.microsoft.com/config/ubuntu/20.10/prod.list | tee /etc/apt/sources.list.d/msprod.list
RUN add-apt-repository ppa:dotnet/backports

# Install Redis repositories
RUN curl -fsSL https://packages.redis.io/gpg | gpg --dearmor -o /usr/share/keyrings/redis-archive-keyring.gpg
RUN echo "deb [signed-by=/usr/share/keyrings/redis-archive-keyring.gpg] https://packages.redis.io/deb $(lsb_release -cs) main" | tee /etc/apt/sources.list.d/redis.list

# Install Postgres repositories
# Create the file repository configuration:
#RUN sh -c 'echo "deb http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list'
# Import the repository signing key:
#RUN wget --quiet -O - https://www.postgresql.org/media/keys/ACCC4CF8.asc | apt-key add -

#RUN apt install curl ca-certificates gnupg
#RUN curl -fsSL https://www.postgresql.org/media/keys/ACCC4CF8.asc | gpg --dearmor -o /usr/share/keyrings/postgresql.gpg
#RUN echo "deb [signed-by=/usr/share/keyrings/postgresql.gpg] http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" | tee /etc/apt/sources.list.d/pgdg.list

# Install packages
RUN apt update 
RUN apt-get install -y net-tools netcat-traditional sudo
#RUN apt-get default-jre
RUN apt-get install -y postgresql postgresql-contrib
RUN apt-get install -y redis
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
