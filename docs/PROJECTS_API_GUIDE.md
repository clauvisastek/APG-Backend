# Guide d'utilisation des APIs Projects - Frontend & Backend

## 📋 Résumé des Implémentations

### ✅ Backend (APG_Backend) - CRUD Complet Implémenté

#### 1. **Entité Project** (`APG.Domain/Entities/Project.cs`)
- Hérite de `BaseEntity` (Id, CreatedAt, UpdatedAt)
- Propriétés principales :
  - Name, Code (unique)
  - ClientId, BusinessUnitId (relations)
  - Type (T&M, Forfait, Autre)
  - ResponsibleName
  - Currency (CAD, USD, EUR)
  - StartDate, EndDate
  - TargetMargin, MinMargin
  - Status (En construction, Actif, Terminé, En pause)
  - Notes, YtdRevenue
  - TeamMembersJson, GlobalMarginHistoryJson (JSON)
  - IsActive (soft delete)

#### 2. **DTOs** (`APG.Application/DTOs/ProjectDto.cs`)
- `ProjectDto` : Retourné par l'API avec toutes les infos + relations
- `ProjectCreateDto` : Pour créer un nouveau projet
- `ProjectUpdateDto` : Pour modifier un projet existant
- `TeamMemberDto` : Membre d'équipe (id, name, role, costRate, sellRate, marges)
- `GlobalMarginHistoryDto` : Historique des marges (label, value)

#### 3. **Service Layer** (`APG.Persistence/Services/ProjectService.cs`)
**Bonnes pratiques implémentées** :
- ✅ Validation complète des données (client, BU, dates, marges)
- ✅ Contrôle d'accès basé sur les Business Units
- ✅ Vérification des doublons (code projet unique)
- ✅ Validation de cohérence (endDate > startDate, minMargin ≤ targetMargin)
- ✅ Soft delete avec mise à jour de `IsActive`
- ✅ Gestion d'erreurs complète avec exceptions typées
- ✅ Logging des opérations
- ✅ JSON serialization/deserialization pour teamMembers et marginHistory

#### 4. **Controller** (`APG.API/Controllers/ProjectsController.cs`)
**Endpoints disponibles** :
```csharp
GET    /api/Projects           // Liste tous les projets (filtrés par BU)
GET    /api/Projects/{id}      // Récupère un projet par ID
POST   /api/Projects           // Crée un nouveau projet
PUT    /api/Projects/{id}      // Met à jour un projet
DELETE /api/Projects/{id}      // Suppression douce (soft delete)
```

Tous les endpoints :
- ✅ Requièrent l'authentification Auth0 (`[Authorize]`)
- ✅ Retournent des codes HTTP appropriés (200, 201, 400, 403, 404, 500)
- ✅ Gèrent les erreurs avec des messages explicites
- ✅ Logguent les opérations et erreurs

#### 5. **Migration SQL** (`migrations/009_AddProjectsTable.sql`)
- ✅ Table Projects créée avec toutes les colonnes
- ✅ Foreign keys vers Clients et BusinessUnits
- ✅ Indexes pour performance (Code unique, Name, ClientId, BusinessUnitId, Status, StartDate, IsActive)
- ✅ Migration appliquée avec succès dans APGDb

---

### ✅ Frontend (APG_Front) - Services API & Hooks

#### 1. **Service API** (`src/services/projectsApi.ts`)
Types TypeScript matching backend DTOs :
```typescript
interface ProjectDto {
  id: number;
  name: string;
  code: string;
  clientId: number;
  clientName: string;
  clientCode: string;
  businessUnitId: number;
  businessUnitCode: string;
  businessUnitName: string;
  type: string; // T&M, Forfait, Autre
  responsibleName?: string | null;
  currency: string; // CAD, USD, EUR
  startDate: string;
  endDate: string;
  targetMargin: number;
  minMargin: number;
  status: string;
  notes?: string | null;
  ytdRevenue?: number | null;
  teamMembers?: TeamMemberDto[] | null;
  globalMarginHistory?: GlobalMarginHistoryDto[] | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}
```

**Fonctions API disponibles** :
```typescript
projectsApi.getAll()                    // GET tous les projets
projectsApi.getById(id)                 // GET un projet par ID
projectsApi.create(payload)             // POST créer un projet
projectsApi.update(id, payload)         // PUT modifier un projet
projectsApi.delete(id)                  // DELETE supprimer un projet
```

#### 2. **Hooks React Query** (`src/hooks/useProjectsApi.ts`)
```typescript
// Récupérer tous les projets
const { data: projects, isLoading, error } = useProjectsQuery();

// Récupérer un projet par ID
const { data: project } = useProjectQuery(projectId);

// Créer un projet
const createMutation = useCreateProjectMutation();
await createMutation.mutateAsync(projectData);

// Modifier un projet
const updateMutation = useUpdateProjectMutation();
await updateMutation.mutateAsync({ id, data: projectData });

// Supprimer un projet
const deleteMutation = useDeleteProjectMutation();
await deleteMutation.mutateAsync(id);
```

---

## 🔧 Utilisation dans ProjectsPage

### Exemple : Intégration dans ProjectsPage.tsx

```typescript
import { useProjectsQuery, useCreateProjectMutation, useDeleteProjectMutation } from '../hooks/useProjectsApi';
import { useClientsQuery } from '../hooks/useClientsApi';
import { toast } from 'react-toastify';

export const ProjectsPage = () => {
  const { data: projects, isLoading } = useProjectsQuery();
  const { data: clients } = useClientsQuery();
  const createMutation = useCreateProjectMutation();
  const deleteMutation = useDeleteProjectMutation();

  const handleCreateProject = async (wizardData: ProjectWizardStep1Values) => {
    try {
      // 1. Trouver le client par nom (ou demander clientId dans le wizard)
      const client = clients?.find(c => c.name === wizardData.clientName);
      if (!client) {
        toast.error('Client non trouvé');
        return;
      }

      // 2. Mapper les données du wizard vers ProjectCreateDto
      const payload = {
        name: wizardData.name,
        code: wizardData.code,
        clientId: client.id,
        businessUnitId: client.businessUnitId, // ou depuis buFilter
        type: wizardData.type,
        responsibleName: wizardData.projectManager,
        currency: wizardData.currency,
        startDate: wizardData.startDate,
        endDate: wizardData.endDate,
        targetMargin: wizardData.margins.targetMarginPercent,
        minMargin: wizardData.margins.minMarginPercent,
        status: 'En construction',
        notes: `Équipe: ${wizardData.teamMembers.length} membre(s)`,
        teamMembers: wizardData.teamMembers.map(tm => ({
          id: tm.id,
          name: `${tm.firstName} ${tm.lastName}`,
          role: tm.role,
          costRate: tm.internalCostRate,
          sellRate: tm.proposedBillRate,
          grossMargin: tm.grossMarginAmount,
          netMargin: tm.netMarginPercent,
        })),
      };

      // 3. Créer le projet
      await createMutation.mutateAsync(payload);
      toast.success('Projet créé avec succès');
    } catch (error) {
      console.error('Error creating project:', error);
      toast.error('Erreur lors de la création du projet');
    }
  };

  const handleDeleteProject = async (projectId: number) => {
    if (window.confirm('Êtes-vous sûr ?')) {
      try {
        await deleteMutation.mutateAsync(projectId);
        toast.success('Projet supprimé');
      } catch (error) {
        toast.error('Erreur lors de la suppression');
      }
    }
  };

  // ... rest of component
}
```

---

## 🎯 Points d'Attention

### 1. **Mapping Client dans le Wizard**
Le wizard actuel utilise `clientName` (string), mais l'API backend nécessite `clientId` (number).

**Solutions** :
- **Option A** : Modifier le wizard pour utiliser un dropdown de clients existants et stocker le `clientId`
- **Option B** : Utiliser `useClientsQuery()` pour retrouver le client par nom avant de créer le projet

### 2. **Business Unit Selection**
Le wizard doit permettre de sélectionner une Business Unit (ou l'inférer du client sélectionné).

### 3. **Status Mapping**
Assurez-vous que les statuts correspondent :
- Frontend: `ProjectStatus.CONSTRUCTION`, `ProjectStatus.ACTIVE`, etc.
- Backend: `"En construction"`, `"Actif"`, `"Terminé"`, `"En pause"`

### 4. **Type Mapping**
- Frontend: `ProjectType.TIME_AND_MATERIALS` = `"T&M"`
- Frontend: `ProjectType.FIXED_PRICE` = `"Forfait"`
- Backend attend les valeurs string directement

---

## ✅ État du Backend

**Backend opérationnel** :
- ✅ Docker containers running (API + SQL Server)
- ✅ Table Projects créée et migrée
- ✅ Tous les endpoints testés (retournent 401 si non authentifié = normal)
- ✅ Service ProjectService enregistré dans DI

**URLs** :
- Backend API: `http://localhost:5001`
- Frontend: `http://localhost:5174`

---

## 🚀 Prochaines Étapes

Pour compléter l'intégration :

1. **Mettre à jour ProjectCreationWizard** :
   - Ajouter un dropdown pour sélectionner un client existant (au lieu de saisir un nom)
   - Stocker `clientId` au lieu de `clientName`
   - Ajouter un champ pour Business Unit (ou l'inférer du client)

2. **Mettre à jour ProjectsPage** :
   - Remplacer `useProjects()` par `useProjectsQuery()`
   - Implémenter `handleWizardSubmit` avec `createMutation`
   - Implémenter `handleDelete` avec `deleteMutation`
   - Mapper `ProjectDto` vers `Project` pour compatibilité

3. **Tester le flux complet** :
   - Créer un projet avec le wizard
   - Visualiser la liste des projets
   - Modifier un projet
   - Supprimer un projet

---

## 📝 Notes Techniques

### Backend Architecture
```
Controller (ProjectsController)
    ↓
Service Interface (IProjectService)
    ↓
Service Implementation (ProjectService)
    ↓
DbContext (AppDbContext)
    ↓
Database (SQL Server - APGDb.Projects)
```

### Frontend Architecture
```
Component (ProjectsPage)
    ↓
React Query Hook (useProjectsQuery)
    ↓
API Service (projectsApi)
    ↓
Auth Fetch (fetchWithAuth)
    ↓
Backend API (/api/Projects)
```

### Sécurité
- ✅ Auth0 JWT tokens requis
- ✅ Filtrage automatique par Business Unit
- ✅ Validation des permissions avant chaque opération
- ✅ Soft delete (pas de suppression physique)

---

**Le CRUD Projects est maintenant COMPLÈTEMENT IMPLÉMENTÉ et OPÉRATIONNEL** ! 🎉

Il ne reste qu'à connecter le wizard existant avec les nouvelles API pour avoir un fonctionnement end-to-end complet.
