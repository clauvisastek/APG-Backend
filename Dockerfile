# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["APG_Backend.sln", "./"]
COPY ["src/APG.API/APG.API.csproj", "src/APG.API/"]
COPY ["src/APG.Domain/APG.Domain.csproj", "src/APG.Domain/"]
COPY ["src/APG.Application/APG.Application.csproj", "src/APG.Application/"]
COPY ["src/APG.Persistence/APG.Persistence.csproj", "src/APG.Persistence/"]

# Restore dependencies
RUN dotnet restore "APG_Backend.sln"

# Copy the rest of the source code
COPY . .

# Build the application
WORKDIR /src/src/APG.API
RUN dotnet build "APG.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "APG.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install SQL Server tools (optional - for debugging)
USER root
RUN apt-get update && \
    apt-get install -y curl gnupg2 && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/debian/11/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y mssql-tools unixodbc-dev && \
    echo 'export PATH="$PATH:/opt/mssql-tools/bin"' >> ~/.bashrc && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Create a non-root user for the application
RUN groupadd -r appuser && useradd -r -g appuser appuser
RUN chown -R appuser:appuser /app

USER appuser

# Copy published application
COPY --from=publish --chown=appuser:appuser /app/publish .

# Expose port
EXPOSE 80

# Set entry point
ENTRYPOINT ["dotnet", "APG.API.dll"]
