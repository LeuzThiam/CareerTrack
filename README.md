# CareerTrack

Application web ASP.NET Core permettant de gérer et d'analyser une recherche d'emploi : offres, candidatures, statuts, relances, entrevues, contacts, documents et statistiques.

## Contexte

Une personne en recherche d'emploi utilise souvent plusieurs sources (Indeed, LinkedIn, Jobillico, Guichet-Emplois, sites d'entreprises, recommandations, candidatures spontanées...). Au fil du temps, il devient facile de perdre le fil : oublier où l'on a postulé, oublier une relance, perdre le lien d'une offre, ne plus savoir quelle version du CV a été envoyée, etc.

## Problématique

> Comment construire une application web sécurisée permettant de centraliser, suivre et analyser toutes les étapes d'une recherche d'emploi ?

## Fonctionnalités

- Création de compte et connexion sécurisée (ASP.NET Core Identity)
- Gestion des entreprises, offres d'emploi et candidatures
- Suivi des changements de statut avec historique complet et transitions contrôlées
- Planification et suivi des relances (annulées automatiquement quand une candidature devient terminale)
- Gestion des entrevues (type, date, préparation, compte rendu, résultat)
- Gestion des contacts professionnels (recruteurs, gestionnaires)
- Téléversement sécurisé de documents (CV, lettres) hors de `wwwroot`
- Tableau de bord avec KPI, indicateurs et graphiques
- Recherche, filtrage, tri et pagination des candidatures
- Export CSV et API REST en lecture seule
- Isolation complète des données par utilisateur

## Utilisateurs

- **Candidat** : utilisateur principal, gère ses propres candidatures
- **Conseiller en emploi** *(version avancée)* : accès limité aux données d'un candidat qui l'y autorise
- **Administrateur** : gestion des comptes et des paramètres globaux, sans accès automatique au contenu privé des candidatures

## Règles métier principales

- Un utilisateur ne peut consulter que ses propres données (isolation stricte sur toutes les requêtes)
- Une candidature doit être associée à une entreprise et posséder un titre de poste
- Chaque changement de statut crée une entrée dans l'historique
- Les transitions de statut invalides sont bloquées (ex. `ToApply → OfferAccepted`)
- Une candidature au statut terminal ne peut plus générer de relance ; ses relances en attente sont annulées automatiquement
- Suppression logique plutôt que définitive (archivage)
- Modifications concurrentes détectées via `RowVersion` (deux onglets ouverts sur la même candidature)

## Architecture

Un seul projet MVC, structuré par responsabilité :

```
CareerTrack/
├── Controllers/       (dont Controllers/Api pour l'API REST)
├── Data/               (DbContext, configurations EF Core, migrations)
├── DTOs/
├── Exceptions/
├── Models/             (dont Models/Enums)
├── Services/
├── ViewModels/
├── Views/
├── wwwroot/
├── Program.cs
└── appsettings.json

tests/
├── CareerTrack.UnitTests/
└── CareerTrack.IntegrationTests/
```

## Technologies

- C# / .NET 10
- ASP.NET Core MVC + API REST
- Entity Framework Core / SQL Server
- ASP.NET Core Identity
- Bootstrap, Chart.js
- xUnit, FluentAssertions
- Docker, GitHub Actions (CI)

## Modèle de données

Entités principales : `ApplicationUser`, `Company`, `JobOffer`, `JobApplication`, `ApplicationStatusHistory`, `FollowUp`, `Interview`, `Contact`, `ApplicationDocument`.

## Installation (développement local)

```bash
git clone https://github.com/LeuzThiam/CareerTrack.git
cd CareerTrack
dotnet restore CareerTrack.slnx
```

Nécessite SQL Server LocalDB (installé avec Visual Studio) ou toute instance SQL Server accessible.

## Configuration

La chaîne de connexion de développement est définie dans `CareerTrack/appsettings.Development.json`. Pour un déploiement, fournir `ConnectionStrings__DefaultConnection` via variable d'environnement plutôt que dans un fichier commité.

## Migrations

```bash
cd CareerTrack
dotnet ef migrations add NomDeLaMigration
dotnet ef database update
```

## Tests

```bash
dotnet test CareerTrack.slnx
```

31 tests (unitaires + intégration), exécutés automatiquement en CI sur chaque push/PR vers `develop`.

## Docker

```bash
cp .env.example .env   # ajuste DB_PASSWORD
docker compose up -d --build
```

L'application sera accessible sur `http://localhost:8080`. Les migrations sont appliquées automatiquement au démarrage du conteneur (`APPLY_MIGRATIONS_ON_STARTUP=true`, voir `docker-compose.yml`) — en dehors de Docker, les migrations restent appliquées manuellement et revues avant déploiement.

```bash
docker compose down -v   # arrête et supprime les volumes (données perdues)
```

## API REST

- `GET /api/applications` — liste des candidatures de l'utilisateur courant
- `GET /api/applications/{id}` — détail d'une candidature
- `GET /openapi/v1.json` — spécification OpenAPI (Development uniquement)

Authentifiée par le même cookie que l'interface web (pas de jeton API dédié pour l'instant).

## Workflow Git

Le développement se fait sur `develop` (branches `feature/*` fusionnées via PR). `main` n'est mise à jour qu'explicitement, une fois une version stabilisée.

## Statut du projet

✅ Version 3 complète (authentification, CRUD complet, historique, relances, entrevues, contacts, documents, tableau de bord, recherche avancée, tests automatisés, export/API, conteneurisation Docker).

## Limites actuelles

- Pas d'application mobile
- Pas de suggestions automatiques (IA)
- Pas d'import automatique depuis Indeed
- Pas de paiement ni de système multi-organisation
- API en lecture seule, authentifiée par cookie plutôt que par jeton dédié
- Pas encore de déploiement cloud automatisé (Azure App Service, etc.)

## Évolutions futures

- Déploiement automatisé (Azure App Service / Container Apps)
- Export Excel multi-feuilles
- API complète (création/modification) avec authentification par jeton
- Fonctionnalités intelligentes (suggestions de relance, comparaison CV/offre)
