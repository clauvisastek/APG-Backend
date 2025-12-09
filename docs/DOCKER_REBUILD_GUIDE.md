# Guide: Reconstruire le Container Docker avec Market Trends

## ⚠️ Problème
Les modifications du code source ne sont pas visibles dans le container Docker car celui-ci utilise une ancienne version.

## ✅ Solution: Reconstruire le Container

### Étape 1: Configurer la clé API OpenAI

Éditez le fichier `.env` à la racine du projet:

```bash
nano .env
```

Ajoutez votre clé API OpenAI:
```
OPENAI_API_KEY=sk-votre-clé-api-ici
```

### Étape 2: Reconstruire et Redémarrer

**Option A - Script automatique (Recommandé):**

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend
./scripts/rebuild-docker.sh
```

**Option B - Commandes manuelles:**

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend

# Arrêter les containers
docker-compose down

# Reconstruire l'image (sans cache)
docker-compose build --no-cache

# Démarrer les containers
docker-compose up -d

# Voir les logs
docker-compose logs -f api
```

### Étape 3: Vérifier que le Endpoint est Disponible

Attendez que le container démarre (environ 30 secondes), puis:

```bash
# Vérifier la santé de l'API
curl http://localhost:5001/health

# Vérifier Swagger UI
open http://localhost:5001/swagger
```

Dans Swagger, vous devriez voir le nouveau endpoint:
- **POST /api/market-trends** - Analyze market trends

## 🔍 Vérification Rapide

### Test 1: Health Check
```bash
curl http://localhost:5001/health
```
**Attendu:** `Healthy`

### Test 2: Swagger UI
Ouvrir dans le navigateur: http://localhost:5001/swagger

Vous devriez voir:
- ✅ `/api/market-trends` (nouveau endpoint)
- ✅ `/api/margin/simulate`
- ✅ `/api/clients`
- ✅ Tous les autres endpoints existants

### Test 3: Market Trends Endpoint (avec JWT)
```bash
# Remplacez YOUR_JWT_TOKEN par votre token Auth0
curl -X POST http://localhost:5001/api/market-trends \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "role": "Java Developer",
    "seniority": "Senior",
    "resourceType": "Employee",
    "location": "Montreal, Canada",
    "currency": "CAD",
    "proposedAnnualSalary": 95000
  }'
```

## 🐛 Dépannage

### Problème: "OPENAI_API_KEY not set"

**Solution:**
```bash
# Vérifier que la variable est définie dans .env
cat .env | grep OPENAI_API_KEY

# Si vide, éditez le fichier
nano .env

# Redémarrer les containers
docker-compose restart api
```

### Problème: "Cannot connect to Docker daemon"

**Solution:**
```bash
# Démarrer Docker Desktop
open -a Docker

# Attendre que Docker soit prêt, puis réessayer
docker ps
```

### Problème: "Port 5001 already in use"

**Solution:**
```bash
# Arrêter tous les containers
docker-compose down

# Vérifier qu'aucun processus n'utilise le port
lsof -i :5001

# Si un processus est trouvé, le tuer
kill -9 <PID>

# Redémarrer
docker-compose up -d
```

### Problème: "Endpoint not found" même après rebuild

**Solution:**
```bash
# Vérifier que le container utilise la nouvelle image
docker-compose ps

# Forcer un rebuild complet
docker-compose down
docker system prune -f
docker-compose build --no-cache --pull
docker-compose up -d
```

## 📊 Vérifier les Logs

```bash
# Logs en temps réel
docker-compose logs -f api

# Rechercher l'enregistrement du service
docker-compose logs api | grep MarketTrends

# Devrait afficher quelque chose comme:
# "AddScoped<IMarketTrendsService, MarketTrendsService>()"
```

## 🚀 Rebuild Rapide (pour les développeurs)

Si vous modifiez le code fréquemment:

```bash
# Rebuild et restart en une commande
docker-compose up -d --build

# Ou utilisez le script
./scripts/rebuild-docker.sh
```

## 📝 Checklist Complète

Après le rebuild, vérifiez:

- [ ] Container `apg-api` est en cours d'exécution (`docker ps`)
- [ ] Aucune erreur dans les logs (`docker-compose logs api`)
- [ ] Health endpoint répond (`curl http://localhost:5001/health`)
- [ ] Swagger UI est accessible (http://localhost:5001/swagger)
- [ ] Endpoint `/api/market-trends` visible dans Swagger
- [ ] Variable `OPENAI_API_KEY` est configurée dans `.env`
- [ ] Test avec JWT token fonctionne

## 💡 Conseils

1. **Développement local sans Docker**: Plus rapide pour tester
   ```bash
   cd src/APG.API
   export OpenAI__ApiKey="sk-votre-clé"
   dotnet run
   ```

2. **Utiliser Docker uniquement pour production/staging**: Plus simple pour le développement

3. **Hot Reload avec Docker** (avancé):
   - Monter le code source comme volume
   - Utiliser `dotnet watch` dans le container

## 🔄 Workflow Recommandé

1. **Développement**: Utiliser `dotnet run` directement (plus rapide)
2. **Test d'intégration**: Reconstruire Docker
3. **Déploiement**: Utiliser l'image Docker

---

**Pour plus d'informations**: Voir [MARKET_TRENDS_API.md](./MARKET_TRENDS_API.md)
