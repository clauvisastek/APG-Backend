# 🚀 Guide de Déploiement Azure - APG Application

Ce guide vous accompagne étape par étape pour déployer l'application APG sur Azure.

## 📋 Prérequis

- ✅ Compte Azure actif avec une subscription
- ✅ Azure CLI installé sur votre machine
- ✅ GitHub CLI (gh) déjà configuré
- ✅ OpenAI API Key
- ✅ Auth0 configuré

## 🎯 Architecture de Déploiement

```
┌─────────────────────────────────────────────────────┐
│  Azure Static Web Apps (Frontend - React + Vite)   │
│  https://apg-frontend.azurestaticapps.net          │
└──────────────────┬──────────────────────────────────┘
                   │ HTTPS Calls
                   ▼
┌─────────────────────────────────────────────────────┐
│  Azure App Service (Backend - .NET 8 API)          │
│  https://apg-backend-api.azurewebsites.net         │
└──────────────────┬──────────────────────────────────┘
                   │ EF Core
                   ▼
┌─────────────────────────────────────────────────────┐
│  Azure SQL Database (Database)                      │
│  apg-sqlserver.database.windows.net                │
└─────────────────────────────────────────────────────┘
```

## 💰 Estimation des Coûts

| Ressource | Tier | Coût/Mois (approx) |
|-----------|------|-------------------|
| Azure SQL Database | Basic (2 GB) | ~5 € |
| App Service | B1 (1 Core, 1.75 GB RAM) | ~13 € |
| Static Web Apps | Free | 0 € |
| **Total** | | **~18-20 €/mois** |

---

## 📝 Étapes de Déploiement

### Étape 1 : Installer Azure CLI (si pas déjà fait)

```bash
brew update && brew install azure-cli
```

### Étape 2 : Se connecter à Azure

```bash
az login
```

### Étape 3 : Créer un Resource Group

```bash
# Définir les variables
RESOURCE_GROUP="apg-resources"
LOCATION="canadacentral"  # Choisir la région la plus proche

# Créer le resource group
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### Étape 4 : Créer Azure SQL Database

```bash
# Variables
SQL_SERVER_NAME="apg-sqlserver-$(openssl rand -hex 4)"  # Nom unique
SQL_ADMIN_USER="apgadmin"
SQL_ADMIN_PASSWORD="$(openssl rand -base64 32 | tr -d '=+/' | cut -c1-32)Admin1!"  # Mot de passe fort généré
SQL_DATABASE_NAME="APGDb"

# Créer le serveur SQL
az sql server create \
  --name $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user $SQL_ADMIN_USER \
  --admin-password "$SQL_ADMIN_PASSWORD"

# Configurer le firewall (autoriser les services Azure)
az sql server firewall-rule create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name AllowAzureServices \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0

# Créer la base de données (tier Basic pour commencer)
az sql db create \
  --resource-group $RESOURCE_GROUP \
  --server $SQL_SERVER_NAME \
  --name $SQL_DATABASE_NAME \
  --service-objective Basic \
  --backup-storage-redundancy Local

# Afficher la connection string (à sauvegarder)
echo "SQL Server: $SQL_SERVER_NAME.database.windows.net"
echo "Database: $SQL_DATABASE_NAME"
echo "Admin User: $SQL_ADMIN_USER"
echo "Admin Password: $SQL_ADMIN_PASSWORD"
echo ""
echo "Connection String:"
echo "Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DATABASE_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**⚠️ IMPORTANT : Sauvegarder ces informations dans un endroit sécurisé !**

### Étape 5 : Créer Azure App Service (Backend)

```bash
# Variables
APP_SERVICE_PLAN="apg-backend-plan"
WEBAPP_NAME="apg-backend-api-$(openssl rand -hex 4)"

# Créer le plan App Service (Linux, B1)
az appservice plan create \
  --name $APP_SERVICE_PLAN \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --is-linux \
  --sku B1

# Créer l'App Service
az webapp create \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan $APP_SERVICE_PLAN \
  --runtime "DOTNET|8.0"

# Activer HTTPS uniquement
az webapp update \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --https-only true

# Télécharger le profil de publication (pour GitHub Actions)
az webapp deployment list-publishing-profiles \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --xml > backend-publish-profile.xml

echo "Backend URL: https://$WEBAPP_NAME.azurewebsites.net"
echo "Publish profile saved to: backend-publish-profile.xml"
```

### Étape 6 : Créer Azure Static Web Apps (Frontend)

```bash
# Cette ressource sera créée automatiquement par GitHub Actions
# lors du premier déploiement, mais vous pouvez aussi la créer manuellement :

STATIC_APP_NAME="apg-frontend"

az staticwebapp create \
  --name $STATIC_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --source https://github.com/clauvisastek/APG-Frontend \
  --branch main \
  --app-location "/" \
  --output-location "dist" \
  --login-with-github

# Récupérer le token de déploiement
STATIC_APP_TOKEN=$(az staticwebapp secrets list \
  --name $STATIC_APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query "properties.apiKey" -o tsv)

echo "Static Web App Token: $STATIC_APP_TOKEN"
echo "Frontend URL: https://$STATIC_APP_NAME.azurestaticapps.net"
```

### Étape 7 : Configurer les GitHub Secrets

**Pour le Backend (APG-Backend) :**

```bash
# Se positionner dans le repo backend
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend

# Ajouter les secrets
gh secret set OPENAI_API_KEY --body "sk-proj-votre-clé-ici"

# Connection string SQL
gh secret set AZURE_SQL_CONNECTION_STRING --body "Server=tcp:$SQL_SERVER_NAME.database.windows.net,1433;Initial Catalog=$SQL_DATABASE_NAME;Persist Security Info=False;User ID=$SQL_ADMIN_USER;Password=$SQL_ADMIN_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Profil de publication (contenu du fichier XML)
gh secret set AZURE_WEBAPP_PUBLISH_PROFILE < backend-publish-profile.xml
```

**Pour le Frontend (APG-Frontend) :**

```bash
# Se positionner dans le repo frontend
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Front

# Ajouter les secrets Auth0
gh secret set VITE_AUTH0_DOMAIN --body "astekcanada.ca.auth0.com"
gh secret set VITE_AUTH0_CLIENT_ID --body "y98drL7i1LAVnW8Hm9C743M6txLkLCHE"
gh secret set VITE_AUTH0_AUDIENCE --body "https://api.apg-astek.com"

# URL de l'API backend
gh secret set VITE_API_BASE_URL --body "https://$WEBAPP_NAME.azurewebsites.net"

# Token Static Web App
gh secret set AZURE_STATIC_WEB_APPS_API_TOKEN --body "$STATIC_APP_TOKEN"
```

### Étape 8 : Mettre à jour les workflows GitHub Actions

Mettre à jour le nom de l'app dans le workflow backend :

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend

# Éditer .github/workflows/azure-deploy.yml
# Remplacer: AZURE_WEBAPP_NAME: 'apg-backend-api'
# Par: AZURE_WEBAPP_NAME: 'votre-webapp-name-généré'
```

### Étape 9 : Configurer Auth0 pour la production

1. Se connecter à [Auth0 Dashboard](https://manage.auth0.com/)
2. Aller dans Applications → APG Application
3. Ajouter les URLs autorisées :
   - **Allowed Callback URLs** : 
     - `https://apg-frontend.azurestaticapps.net/callback`
     - `https://*.azurestaticapps.net/callback` (pour les PR previews)
   - **Allowed Logout URLs** :
     - `https://apg-frontend.azurestaticapps.net`
   - **Allowed Web Origins** :
     - `https://apg-frontend.azurestaticapps.net`

4. Dans APIs → APG API, vérifier que l'identifier est bien : `https://api.apg-astek.com`

### Étape 10 : Configurer CORS sur le Backend

Le backend doit autoriser les requêtes depuis le frontend. Ajouter dans Azure App Service :

```bash
az webapp cors add \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --allowed-origins "https://apg-frontend.azurestaticapps.net"
```

### Étape 11 : Pousser les changements et déclencher le déploiement

```bash
# Backend
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend
git add .
git commit -m "Configure Azure deployment workflows and environment"
git push origin main

# Frontend
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Front
git add .
git commit -m "Configure Azure Static Web Apps deployment"
git push origin main
```

### Étape 12 : Appliquer les migrations de base de données

Une fois l'API déployée, appliquer les migrations :

**Option 1 : Depuis votre machine locale**

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend

# Mettre à jour appsettings.json temporairement avec la connection string Azure
# Puis exécuter :
dotnet ef database update --project src/APG.Persistence --startup-project src/APG.API
```

**Option 2 : Via Azure Cloud Shell**

```bash
# Se connecter à la base de données
az sql db show-connection-string \
  --client sqlcmd \
  --name $SQL_DATABASE_NAME \
  --server $SQL_SERVER_NAME

# Utiliser Azure Data Studio ou SQL Server Management Studio
# pour se connecter et exécuter les scripts de migration dans le dossier migrations/
```

---

## 🧪 Vérification du Déploiement

### 1. Vérifier le Backend

```bash
# Test de santé
curl https://$WEBAPP_NAME.azurewebsites.net/api/test

# Test de l'API (nécessite un token Auth0)
curl https://$WEBAPP_NAME.azurewebsites.net/api/clients
```

### 2. Vérifier le Frontend

Ouvrir dans le navigateur : `https://apg-frontend.azurestaticapps.net`

### 3. Vérifier la base de données

```bash
# Connexion à la base de données
az sql db show \
  --name $SQL_DATABASE_NAME \
  --server $SQL_SERVER_NAME \
  --resource-group $RESOURCE_GROUP
```

---

## 🔧 Maintenance et Monitoring

### Logs Backend

```bash
# Voir les logs en temps réel
az webapp log tail \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP
```

### Logs Frontend

Les logs sont disponibles dans le portail Azure → Static Web Apps → Logs

### Redémarrer l'App Service

```bash
az webapp restart \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP
```

---

## 🚨 Dépannage

### Problème : 500 Internal Server Error

1. Vérifier les logs : `az webapp log tail --name $WEBAPP_NAME --resource-group $RESOURCE_GROUP`
2. Vérifier la connection string dans les App Settings
3. Vérifier que les migrations sont appliquées

### Problème : CORS Error

```bash
az webapp cors add \
  --name $WEBAPP_NAME \
  --resource-group $RESOURCE_GROUP \
  --allowed-origins "https://apg-frontend.azurestaticapps.net"
```

### Problème : Auth0 Login Failed

1. Vérifier que les URLs sont bien configurées dans Auth0
2. Vérifier que le domaine et client ID sont corrects dans les secrets GitHub

---

## 💡 Commandes Utiles

```bash
# Lister toutes les ressources
az resource list --resource-group $RESOURCE_GROUP --output table

# Voir les coûts actuels
az consumption usage list --output table

# Supprimer tout le resource group (⚠️ DANGER)
az group delete --name $RESOURCE_GROUP --yes --no-wait
```

---

## 📚 Ressources

- [Documentation Azure App Service](https://docs.microsoft.com/azure/app-service/)
- [Documentation Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/)
- [Documentation Azure SQL Database](https://docs.microsoft.com/azure/azure-sql/)
- [GitHub Actions for Azure](https://github.com/Azure/actions)

---

**✅ Une fois toutes ces étapes complétées, votre application sera en production !**
