@echo off
REM Script to rebuild and restart Docker containers with new code changes

echo Rebuilding APG Backend Docker containers...

REM Stop existing containers
echo Stopping containers...
docker-compose down

REM Rebuild images (no cache to ensure fresh build)
echo Building new images...
docker-compose build --no-cache

REM Start containers
echo Starting containers...
docker-compose up -d

REM Show logs
echo Container logs:
docker-compose logs -f api
