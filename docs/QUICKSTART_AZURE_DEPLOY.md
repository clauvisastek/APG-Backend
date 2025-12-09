# 🚀 Déploiement Rapide Azure

## Option 1 : Déploiement Automatisé (Recommandé)

Le script automatisé crée toutes les ressources Azure en une seule commande :

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend
./scripts/deploy-azure.sh
```

Ce script va :
- ✅ Créer le Resource Group
- ✅ Créer la base de données Azure SQL
- ✅ Créer l'App Service pour l'API
- ✅ Créer le Static Web App pour le frontend
- ✅ Configurer tous les secrets GitHub automatiquement
- ✅ Générer un fichier avec toutes les informations de déploiement

**Durée estimée : 5-10 minutes**

## Option 2 : Déploiement Manuel

Suivez le guide détaillé : [AZURE_DEPLOYMENT_GUIDE.md](./AZURE_DEPLOYMENT_GUIDE.md)

## Après le Déploiement

### 1. Mettre à jour le workflow backend

Éditer `.github/workflows/azure-deploy.yml` et remplacer :

```yaml
env:
  AZURE_WEBAPP_NAME: 'apg-backend-api'  # Remplacer par le nom généré
```

### 2. Configurer Auth0

Dans [Auth0 Dashboard](https://manage.auth0.com/), ajouter :
- Callback URL : `https://VOTRE-FRONTEND.azurestaticapps.net/callback`
- Logout URL : `https://VOTRE-FRONTEND.azurestaticapps.net`

### 3. Déployer le code

```bash
# Backend
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend
git add .
git commit -m "Configure Azure deployment"
git push origin main

# Frontend  
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Front
git add .
git commit -m "Configure Azure deployment"
git push origin main
```

### 4. Appliquer les migrations

Une fois l'API déployée :

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend

# Option A : Depuis votre machine (modifier temporairement appsettings.json)
dotnet ef database update --project src/APG.Persistence --startup-project src/APG.API

# Option B : Utiliser Azure Data Studio avec la connection string
```

## Vérification

### Backend
```bash
curl https://VOTRE-API.azurewebsites.net/api/test
```

### Frontend
Ouvrir : `https://VOTRE-FRONTEND.azurestaticapps.net`

## Dépannage

### Voir les logs backend
```bash
az webapp log tail --name VOTRE-API --resource-group apg-resources
```

### Redémarrer l'API
```bash
az webapp restart --name VOTRE-API --resource-group apg-resources
```

## Coûts Estimés

| Ressource | Coût/Mois |
|-----------|-----------|
| Azure SQL Database (Basic) | ~5 € |
| App Service (B1) | ~13 € |
| Static Web App (Free) | 0 € |
| **Total** | **~18-20 €** |

## Support

En cas de problème, consulter :
- [Guide complet de déploiement](./AZURE_DEPLOYMENT_GUIDE.md)
- [Documentation Azure](https://docs.microsoft.com/azure)
