# Solution du problème 500 - Calculator Settings Endpoints

## Problème identifié
Les endpoints `/api/calculator-settings/*` retournaient une erreur 500 avec le message:
```
Microsoft.Data.SqlClient.SqlException (0x80131904): Invalid object name 'GlobalSalarySettings'.
Microsoft.Data.SqlClient.SqlException (0x80131904): Invalid object name 'ClientMarginSettings'.
```

## Cause racine
Les tables `GlobalSalarySettings` et `ClientMarginSettings` n'existaient pas dans la base de données car:
1. La migration a été créée manuellement (fichier .cs) mais n'a jamais été compilée dans l'assembly APG.Persistence
2. Lors du rebuild Docker, l'API a signalé: "No migrations were found in assembly 'APG.Persistence'"
3. Sans migration compilée, EF Core n'a pas pu créer les tables automatiquement au démarrage

## Solution appliquée
Comme .NET SDK n'était pas installé localement et que le container API est runtime-only, nous avons:

1. **Créé un script SQL manuel** (`005_AddCalculatorSettings.sql`) avec:
   - Table `GlobalSalarySettings` (Id, EmployerChargesRate, IndirectAnnualCosts, BillableHoursPerYear, CreatedAt, UpdatedAt)
   - Table `ClientMarginSettings` (Id, ClientId, TargetMarginPercent, TargetHourlyRate, CreatedAt, UpdatedAt)
   - Foreign Key vers Clients avec ON DELETE CASCADE
   - Index unique sur ClientMarginSettings.ClientId

2. **Créé un script Python** (`apply_migration.py`) pour exécuter le SQL via docker exec vers le container apg-sqlserver

3. **Exécuté la migration** avec succès:
   ```
   ✓ Batch 1 executed successfully - Table GlobalSalarySettings created
   ✓ Batch 2 executed successfully - Table ClientMarginSettings created
   ```

## Vérifications post-migration
- ✅ Tables créées avec la bonne structure (6 colonnes chacune)
- ✅ Foreign key et index unique en place
- ✅ Endpoints ne retournent plus 500 (maintenant 401 sans auth = comportement correct)
- ✅ Plus d'erreurs "Invalid object name" dans les logs
- ✅ Frontend toujours en cours d'exécution sur port 5173

## Prochaines étapes recommandées
1. **Tester avec un token valide** depuis le frontend Auth0
2. **Vérifier les opérations CRUD**:
   - GET global-salary (devrait retourner 204 No Content initialement)
   - PUT global-salary avec des données de test
   - GET client-margins
   - POST client-margins pour un client
   - PUT/DELETE client-margins

3. **Optionnel - Régénérer la migration EF Core**:
   - Installer .NET 8 SDK localement
   - Supprimer les tables (DROP TABLE)
   - Utiliser `dotnet ef migrations add AddCalculatorSettings`
   - Cela mettra à jour le ModelSnapshot correctement

## Fichiers créés/modifiés
- ✅ `/migrations/005_AddCalculatorSettings.sql` - Script SQL de création
- ✅ `/migrations/apply_migration.py` - Script d'application
- ✅ `/migrations/verify_tables.py` - Script de vérification

## Status final
🟢 **RÉSOLU** - Les endpoints calculator-settings sont maintenant opérationnels et prêts pour les tests avec authentification Auth0.
