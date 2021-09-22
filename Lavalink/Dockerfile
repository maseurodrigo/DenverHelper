# syntax=docker/dockerfile:1
# lavalink:dev (zulu-openjdk:13)
FROM fredboat/lavalink:dev
WORKDIR /app

# Copy everything else and build
COPY ./Lavalink/Lavalink.jar Lavalink.jar
COPY ./Lavalink/application.yml application.yml

# Internal port expose
EXPOSE 2333

ENTRYPOINT ["java", "-Djdk.tls.client.protocols=TLSv1.1,TLSv1.2", "-Xmx4G", "-jar", "Lavalink.jar"]