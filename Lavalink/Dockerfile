# syntax=docker/dockerfile:1
# openjdk:13
FROM azul/zulu-openjdk:13

# Run as non-root user
RUN groupadd -g 322 lavalink && \
    useradd -r -u 322 -g lavalink lavalink
USER lavalink

WORKDIR /app

# Copy everything else and build
COPY ./Lavalink/Lavalink.jar ./Lavalink/Lavalink.jar
COPY ./Lavalink/application.yml ./Lavalink/application.yml

# Internal port expose
EXPOSE 2333

CMD ["java", "-jar", "Lavalink.jar"]