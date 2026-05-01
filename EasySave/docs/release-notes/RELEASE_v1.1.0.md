# Release Note — EasySave v1.1.0

## Version

EasySave v1.1.0

## Type de version

Version mineure basée sur EasySave v1.0.

Cette version conserve le fonctionnement console de la version 1.0 et ajoute principalement la possibilité de choisir le format du fichier log journalier.

## Fonctionnalités conservées depuis EasySave v1.0

- Application console.
- Gestion de 5 travaux de sauvegarde maximum.
- Création, affichage, suppression et exécution des travaux de sauvegarde.
- Sauvegarde complète.
- Sauvegarde différentielle.
- Exécution d’un travail unique.
- Exécution séquentielle via menu ou ligne de commande.
- Interface bilingue français / anglais.
- Fichier d’état temps réel `state.json`.
- Journalisation via la bibliothèque séparée `EasyLog.dll`.

## Nouveautés EasySave v1.1

- Ajout du choix du format de log journalier : JSON ou XML.
- Ajout d’un menu Paramètres.
- Sauvegarde des paramètres utilisateur dans `settings.json`.
- Ajout des repositories pour mieux séparer la persistance :
  - `IJobRepository`
  - `JsonJobRepository`
  - `ISettingsRepository`
  - `JsonSettingsRepository`
- Refactorisation vers une architecture console MVC :
  - `Controllers`
  - `Views`
  - `Models`
  - `Repositories`
  - `Services`
  - `Strategies`
- Allègement de `Program.cs`.
- Ajout de `ConsoleApplicationController` pour centraliser le flux applicatif console.
- Uniformisation des statuts du fichier d’état :
  - `Actif`
  - `Terminé`
  - `Erreur`
- Ajout du champ `Progression` dans `state.json`.
- Préparation d’EasyLog pour les versions futures avec le champ `EncryptionTimeMs`.

## Compatibilité

EasySave v1.1 reste compatible avec les fonctionnalités de la version 1.0.