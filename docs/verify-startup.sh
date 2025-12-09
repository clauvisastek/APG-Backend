#!/bin/bash

# Startup Verification Script
# This script verifies that all services are running correctly

echo "🔍 APG Backend - Startup Verification"
echo "======================================"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if Docker is running
echo "1. Checking Docker..."
if ! docker info > /dev/null 2>&1; then
    echo -e "${RED}❌ Docker is not running${NC}"
    echo "   Please start Docker Desktop and try again"
    exit 1
else
    echo -e "${GREEN}✅ Docker is running${NC}"
fi
echo ""

# Check if docker-compose.yml exists
echo "2. Checking docker-compose.yml..."
if [ ! -f "../docker-compose.yml" ]; then
    echo -e "${RED}❌ docker-compose.yml not found${NC}"
    echo "   Please run this script from the APG_Backend/docs directory"
    exit 1
else
    echo -e "${GREEN}✅ docker-compose.yml found${NC}"
fi
echo ""

# Check if containers are running
echo "3. Checking containers..."
SQLSERVER_RUNNING=$(docker compose ps sqlserver | grep -c "Up")
API_RUNNING=$(docker compose ps api | grep -c "Up")

if [ "$SQLSERVER_RUNNING" -eq 0 ]; then
    echo -e "${RED}❌ SQL Server container is not running${NC}"
    echo "   Run: docker compose up sqlserver -d"
else
    echo -e "${GREEN}✅ SQL Server container is running${NC}"
fi

if [ "$API_RUNNING" -eq 0 ]; then
    echo -e "${YELLOW}⚠️  API container is not running${NC}"
    echo "   This is OK if you're running the API locally"
else
    echo -e "${GREEN}✅ API container is running${NC}"
fi
echo ""

# Check SQL Server connectivity
echo "4. Checking SQL Server connectivity..."
sleep 2
if docker compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT 1" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ SQL Server is accepting connections${NC}"
else
    echo -e "${RED}❌ Cannot connect to SQL Server${NC}"
    echo "   SQL Server might still be initializing. Wait 30 seconds and try again."
fi
echo ""

# Check if database exists
echo "5. Checking if APGDb database exists..."
DB_EXISTS=$(docker compose exec -T sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "YourStrong@Passw0rd" -Q "SELECT name FROM sys.databases WHERE name = 'APGDb'" -h -1 2>/dev/null | grep -c "APGDb")

if [ "$DB_EXISTS" -gt 0 ]; then
    echo -e "${GREEN}✅ APGDb database exists${NC}"
else
    echo -e "${YELLOW}⚠️  APGDb database does not exist yet${NC}"
    echo "   It will be created when you first run the API"
fi
echo ""

# Check API health endpoint
echo "6. Checking API health endpoint..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:5000/health 2>/dev/null)

if [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ API health check passed (HTTP $HTTP_CODE)${NC}"
    echo "   API is running at: http://localhost:5000"
elif [ "$HTTP_CODE" = "000" ]; then
    echo -e "${YELLOW}⚠️  API is not responding${NC}"
    echo "   Run: docker compose up api -d"
    echo "   Or run locally: cd APG_Backend/src/APG.API && dotnet run"
else
    echo -e "${RED}❌ API health check failed (HTTP $HTTP_CODE)${NC}"
fi
echo ""

# Check if ports are available
echo "7. Checking port availability..."
if lsof -Pi :1433 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo -e "${GREEN}✅ Port 1433 (SQL Server) is in use${NC}"
else
    echo -e "${YELLOW}⚠️  Port 1433 is not in use${NC}"
    echo "   SQL Server container might not be running"
fi

if lsof -Pi :5000 -sTCP:LISTEN -t >/dev/null 2>&1; then
    echo -e "${GREEN}✅ Port 5000 (API) is in use${NC}"
else
    echo -e "${YELLOW}⚠️  Port 5000 is not in use${NC}"
    echo "   API container might not be running"
fi
echo ""

# Summary
echo "======================================"
echo "📋 Summary"
echo "======================================"

if [ "$SQLSERVER_RUNNING" -gt 0 ] && [ "$HTTP_CODE" = "200" ]; then
    echo -e "${GREEN}✅ All systems operational!${NC}"
    echo ""
    echo "🚀 Quick Links:"
    echo "   - Swagger UI: http://localhost:5000/swagger"
    echo "   - Health Check: http://localhost:5000/health"
    echo "   - SQL Server: localhost,1433 (sa / YourStrong@Passw0rd)"
    echo ""
    echo "📚 Next Steps:"
    echo "   1. Open Swagger UI to test the API"
    echo "   2. Connect Visual Studio to the database"
    echo "   3. Start developing your features!"
elif [ "$SQLSERVER_RUNNING" -gt 0 ]; then
    echo -e "${YELLOW}⚠️  SQL Server is running, but API needs to be started${NC}"
    echo ""
    echo "To start the API:"
    echo "   docker compose up api -d"
    echo "Or run locally:"
    echo "   cd APG_Backend/src/APG.API && dotnet run"
else
    echo -e "${RED}❌ Services need to be started${NC}"
    echo ""
    echo "To start all services:"
    echo "   docker compose up -d"
    echo ""
    echo "Then wait 30 seconds and run this script again to verify"
fi

echo ""
echo "For more information, see:"
echo "  - QUICKSTART.md"
echo "  - APG_Backend/README_DB.md"
echo ""
