FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

# Copy project files and restore
COPY Directory.Build.props ./
COPY OwlSight.sln ./
COPY src/OwlSight.Core/OwlSight.Core.csproj src/OwlSight.Core/
COPY src/OwlSight.Cli/OwlSight.Cli.csproj src/OwlSight.Cli/
RUN dotnet restore src/OwlSight.Cli/OwlSight.Cli.csproj

# Copy source and publish
COPY src/ src/
RUN dotnet publish src/OwlSight.Cli/OwlSight.Cli.csproj -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/runtime:10.0-preview
RUN apt-get update && apt-get install -y --no-install-recommends git && rm -rf /var/lib/apt/lists/*
WORKDIR /repo
COPY --from=build /app /app
ENTRYPOINT ["dotnet", "/app/OwlSight.Cli.dll"]
