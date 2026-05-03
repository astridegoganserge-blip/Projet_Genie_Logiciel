# Changelog

## [EN] Version history

### [v1.1.0] - 2026-05-01

#### Added
- Added JSON/XML daily log format selection.
- Added Settings menu.
- Added persistent user settings in `settings.json`.
- Added `IJobRepository` and `JsonJobRepository`.
- Added `ISettingsRepository` and `JsonSettingsRepository`.
- Added `JobController`, `SettingsController` and `ConsoleApplicationController`.
- Added console views: `JobView`, `SettingsView`, `LanguageView`.
- Added `Progression` field in `state.json`.
- Added `EncryptionTimeMs` field in EasyLog entries for future compatibility.

#### Changed
- Refactored the console application toward a clearer MVC structure.
- Simplified `Program.cs` as a lightweight entry point.
- Standardized state statuses: `Actif`, `Terminé`, `Erreur`.
- Updated documentation for .NET 10.0.

#### Fixed
- Fixed language resource loading.
- Fixed immediate use of selected JSON/XML log format.
- Fixed file transfer error logging with negative transfer time.

#### Not included
- WPF graphical interface.
- CryptoSoft encryption.
- Business software detection.
- Play / Pause / Stop controls.

---

## [FR] Historique des versions

### [v1.1.0] - 2026-05-01

#### Ajouté
- Ajout du choix du format de log journalier JSON/XML.
- Ajout d’un menu Paramètres.
- Ajout de la persistance des paramètres utilisateur dans `settings.json`.
- Ajout de `IJobRepository` et `JsonJobRepository`.
- Ajout de `ISettingsRepository` et `JsonSettingsRepository`.
- Ajout de `JobController`, `SettingsController` et `ConsoleApplicationController`.
- Ajout des vues console : `JobView`, `SettingsView`, `LanguageView`.
- Ajout du champ `Progression` dans `state.json`.
- Ajout du champ `EncryptionTimeMs` dans EasyLog pour compatibilité future.

#### Modifié
- Refactorisation de l’application console vers une architecture MVC plus claire.
- Simplification de `Program.cs` comme point d’entrée léger.
- Uniformisation des statuts : `Actif`, `Terminé`, `Erreur`.
- Mise à jour de la documentation pour .NET 10.0.

#### Corrigé
- Correction du chargement des fichiers de langue.
- Correction de la prise en compte immédiate du format de log JSON/XML sélectionné.
- Correction de la journalisation des erreurs de transfert avec un temps négatif.

#### Non inclus
- Interface graphique WPF.
- Chiffrement CryptoSoft.
- Détection du logiciel métier.
- Commandes Play / Pause / Stop.