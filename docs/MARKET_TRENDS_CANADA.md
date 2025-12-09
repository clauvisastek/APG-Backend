# Market Trends - Analyse du marché canadien

## Changements apportés

### 1. Ciblage du marché canadien
- Les analyses se basent maintenant **exclusivement sur le marché canadien** (Toronto, Montréal, Vancouver, Calgary, Ottawa)
- Tous les montants sont en **dollars canadiens (CAD)** par défaut
- Remplacement du focus Europe/Amérique du Nord par un focus Canada uniquement

### 2. Résultats par niveau de séniorité
La réponse inclut maintenant des données détaillées pour **3 niveaux de séniorité** :
- **Junior** : Profils débutants (0-2 ans d'expérience)
- **Intermediate** : Profils intermédiaires (3-5 ans d'expérience)
- **Senior** : Profils expérimentés (5+ ans d'expérience)

## Structure de la réponse

### Nouveaux champs ajoutés

```json
{
  "salaryRangeByLevel": {
    "junior": { "min": 55000, "max": 75000, "currency": "CAD" },
    "intermediate": { "min": 75000, "max": 95000, "currency": "CAD" },
    "senior": { "min": 95000, "max": 130000, "currency": "CAD" }
  },
  "freelanceRateRangeByLevel": {
    "junior": { "min": 45, "max": 65, "currency": "CAD" },
    "intermediate": { "min": 65, "max": 90, "currency": "CAD" },
    "senior": { "min": 90, "max": 140, "currency": "CAD" }
  },
  "salaryRange": { "min": 75000, "max": 95000, "currency": "CAD" },
  "freelanceRateRange": { "min": 65, "max": 90, "currency": "CAD" },
  "employeePositioning": "in_line",
  "freelancePositioning": "in_line",
  "marketDemand": "high",
  "riskLevel": "low",
  "summary": "...",
  "recommendation": "..."
}
```

### Champs conservés
- `salaryRange` : Fourchette pour le niveau de séniorité spécifié (ou intermédiaire par défaut)
- `freelanceRateRange` : Taux horaire pour le niveau de séniorité spécifié
- `employeePositioning` : Positionnement par rapport au marché canadien
- `freelancePositioning` : Positionnement des taux par rapport au marché canadien
- `marketDemand` : Demande sur le marché canadien
- `riskLevel` : Risque d'attrition au Canada
- `summary` : Résumé en français
- `recommendation` : Recommandation en français

## Exemple de requête

### Avec niveau de séniorité spécifié

```json
POST /api/market-trends
{
  "role": "Java Developer",
  "seniority": "Senior",
  "resourceType": "Employee",
  "location": "Montreal, Canada",
  "currency": "CAD",
  "proposedAnnualSalary": 95000
}
```

### Sans niveau de séniorité (obtenir tous les niveaux)

```json
POST /api/market-trends
{
  "role": "Python Developer",
  "resourceType": "Freelancer",
  "currency": "CAD",
  "proposedBillRate": 85
}
```

**Note** : Si le niveau de séniorité n'est pas spécifié, l'IA fournira quand même les données pour les trois niveaux et utilisera le niveau intermédiaire pour `salaryRange` et `freelanceRateRange`.

## Avantages

1. **Précision géographique** : Données spécifiques au marché canadien
2. **Devise cohérente** : Tous les montants en CAD
3. **Granularité** : Visibilité complète sur les trois niveaux de séniorité
4. **Flexibilité** : Possibilité de comparer les niveaux ou de cibler un niveau spécifique

## Test rapide

Utilisez le script de test fourni :

```bash
cd /Users/clauviskitieu/Documents/Projets/DPO/Apps/APG_Backend
./test-market-trends.sh
```

Ou testez manuellement avec curl :

```bash
curl -X POST http://localhost:5000/api/market-trends \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "role": "Full Stack Developer",
    "seniority": "Intermediate",
    "resourceType": "Employee",
    "location": "Toronto, Canada",
    "currency": "CAD",
    "proposedAnnualSalary": 85000
  }'
```

## Villes canadiennes de référence

L'analyse se base sur les marchés suivants :
- **Toronto** : Plus grand marché tech canadien
- **Montréal** : Hub tech bilingue avec coût de vie moyen
- **Vancouver** : Tech hub côte ouest
- **Calgary** : Marché en croissance
- **Ottawa** : Secteur gouvernemental et tech

Les fourchettes salariales tiennent compte des variations entre ces marchés.
