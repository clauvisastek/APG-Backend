# Implémentation de la Calculette de Marge

## Vue d'ensemble

La calculette de marge a été implémentée avec une architecture complète backend + frontend pour calculer et afficher les marges selon deux scénarios :
1. **Résultats cibles (CFO)** : basés sur les paramètres financiers configurés du client
2. **Résultats avec vendant proposé** : simulation réelle avec le taux horaire proposé

## Architecture Backend (C# / ASP.NET Core)

### 1. DTOs (`APG.Application/DTOs/MarginSimulationDto.cs`)

**MarginSimulationRequest**
- `ResourceType` : "Salarie" | "Pigiste"
- `AnnualGrossSalary` : Salaire annuel brut (nullable, requis pour Salarié)
- `ClientId` : ID du client
- `ProposedBillRate` : Vendant client proposé ($/h)
- `PlannedHours` : Nombre d'heures prévues (optionnel)
- `Seniority` : Séniorité (optionnel)

**MarginSimulationResponse**
- `TargetResults` : Résultats cibles CFO
- `ProposedResults` : Résultats avec vendant proposé

**TargetResults**
- `CostPerHour` : Coûtant moyen / h
- `EffectiveTargetBillRate` : Vendant cible après remise / h
- `TheoreticalMarginPercent` : Marge cible théorique (%)
- `TheoreticalMarginPerHour` : Marge cible théorique ($/h)
- `ConfiguredTargetMarginPercent` : Marge cible configurée (%)
- `ConfiguredMinMarginPercent` : Marge minimale configurée (%)
- `ConfiguredDiscountPercent` : Remise configurée (%)
- `ForcedVacationDaysPerYear` : Jours de vacances forcés / an
- `Status` : "OK" | "WARNING" | "KO"

**ProposedResults**
- `ProposedBillRate` : Vendant proposé / h
- `MarginPercent` : Marge obtenue (%)
- `MarginPerHour` : Marge obtenue ($/h)
- `DiscountPercentApplied` : Remise appliquée vs vendant cible (%)
- `Status` : "OK" | "WARNING" | "KO"

### 2. Service de domaine (`APG.Application/Services/MarginSimulationService.cs`)

**Méthode principale** : `SimulateAsync(MarginSimulationRequest request)`

#### Logique de calcul du coûtant moyen / heure

**Pour un salarié :**
```csharp
employerChargesFactor = 1 + (EmployerChargesRate / 100);
totalAnnualSalaryCost = salaireAnnuelBrut * employerChargesFactor;
totalAnnualCost = totalAnnualSalaryCost + IndirectAnnualCosts;

forcedVacationHours = ForcedVacationDaysPerYear * 7.5;
effectiveBillableHours = BillableHoursPerYear - forcedVacationHours;

costPerHour = totalAnnualCost / effectiveBillableHours;
```

**Pour un pigiste :**
```csharp
costPerHour = proposedBillRate; // Proxy du tarif pigiste
```

#### Calcul des résultats cibles (CFO)

```csharp
effectiveTargetBillRate = targetBillRate * (1 - (discountPercent / 100));

theoreticalMarginPercent = 
    ((effectiveTargetBillRate - costPerHour) / effectiveTargetBillRate) * 100;

theoreticalMarginPerHour = effectiveTargetBillRate - costPerHour;
```

**Détermination du statut :**
- `OK` : si `theoreticalMarginPercent >= targetMarginPercent`
- `WARNING` : si `theoreticalMarginPercent >= minMarginPercent`
- `KO` : sinon

#### Calcul des résultats avec vendant proposé

```csharp
marginPercentProposed = 
    ((proposedBillRate - costPerHour) / proposedBillRate) * 100;

marginPerHourProposed = proposedBillRate - costPerHour;

discountPercentApplied = 
    ((effectiveTargetBillRate - proposedBillRate) / effectiveTargetBillRate) * 100;
```

**Statut** : déterminé de la même façon que pour les résultats cibles.

### 3. Contrôleur API (`APG.API/Controllers/MarginController.cs`)

**Endpoint** : `POST /api/margin/simulate`

**Validations :**
- Type de ressource requis
- Client ID valide
- Vendant proposé > 0
- Salaire annuel brut requis pour Salarié

**Gestion des erreurs :**
- `ArgumentException` → 400 Bad Request
- `InvalidOperationException` → 400 Bad Request (config incomplète)
- Exception générale → 500 Internal Server Error

### 4. Enregistrement du service (Program.cs)

```csharp
builder.Services.AddScoped<MarginSimulationService>();
```

## Architecture Frontend (React / TypeScript)

### 1. Types TypeScript (`src/types/margin.ts`)

Types miroirs des DTOs backend :
- `ResourceType` : 'Salarie' | 'Pigiste'
- `MarginStatus` : 'OK' | 'WARNING' | 'KO'
- `MarginSimulationRequest`
- `TargetResults`
- `ProposedResults`
- `MarginSimulationResponse`

### 2. Client API (`src/services/api.ts`)

**Fonction** : `marginApi.simulate(request: MarginSimulationRequest)`

Effectue un POST vers `/api/margin/simulate` et retourne `MarginSimulationResponse`.

### 3. Composant Badge de statut (`src/components/MarginStatusBadge.tsx`)

Composant réutilisable pour afficher les badges colorés :
- **OK** : Fond vert, "Conforme à l'objectif"
- **WARNING** : Fond ambre, "Sous l'objectif (≥ min)"
- **KO** : Fond rouge, "Sous le minimum"

### 4. Affichage des résultats (`src/components/CalculetteResults.tsx`)

**Layout responsive en 2 colonnes** (grille Tailwind) :

#### Bloc A - Objectifs CFO – Résultats cibles
- Coûtant moyen / h
- Vendant cible / h (après remise)
- Marge cible théorique avec badge de statut
- Section "Configuration client" :
  - Marge cible configurée
  - Marge minimale configurée
  - Remise configurée
  - Jours de vacances forcés / an

#### Bloc B - Résultats avec le vendant proposé
- Vendant proposé / h
- Marge obtenue avec badge de statut
- Marge / h
- Remise appliquée vs vendant cible
- Message d'analyse contextuel
- Section "Comparaison rapide" :
  - Marge cible théorique
  - Marge avec vendant proposé
  - Écart (avec couleur conditionnelle)

### 5. Formulaire de calculette (`src/components/CalculetteForm.tsx`)

**Champs du formulaire :**
- Type de ressource (Salarié / Pigiste)
- Salaire annuel brut (pour Salarié)
- Tarif horaire (pour Pigiste)
- Nombre d'heures prévues
- Séniorité (optionnel)
- Client (avec validation de configuration financière)
- Vendant client proposé ($/h)
- Projet / BU (optionnel)

**Validation :**
- Le bouton "Calculer la marge" est désactivé si :
  - Le client n'a pas tous ses paramètres financiers configurés
  - Les champs requis sont manquants

**Warning box :**
Affiche un avertissement jaune si le client sélectionné a des paramètres financiers incomplets.

### 6. Page Calculette (`src/pages/CalculettePage.tsx`)

**Logique principale :**
1. Charge la liste des clients via `clientsApi.getAll()`
2. Transforme les données du formulaire en `MarginSimulationRequest`
3. Appelle `marginApi.simulate(request)`
4. Affiche les résultats via `CalculetteResultsDisplay`

**Mapping des types de ressources :**
```typescript
const request = {
  resourceType: formData.resourceType === 'SALARIE' ? 'Salarie' : 'Pigiste',
  annualGrossSalary: formData.salaireAnnuel,
  clientId: parseInt(formData.clientId),
  proposedBillRate: formData.vendantClientProposeHoraire,
  plannedHours: formData.heures,
  seniority: formData.seniorite,
};
```

**Gestion des erreurs :**
- Affiche un toast d'erreur avec le message du backend
- Validation côté client avant l'appel API

## Formules métier importantes

### Constantes
```csharp
HOURS_PER_DAY = 7.5m;  // 1 jour = 7,5 heures facturables
```

### Coûtant salarié avec vacances forcées
```
Coût total annuel = (Salaire brut × (1 + Charges patronales %)) + Coûts indirects
Heures facturables effectives = Heures facturables/an - (Jours vacances forcés × 7.5)
Coûtant/h = Coût total annuel / Heures facturables effectives
```

### Vendant cible après remise
```
Vendant cible effectif = Vendant cible × (1 - Remise % / 100)
```

### Marge théorique
```
Marge % = ((Vendant - Coûtant) / Vendant) × 100
Marge $/h = Vendant - Coûtant
```

### Remise appliquée vs vendant cible
```
Remise appliquée % = ((Vendant cible effectif - Vendant proposé) / Vendant cible effectif) × 100
```

## Workflow utilisateur

1. L'utilisateur sélectionne un client dans le formulaire
2. Si les paramètres financiers du client sont incomplets, un warning s'affiche
3. L'utilisateur remplit les champs requis (type ressource, salaire/tarif, heures, vendant proposé)
4. Le bouton "Calculer la marge" est activé uniquement si :
   - Tous les champs requis sont remplis
   - Le client a une configuration financière complète
5. Au clic sur "Calculer la marge" :
   - Appel API vers `/api/margin/simulate`
   - Affichage des 2 blocs de résultats en temps réel
6. L'utilisateur peut comparer :
   - Les objectifs CFO (résultats théoriques)
   - La simulation réelle avec le vendant proposé
   - L'écart entre les deux scénarios

## Prochaines étapes

- [ ] Implémenter la sauvegarde de scénarios avec la nouvelle structure de données
- [ ] Ajouter l'historique des simulations
- [ ] Tests unitaires backend complets
- [ ] Tests d'intégration frontend/backend
- [ ] Optimiser les calculs pour les ressources pigistes (ajouter un champ dédié)
- [ ] Permettre l'édition des paramètres globaux depuis l'interface

## Notes techniques

- Tous les montants sont arrondis à 2 décimales
- La remise est appliquée sur le vendant cible, pas sur le coûtant
- Les jours de vacances forcés réduisent les heures facturables effectives
- Un client doit avoir **tous** ses paramètres financiers configurés pour pouvoir lancer une simulation
- Le statut est calculé de manière identique pour les résultats cibles et proposés
