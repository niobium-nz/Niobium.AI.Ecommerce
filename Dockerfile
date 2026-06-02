# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src


# Copy source code and restore app dependencies
COPY --link . .
RUN dotnet restore Niobium.AI.slnx -a $TARGETARCH

# Publish app
RUN dotnet publish Niobium.AI.slnx -a $TARGETARCH -c $BUILD_CONFIGURATION --no-restore -o /app


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --link --from=build /app .
ENTRYPOINT ["./Niobium.AI.Host"]
