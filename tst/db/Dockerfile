# Use an official PostgreSQL base image from Docker Hub
FROM postgres:latest

# Set environment variables for PostgreSQL
ENV POSTGRES_DB=esper
ENV POSTGRES_USER=esper
ENV POSTGRES_PASSWORD=3sp3rP@ssw0rd

# Optionally, you can expose the PostgreSQL port (5432) if needed
EXPOSE 5432

# You can also provide additional initialization scripts by copying them into the /docker-entrypoint-initdb.d/ directory
# For example, if you have a custom initialization script named "init.sql" in the same directory as the Dockerfile, you can copy it like this:
# COPY init.sql /docker-entrypoint-initdb.d/

# CMD instruction is provided by the base image and it starts the PostgreSQL server automatically