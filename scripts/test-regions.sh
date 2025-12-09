#!/bin/bash

echo "🔍 Testing which Azure regions work for your subscription..."
echo ""

# Régions à tester (les plus communes pour Azure for Students)
regions=("eastus" "westus2" "northeurope" "westeurope" "centralus" "canadaeast")

for region in "${regions[@]}"; do
    echo -n "Testing $region... "
    
    # Test simple: créer un resource group temporaire
    result=$(az group create --name "test-region-$region" --location "$region" 2>&1)
    
    if [[ $result == *"error"* ]] || [[ $result == *"disallowed"* ]]; then
        echo "❌ Not allowed"
    else
        echo "✅ Available!"
        echo "   -> You can use: $region"
        # Nettoyer
        az group delete --name "test-region-$region" --yes --no-wait 2>/dev/null
    fi
done

echo ""
echo "Test complete. Use one of the ✅ regions above."
