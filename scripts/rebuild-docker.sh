#!/bin/bash
# Script to rebuild and restart Docker containers with new code changes

echo "🔄 Rebuilding APG Backend Docker containers..."

# Stop existing containers
echo "📦 Stopping containers..."
docker-compose down

# Rebuild images (no cache to ensure fresh build)
echo "🔨 Building new images..."
docker-compose build --no-cache

# Start containers
echo "🚀 Starting containers..."
docker-compose up -d

# Show logs
echo "📋 Container logs (Ctrl+C to exit):"
docker-compose logs -f api
