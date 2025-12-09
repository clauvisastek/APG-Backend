#!/bin/bash

# APG Application - Azure Deployment Script
# This script automates the creation of Azure resources for the APG application

set -e  # Exit on error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║     APG Application - Azure Deployment Automation Script      ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check prerequisites
echo -e "${YELLOW}⏳ Checking prerequisites...${NC}"

if ! command -v az &> /dev/null; then
    echo -e "${RED}❌ Azure CLI is not installed. Please install it first:${NC}"
    echo "   brew install azure-cli"
    exit 1
fi

if ! command -v gh &> /dev/null; then
    echo -e "${RED}❌ GitHub CLI is not installed. Please install it first:${NC}"
    echo "   brew install gh"
    exit 1
fi

echo -e "${GREEN}✅ All prerequisites met${NC}"
echo ""

# Login to Azure
echo -e "${YELLOW}⏳ Logging in to Azure...${NC}"
az login --use-device-code
echo -e "${GREEN}✅ Logged in to Azure${NC}"
echo ""

# Select subscription
echo -e "${YELLOW}📋 Available subscriptions:${NC}"
az account list --output table
echo ""
read -p "Enter your subscription ID: " SUBSCRIPTION_ID
az account set --subscription "$SUBSCRIPTION_ID"
echo -e "${GREEN}✅ Subscription set${NC}"
echo ""

# Configuration
echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                    Resource Configuration                      ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

read -p "Resource Group Name [apg-resources]: " RESOURCE_GROUP
RESOURCE_GROUP=${RESOURCE_GROUP:-apg-resources}

echo -e "${YELLOW}Available locations:${NC}"
echo "  1) canadacentral"
echo "  2) canadaeast"
echo "  3) eastus"
echo "  4) westeurope"
read -p "Choose location (1-4) [1]: " LOCATION_CHOICE
LOCATION_CHOICE=${LOCATION_CHOICE:-1}

case $LOCATION_CHOICE in
    1) LOCATION="canadacentral";;
    2) LOCATION="canadaeast";;
    3) LOCATION="eastus";;
    4) LOCATION="westeurope";;
    *) LOCATION="canadacentral";;
esac

# Generate unique names
UNIQUE_SUFFIX=$(openssl rand -hex 3)
SQL_SERVER_NAME="apg-sqlserver-$UNIQUE_SUFFIX"
WEBAPP_NAME="apg-backend-api-$UNIQUE_SUFFIX"
STATIC_APP_NAME="apg-frontend-$UNIQUE_SUFFIX"
APP_SERVICE_PLAN="apg-backend-plan"

read -p "SQL Admin Username [apgadmin]: " SQL_ADMIN_USER
SQL_ADMIN_USER=${SQL_ADMIN_USER:-apgadmin}

# Generate secure password
SQL_ADMIN_PASSWORD="$(openssl rand -base64 24 | tr -d '=+/' | cut -c1-20)Aa1!"

SQL_DATABASE_NAME="APGDb"

echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}Configuration Summary:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo "  Resource Group:     $RESOURCE_GROUP"
echo "  Location:           $LOCATION"
echo "  SQL Server:         $SQL_SERVER_NAME"
echo "  SQL Database:       $SQL_DATABASE_NAME"
echo "  SQL Admin User:     $SQL_ADMIN_USER"
echo "  Backend App:        $WEBAPP_NAME"
echo "  Frontend App:       $STATIC_APP_NAME"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""
read -p "Continue with deployment? (yes/no): " CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo -e "${RED}❌ Deployment cancelled${NC}"
    exit 0
fi

echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║                   Starting Deployment...                       ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Step 1: Create Resource Group
echo -e "${YELLOW}⏳ [1/6] Creating Resource Group...${NC}"
az group create --name "$RESOURCE_GROUP" --location "$LOCATION" --output none
echo -e "${GREEN}✅ Resource Group created${NC}"
echo ""

# Step 2: Create SQL Server and Database
echo -e "${YELLOW}⏳ [2/6] Creating Azure SQL Database (this may take 2-3 minutes)...${NC}"

az sql server create \
  --name "$SQL_SERVER_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --admin-user "$SQL_ADMIN_USER" \
  --admin-password "$SQL_ADMIN_PASSWORD" \
  --output none

az sql server firewall-rule create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0 \
  --output none

az sql db create \
  --resource-group "$RESOURCE_GROUP" \
  --server "$SQL_SERVER_NAME" \
  --name "$SQL_DATABASE_NAME" \
  --service-objective Basic \
  --backup-storage-redundancy Local \
  --output none

echo -e "${GREEN}✅ Azure SQL Database created${NC}"
echo ""

# Step 3: Create App Service
echo -e "${YELLOW}⏳ [3/6] Creating App Service for Backend API...${NC}"

az appservice plan create \
  --name "$APP_SERVICE_PLAN" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --is-linux \
  --sku B1 \
  --output none

az webapp create \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --plan "$APP_SERVICE_PLAN" \
  --runtime "DOTNET:8.0" \
  --output none

az webapp update \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --https-only true \
  --output none

# Configure CORS
az webapp cors add \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --allowed-origins "https://$STATIC_APP_NAME.azurestaticapps.net" \
  --output none

echo -e "${GREEN}✅ App Service created${NC}"
echo ""

# Step 4: Create Static Web App
echo -e "${YELLOW}⏳ [4/6] Creating Azure Static Web App for Frontend...${NC}"

az staticwebapp create \
  --name "$STATIC_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --location "$LOCATION" \
  --sku Free \
  --output none

echo -e "${GREEN}✅ Static Web App created${NC}"
echo ""

# Step 5: Get deployment credentials
echo -e "${YELLOW}⏳ [5/6] Retrieving deployment credentials...${NC}"

# Get publish profile for backend
az webapp deployment list-publishing-profiles \
  --name "$WEBAPP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --xml > backend-publish-profile.xml

# Get deployment token for frontend
STATIC_APP_TOKEN=$(az staticwebapp secrets list \
  --name "$STATIC_APP_NAME" \
  --resource-group "$RESOURCE_GROUP" \
  --query "properties.apiKey" -o tsv)

echo -e "${GREEN}✅ Credentials retrieved${NC}"
echo ""

# Step 6: Configure GitHub Secrets
echo -e "${YELLOW}⏳ [6/6] Configuring GitHub Secrets...${NC}"

# Connection string
CONNECTION_STRING="Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DATABASE_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Backend secrets
echo -e "${YELLOW}  📦 Configuring Backend secrets...${NC}"
cd "$(dirname "$0")/.."
pwd

read -p "Enter your OpenAI API Key: " OPENAI_API_KEY

gh secret set AZURE_SQL_CONNECTION_STRING --body "$CONNECTION_STRING" --repo clauvisastek/APG-Backend
gh secret set OPENAI_API_KEY --body "$OPENAI_API_KEY" --repo clauvisastek/APG-Backend
gh secret set AZURE_WEBAPP_PUBLISH_PROFILE --body "$(cat backend-publish-profile.xml)" --repo clauvisastek/APG-Backend

# Frontend secrets
echo -e "${YELLOW}  📦 Configuring Frontend secrets...${NC}"
gh secret set VITE_AUTH0_DOMAIN --body "astekcanada.ca.auth0.com" --repo clauvisastek/APG-Frontend
gh secret set VITE_AUTH0_CLIENT_ID --body "y98drL7i1LAVnW8Hm9C743M6txLkLCHE" --repo clauvisastek/APG-Frontend
gh secret set VITE_AUTH0_AUDIENCE --body "https://api.apg-astek.com" --repo clauvisastek/APG-Frontend
gh secret set VITE_API_BASE_URL --body "https://$WEBAPP_NAME.azurewebsites.net" --repo clauvisastek/APG-Frontend
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$STATIC_APP_TOKEN" --repo clauvisastek/APG-Frontend

# Clean up
rm -f backend-publish-profile.xml

echo -e "${GREEN}✅ GitHub Secrets configured${NC}"
echo ""

# Final Summary
echo ""
echo -e "${GREEN}╔════════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║              ✅ DEPLOYMENT COMPLETED SUCCESSFULLY!              ║${NC}"
echo -e "${GREEN}╚════════════════════════════════════════════════════════════════╝${NC}"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo -e "${YELLOW}📋 Deployment Information:${NC}"
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}🔹 Resource Group:${NC}"
echo "   $RESOURCE_GROUP"
echo ""
echo -e "${YELLOW}🔹 SQL Database:${NC}"
echo "   Server:   $SQL_SERVER_NAME.database.windows.net"
echo "   Database: $SQL_DATABASE_NAME"
echo "   Username: $SQL_ADMIN_USER"
echo "   Password: $SQL_ADMIN_PASSWORD"
echo ""
echo -e "${YELLOW}🔹 Backend API:${NC}"
echo "   URL: https://$WEBAPP_NAME.azurewebsites.net"
echo ""
echo -e "${YELLOW}🔹 Frontend:${NC}"
echo "   URL: https://$STATIC_APP_NAME.azurestaticapps.net"
echo ""
echo -e "${BLUE}═══════════════════════════════════════════════════════════════${NC}"
echo ""
echo -e "${YELLOW}⚠️  IMPORTANT: Save this information securely!${NC}"
echo ""
echo -e "${YELLOW}📝 Next Steps:${NC}"
echo "   1. Update the workflow file with the correct webapp name:"
echo "      Edit .github/workflows/azure-deploy.yml"
echo "      Set AZURE_WEBAPP_NAME: '$WEBAPP_NAME'"
echo ""
echo "   2. Configure Auth0 callback URLs:"
echo "      - Allowed Callback: https://$STATIC_APP_NAME.azurestaticapps.net/callback"
echo "      - Allowed Logout:   https://$STATIC_APP_NAME.azurestaticapps.net"
echo ""
echo "   3. Push your code to trigger deployment:"
echo "      git add ."
echo "      git commit -m 'Configure Azure deployment'"
echo "      git push origin main"
echo ""
echo "   4. Run database migrations (after first deployment):"
echo "      dotnet ef database update --project src/APG.Persistence --startup-project src/APG.API"
echo ""
echo -e "${GREEN}🎉 Your application is ready to deploy!${NC}"
echo ""

# Save configuration to file
cat > azure-deployment-info.txt << EOF
APG Application - Azure Deployment Information
================================================

Deployment Date: $(date)
Resource Group: $RESOURCE_GROUP
Location: $LOCATION

SQL Server
----------
Server: $SQL_SERVER_NAME.database.windows.net
Database: $SQL_DATABASE_NAME
Username: $SQL_ADMIN_USER
Password: $SQL_ADMIN_PASSWORD

Connection String:
$CONNECTION_STRING

Backend API
-----------
URL: https://$WEBAPP_NAME.azurewebsites.net
App Name: $WEBAPP_NAME

Frontend
--------
URL: https://$STATIC_APP_NAME.azurestaticapps.net
App Name: $STATIC_APP_NAME

GitHub Repositories
-------------------
Backend: https://github.com/clauvisastek/APG-Backend
Frontend: https://github.com/clauvisastek/APG-Frontend
EOF

echo -e "${YELLOW}💾 Deployment information saved to: azure-deployment-info.txt${NC}"
echo ""
