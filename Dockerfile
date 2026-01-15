# Use the official Microsoft .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS staging

# multistage-build
WORKDIR /staging

# Copy the solution and project files in the root to the staging area
COPY *.sln .
COPY *.proj .

# Copy the source and test files to the staging area
COPY src src
COPY tst tst

RUN dotnet restore NEsper.sln
RUN dotnet clean --configuration Debug NEsper.sln
RUN dotnet clean --configuration Release NEsper.sln

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /build

RUN mkdir -p /build/src /build/tst

COPY --from=staging /staging/*.sln  /build/
COPY --from=staging /staging/*.proj /build/
COPY --from=staging /staging/src    /build/src/
COPY --from=staging /staging/tst    /build/tst/

RUN ls -la /build/

RUN dotnet restore NEsper.sln
RUN dotnet build --configuration Release NEsper.sln

# Run the NUnit tests
#RUN dotnet test \
#    --configuration Release \
#    --logger:"console;verbosity=detailed" \
#    --no-build \
#    NEsper.sln

# Specify the entry point for the container
ENTRYPOINT "dotnet" "test" "NEsper.sln" "--configuration" "Release" "--logger:console;verbosity=detailed" "--no-build"

