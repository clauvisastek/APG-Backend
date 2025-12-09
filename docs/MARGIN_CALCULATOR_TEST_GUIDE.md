# Guide de Test - Calculette de Marge

## Prérequis

### Backend
1. SQL Server avec la base de données APGDb configurée
2. Au moins un GlobalSalarySettings actif dans la base
3. Au moins un client avec **tous** les paramètres financiers configurés :
   - DefaultTargetMarginPercent
   - DefaultMinimumMarginPercent
   - DiscountPercent
   - ForcedVacationDaysPerYear
   - TargetHourlyRate

### Frontend
1. Backend ASP.NET Core en cours d'exécution (port 5157 par défaut)
2. Frontend React en cours d'exécution (port 5173 par défaut)
3. Variable d'environnement `VITE_API_BASE_URL` configurée si nécessaire

## Étapes de test

### 1. Préparation des données test

**Créer un GlobalSalarySettings actif** (via l'interface Admin/CFO) :
```
Charges patronales : 65 %
Coûts indirects annuels : 5 000 $
Heures facturables / an : 1 600 h
```

**Créer ou configurer un client** avec paramètres financiers complets :
```
Nom : Test Corp
Marge cible : 25 %
Marge minimale : 15 %
Remise : 10 %
Jours de vacances forcés : 10 jours
Vendant cible : 120 $/h
```

### 2. Test d'une ressource Salariée

1. Naviguer vers la page Calculette
2. Sélectionner les paramètres suivants :
   ```
   Type de ressource : Salarié
   Salaire annuel brut : 75 000 $
   Nombre d'heures prévues : 160 h
   Séniorité : Sénior (optionnel)
   Client : Test Corp
   Vendant client proposé : 110 $/h
   ```

3. Cliquer sur "Calculer la marge"

#### Résultats attendus

**Bloc A - Objectifs CFO :**
- Coûtant moyen / h : environ 79,17 $/h
  - Calcul : (75000 × 1.65 + 5000) / (1600 - 10 × 7.5) = 128,750 / 1525 ≈ 84,43 $/h
- Vendant cible / h : 108 $/h (120 × 0.90, après remise de 10%)
- Marge cible théorique : environ 21,85 %
- Badge : **WARNING** (sous l'objectif de 25% mais au-dessus du minimum de 15%)

**Bloc B - Résultats avec vendant proposé :**
- Vendant proposé : 110 $/h
- Marge obtenue : environ 23,26 %
  - Calcul : (110 - 84,43) / 110 × 100 ≈ 23,26 %
- Marge / h : environ 25,57 $/h
- Remise appliquée : -1,85 % (vendant proposé supérieur au vendant cible après remise)
- Badge : **WARNING** (sous l'objectif mais au-dessus du minimum)

### 3. Test d'une ressource Pigiste

1. Modifier le formulaire :
   ```
   Type de ressource : Pigiste
   Tarif horaire : N/A (dans cette v1, on utilise le vendant proposé comme proxy)
   Nombre d'heures prévues : 160 h
   Client : Test Corp
   Vendant client proposé : 120 $/h
   ```

2. Cliquer sur "Calculer la marge"

#### Résultats attendus

**Bloc A - Objectifs CFO :**
- Coûtant moyen / h : 120 $/h (= vendant proposé pour un pigiste)
- Vendant cible / h : 108 $/h (après remise)
- Marge cible théorique : **négative** (le coûtant est supérieur au vendant cible)
- Badge : **KO**

**Bloc B - Résultats avec vendant proposé :**
- Vendant proposé : 120 $/h
- Marge obtenue : 0 % (coûtant = vendant)
- Badge : **KO** (sous le minimum)

### 4. Test des validations

**Test 1 : Client sans paramètres financiers complets**
1. Créer un client sans tous les paramètres requis
2. Sélectionner ce client dans le formulaire
3. Vérifier qu'un warning jaune s'affiche
4. Vérifier que le bouton "Calculer la marge" est désactivé

**Test 2 : Champs requis manquants**
1. Ne pas renseigner le salaire annuel (pour Salarié)
2. Vérifier que le bouton est désactivé
3. Renseigner le salaire
4. Ne pas renseigner le vendant proposé
5. Vérifier que le bouton est désactivé

**Test 3 : Pas de GlobalSalarySettings actif**
1. Désactiver tous les GlobalSalarySettings dans la base
2. Essayer de calculer pour un Salarié
3. Vérifier qu'une erreur s'affiche : "Aucun paramètre global de salaire actif trouvé"

### 5. Test des cas limites

**Cas 1 : Vendant proposé très élevé**
```
Salaire : 75 000 $
Vendant proposé : 200 $/h
```
Résultat attendu : Badge **OK** (marge largement au-dessus de l'objectif)

**Cas 2 : Vendant proposé très bas**
```
Salaire : 75 000 $
Vendant proposé : 70 $/h
```
Résultat attendu : Badge **KO** (marge sous le minimum)

**Cas 3 : Remise à 0%**
```
Configurer le client avec Remise : 0 %
```
Résultat attendu : Vendant cible effectif = Vendant cible configuré (pas de réduction)

**Cas 4 : Jours de vacances forcés à 0**
```
Configurer le client avec Jours vacances forcés : 0
```
Résultat attendu : Heures facturables effectives = Heures facturables/an du GlobalSalarySettings

### 6. Vérification de l'UI

**Responsive :**
- Desktop : Les deux blocs sont côte à côte
- Tablet/Mobile : Les deux blocs sont empilés verticalement

**Badges de statut :**
- **OK** : Fond vert léger avec bordure verte
- **WARNING** : Fond jaune/ambre avec bordure ambre
- **KO** : Fond rouge clair avec bordure rouge

**Cartes :**
- Design propre avec bordures arrondies
- Icônes SVG pour chaque bloc
- Informations bien organisées en sections

## Tests API directs (via Swagger ou Postman)

### Endpoint : POST /api/margin/simulate

**Exemple de requête - Salarié :**
```json
{
  "resourceType": "Salarie",
  "annualGrossSalary": 75000,
  "clientId": 1,
  "proposedBillRate": 110,
  "plannedHours": 160,
  "seniority": "Sénior"
}
```

**Exemple de requête - Pigiste :**
```json
{
  "resourceType": "Pigiste",
  "clientId": 1,
  "proposedBillRate": 120,
  "plannedHours": 160
}
```

**Exemple de réponse réussie (200 OK) :**
```json
{
  "targetResults": {
    "costPerHour": 84.43,
    "effectiveTargetBillRate": 108.00,
    "theoreticalMarginPercent": 21.85,
    "theoreticalMarginPerHour": 23.57,
    "configuredTargetMarginPercent": 25.00,
    "configuredMinMarginPercent": 15.00,
    "configuredDiscountPercent": 10.00,
    "forcedVacationDaysPerYear": 10,
    "status": "WARNING"
  },
  "proposedResults": {
    "proposedBillRate": 110.00,
    "marginPercent": 23.26,
    "marginPerHour": 25.57,
    "discountPercentApplied": -1.85,
    "status": "WARNING"
  }
}
```

**Exemples de réponses d'erreur (400 Bad Request) :**

Client introuvable :
```json
{
  "status": 400,
  "title": "Données invalides",
  "detail": "Client avec l'ID 999 introuvable."
}
```

Configuration client incomplète :
```json
{
  "status": 400,
  "title": "Configuration incomplète",
  "detail": "Les paramètres financiers du client sont incomplets. Veuillez configurer : marge cible, marge minimale, remise, jours de vacances forcés et vendant cible avant de lancer une simulation."
}
```

Pas de GlobalSalarySettings actif :
```json
{
  "status": 400,
  "title": "Configuration incomplète",
  "detail": "Aucun paramètre global de salaire actif trouvé. Veuillez configurer les paramètres globaux pour les calculs de ressources salariées."
}
```

## Checklist de validation

- [ ] Backend compile sans erreur
- [ ] Frontend compile sans erreur TypeScript
- [ ] L'API /api/margin/simulate répond correctement
- [ ] Le formulaire affiche le warning pour un client incomplet
- [ ] Le bouton est désactivé si données manquantes
- [ ] Les calculs pour Salarié sont corrects
- [ ] Les calculs pour Pigiste sont corrects
- [ ] Les badges de statut s'affichent correctement
- [ ] Les deux blocs de résultats sont bien visibles
- [ ] Le layout est responsive (desktop + mobile)
- [ ] Les remises sont correctement appliquées
- [ ] Les jours de vacances forcés réduisent les heures facturables
- [ ] Les messages d'erreur sont clairs et informatifs
- [ ] Les montants sont arrondis à 2 décimales

## Dépannage

**Problème** : "Cannot read properties of undefined"
- **Solution** : Vérifier que tous les paramètres financiers du client sont configurés

**Problème** : "Aucun paramètre global de salaire actif trouvé"
- **Solution** : Créer ou activer un GlobalSalarySettings via l'interface Admin/CFO

**Problème** : Les résultats ne s'affichent pas
- **Solution** : Vérifier la console browser pour les erreurs réseau ou de parsing JSON

**Problème** : Les calculs semblent incorrects
- **Solution** : Vérifier les formules dans `MarginSimulationService.cs` et comparer avec la documentation

**Problème** : Le bouton reste désactivé
- **Solution** : Vérifier que `isFinancialConfigComplete` du client est `true`
