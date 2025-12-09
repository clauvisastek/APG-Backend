#!/bin/bash
# Quick rebuild script - rebuilds only if you confirm

echo "🔄 APG Backend - Rebuild Docker Container"
echo "=========================================="
echo ""
echo "Ce script va:"
echo "  1. Arrêter les containers existants"
echo "  2. Reconstruire l'image API avec le nouveau code"
echo "  3. Redémarrer tous les services"
echo ""

# Check if OpenAI API key is set
if grep -q "OPENAI_API_KEY=$" .env || ! grep -q "OPENAI_API_KEY" .env; then
    echo "⚠️  WARNING: OpenAI API Key n'est pas configurée!"
    echo ""
    echo "Éditez le fichier .env et ajoutez votre clé:"
    echo "  nano .env"
    echo ""
    echo "Puis ajoutez:"
    echo "  OPENAI_API_KEY=sk-votre-clé-api"
    echo ""
    read -p "Voulez-vous continuer quand même? (y/n) " -n 1 -r
    echo
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        exit 1
    fi
fi

echo ""
echo "🛑 Arrêt des containers..."
docker-compose down

echo ""
echo "🔨 Reconstruction de l'image (cela peut prendre 2-3 minutes)..."
docker-compose build --no-cache api

echo ""
echo "🚀 Démarrage des services..."
docker-compose up -d

echo ""
echo "⏳ Attente du démarrage (30 secondes)..."
sleep 30

echo ""
echo "✅ Vérification de l'API..."
HEALTH_STATUS=$(curl -s http://localhost:5001/health || echo "FAILED")

if [[ $HEALTH_STATUS == *"Healthy"* ]] || [[ $HEALTH_STATUS == *"200"* ]]; then
    echo "✅ API est en cours d'exécution!"
    echo ""
    echo "🌐 Ouvrez Swagger UI pour voir le nouveau endpoint:"
    echo "   http://localhost:5001/swagger"
    echo ""
    echo "📋 Nouveau endpoint disponible:"
    echo "   POST /api/market-trends"
    echo ""
else
    echo "⚠️  L'API ne répond pas encore. Vérifiez les logs:"
    echo "   docker-compose logs -f api"
fi

echo ""
echo "📊 Logs en temps réel (Ctrl+C pour quitter):"
docker-compose logs -f api
