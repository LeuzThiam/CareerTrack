# CareerTrack

Application web ASP.NET Core permettant de gérer et d'analyser une recherche d'emploi : offres, candidatures, statuts, relances, entrevues, contacts, documents et statistiques.

## Contexte

Une personne en recherche d'emploi utilise souvent plusieurs sources (Indeed, LinkedIn, Jobillico, Guichet-Emplois, sites d'entreprises, recommandations, candidatures spontanées...). Au fil du temps, il devient facile de perdre le fil : oublier où l'on a postulé, oublier une relance, perdre le lien d'une offre, ne plus savoir quelle version du CV a été envoyée, etc.

## Problématique

> Comment construire une application web sécurisée permettant de centraliser, suivre et analyser toutes les étapes d'une recherche d'emploi ?

## Fonctionnalités prévues

- Création de compte et connexion sécurisée
- Enregistrement des offres d'emploi et des candidatures envoyées
- Suivi des changements de statut avec historique complet
- Planification et suivi des relances
- Gestion des entrevues (type, date, préparation, compte rendu, résultat)
- Gestion des contacts professionnels (recruteurs, gestionnaires)
- Pièces jointes (CV, lettres, documents)
- Tableau de bord avec indicateurs (taux de réponse, d'entrevue, de conversion...)
- Recherche, filtrage et tri des candidatures
- Export des données (CSV / Excel)
- Isolation complète des données par utilisateur

## Utilisateurs

- **Candidat** : utilisateur principal, gère ses propres candidatures
- **Conseiller en emploi** *(version avancée)* : accès limité aux données d'un candidat qui l'y autorise
- **Administrateur** : gestion des comptes et des paramètres globaux, sans accès automatique au contenu privé des candidatures

## Règles métier principales

- Un utilisateur ne peut consulter que ses propres données
- Une candidature doit être associée à une entreprise et posséder un titre de poste
- Chaque changement de statut crée une entrée dans l'historique
- Les transitions de statut invalides sont bloquées (ex. `ToApply → OfferAccepted`)
- Suppression logique plutôt que définitive (archivage)
- Détection des candidatures potentiellement en doublon

## Architecture

Le projet démarre en architecture simple (un seul projet MVC) puis évoluera vers une séparation en couches :

```
CareerTrack/
├── Controllers/
├── Data/
├── Models/
├── Services/
├── ViewModels/
├── Views/
├── wwwroot/
├── Program.cs
└── appsettings.json
```

Évolution prévue vers quatre projets : `Domain`, `Application`, `Infrastructure`, `Web`.

## Technologies

- C# / .NET
- ASP.NET Core MVC
- Entity Framework Core
- SQL Server
- ASP.NET Core Identity
- Bootstrap
- xUnit (tests)
- Docker *(à venir)*

## Modèle de données

Entités principales : `ApplicationUser`, `Company`, `JobOffer`, `JobApplication`, `ApplicationStatusHistory`, `FollowUp`, `Interview`, `Contact`, `Document`, `Note`, `Skill`.

## Installation

```bash
git clone https://github.com/<ton-utilisateur>/careertrack-dotnet.git
cd careertrack-dotnet
dotnet restore
```

## Configuration

Configurer la chaîne de connexion dans `appsettings.Development.json` ou via `dotnet user-secrets`.

## Migrations

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Tests

```bash
dotnet test
```

## Statut du projet

🚧 En développement — Phase 3 (initialisation) complétée. Voir la feuille de route interne pour les phases suivantes.

## Limites actuelles (V1)

- Pas encore d'application mobile
- Pas encore d'IA / suggestions automatiques
- Pas encore d'import automatique depuis Indeed
- Pas encore de microservices

## Évolutions futures

- API REST + Swagger
- Docker + CI/CD (GitHub Actions)
- Déploiement Azure
- Fonctionnalités intelligentes (suggestions de relance, comparaison CV/offre)
