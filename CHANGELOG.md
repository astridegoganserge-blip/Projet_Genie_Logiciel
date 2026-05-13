# Changelog

## [EN] Version history

### [v3.0.0] - 2026-05-13

#### Added
- Parallel backup execution (replaces sequential mode).
- `JobExecutionContext` to carry per-job pause and stop tokens.
- `BackupStrategyBase` abstract Template Method class to eliminate duplication between Complete and Differential strategies.
- Priority files: non-priority files are blocked while any priority file is pending on at least one job (global barrier via `PriorityFileFilter`).
- Large-file throttling: only one file above the configurable size threshold is transferred at a time across all backups (via `LargeFileThrottle`).
- Per-job and global Play / Pause / Stop controls in the GUI.
- Automatic pause of all backup jobs when the business software is detected, with automatic resume.
- CryptoSoft single-instance enforcement via a named global mutex.
- New `EasySave.LogServer` Docker service for centralized daily log collection.
- Three log modes: `Local`, `Docker`, `Both`.
- `MachineName` field on every centralized log entry.
- `MaxFileSizeKb`, `PriorityExtensions`, `LogMode`, `DockerLogServerUrl` settings.
- Differential backup orphan-file cleanup option (`DeleteOrphanFilesInDifferential`).
- Three V3.0 UML diagrams (use case, class, sequence).

#### Changed
- `BackupManager` now supports `ExecuteParallel(ids, logger)` alongside `ExecuteJob(id, logger)`.
- State tracking exposes new statuses: `EnPause`, `Interrompu`.
- `CompleteBackupStrategy` and `DifferentialBackupStrategy` now derive from `BackupStrategyBase` and only override `SelectSourceFiles` and `RunPostCopyStep`. Strategies code went from 687 to 542 lines (-21 %), 150+ duplicated lines removed.
- `LogServer` write path serialized through a `SemaphoreSlim` to prevent line interleaving under concurrent client load.

#### Fixed
- Removed parasitic `Console.WriteLine` and 1-second `Thread.Sleep` in `CryptoSoft.FileManager.CheckFile`.

#### Not included
- Live reload of the business software watcher when the process name is changed at runtime (requires application restart).
- XML log append-mode optimization (current implementation rewrites the file per entry, planned for v4.0).

---

### [v2.0.0] - 2026-05-04

#### Added
- WPF graphical interface based on the MVVM architecture.
- `EasySave.Core` class library shared between console and GUI projects.
- Unlimited number of backup jobs (previous cap of 5 removed in GUI).
- Visible auto-incremented job number (01, 02, 03...) on top of internal `Guid`.
- Sequential execution of all jobs from the GUI.
- CryptoSoft integration: extensions listed in settings are encrypted after copy.
- `EncryptionTimeMs` field in EasyLog entries (0 = none, > 0 = success, < 0 = error).
- Business software detection: blocks job execution when the configured process is running.
- Live in-app language switch (FR / EN).
- Enriched job state with real-time progression.

#### Changed
- Documentation moved to a top-level `Documentation/` folder.
- Console application kept at v1.1 feature parity and now references `EasySave.Core` to avoid duplication.

---

### [v1.1.0] - 2026-05-01

#### Added
- JSON / XML daily log format selection.
- Settings menu.
- Persistent user settings in `settings.json`.
- `IJobRepository` and `JsonJobRepository`.
- `ISettingsRepository` and `JsonSettingsRepository`.
- `JobController`, `SettingsController`, `ConsoleApplicationController`.
- Console views: `JobView`, `SettingsView`, `LanguageView`.
- `Progression` field in `state.json`.
- `EncryptionTimeMs` field in EasyLog entries for future compatibility.

#### Changed
- Console application refactored toward a clearer MVC structure.
- `Program.cs` simplified into a lightweight entry point.
- State statuses standardized: `Actif`, `Terminé`, `Erreur`.
- Documentation updated for .NET 10.0.

#### Fixed
- Language resource loading.
- Immediate use of the selected JSON / XML log format.
- File transfer error logging with negative transfer time.

#### Not included
- WPF graphical interface.
- CryptoSoft encryption.
- Business software detection.
- Play / Pause / Stop controls.

---

## [FR] Historique des versions

### [v3.0.0] - 2026-05-13

#### Ajouté
- Exécution parallèle des sauvegardes (remplace le mode séquentiel).
- `JobExecutionContext` pour porter les jetons de pause et d'arrêt de chaque travail.
- Classe abstraite `BackupStrategyBase` (Template Method) pour éliminer la duplication entre les stratégies complète et différentielle.
- Fichiers prioritaires : les fichiers non prioritaires sont bloqués tant qu'un fichier prioritaire reste à traiter sur au moins un travail (barrière globale via `PriorityFileFilter`).
- Limitation des transferts simultanés de fichiers volumineux : un seul fichier au-dessus du seuil paramétrable peut être transféré à la fois, toutes sauvegardes confondues (via `LargeFileThrottle`).
- Commandes Play / Pause / Stop par travail et globales dans l'interface graphique.
- Mise en pause automatique de toutes les sauvegardes lorsque le logiciel métier est détecté, avec reprise automatique.
- CryptoSoft mono-instance via mutex global nommé.
- Nouveau service Docker `EasySave.LogServer` pour la centralisation des logs journaliers.
- Trois modes de log : `Local`, `Docker`, `Both`.
- Champ `MachineName` sur chaque entrée de log centralisée.
- Paramètres `MaxFileSizeKb`, `PriorityExtensions`, `LogMode`, `DockerLogServerUrl`.
- Option de suppression des fichiers orphelins en sauvegarde différentielle (`DeleteOrphanFilesInDifferential`).
- Trois diagrammes UML V3.0 (cas d'utilisation, classe, séquence).

#### Modifié
- `BackupManager` expose désormais `ExecuteParallel(ids, logger)` en plus de `ExecuteJob(id, logger)`.
- Le suivi d'état expose de nouveaux statuts : `EnPause`, `Interrompu`.
- `CompleteBackupStrategy` et `DifferentialBackupStrategy` héritent désormais de `BackupStrategyBase` et ne redéfinissent que `SelectSourceFiles` et `RunPostCopyStep`. Le code des stratégies passe de 687 à 542 lignes (-21 %), avec plus de 150 lignes dupliquées supprimées.
- Chemin d'écriture du `LogServer` sérialisé via un `SemaphoreSlim` pour éviter l'entrelacement des lignes sous charge concurrente.

#### Corrigé
- Suppression d'un `Console.WriteLine` et d'un `Thread.Sleep(1000)` parasites dans `CryptoSoft.FileManager.CheckFile`.

#### Non inclus
- Rechargement à chaud du surveillant de logiciel métier en cas de changement du nom de processus (un redémarrage de l'application reste nécessaire).
- Optimisation du log XML en mode append (l'implémentation actuelle réécrit le fichier à chaque entrée, prévue pour la v4.0).

---

### [v2.0.0] - 2026-05-04

#### Ajouté
- Interface graphique WPF fondée sur l'architecture MVVM.
- Bibliothèque `EasySave.Core` partagée entre la console et l'interface graphique.
- Nombre illimité de travaux de sauvegarde (suppression de la limite à 5 dans la version graphique).
- Numéro visible auto-incrémenté (01, 02, 03...) en plus du `Guid` interne.
- Exécution séquentielle de tous les travaux depuis la GUI.
- Intégration de CryptoSoft : les extensions définies dans les paramètres sont chiffrées après copie.
- Champ `EncryptionTimeMs` dans les entrées EasyLog (0 = aucun, > 0 = succès, < 0 = erreur).
- Détection de logiciel métier : bloque l'exécution d'un travail si le processus configuré est actif.
- Changement de langue dynamique dans l'application (FR / EN).
- Suivi enrichi de l'état avec progression en temps réel.

#### Modifié
- Documentation déplacée dans un dossier `Documentation/` à la racine.
- Application console maintenue au niveau fonctionnel v1.1 et adaptée pour référencer `EasySave.Core` afin d'éviter la duplication.

---

### [v1.1.0] - 2026-05-01

#### Ajouté
- Ajout du choix du format de log journalier JSON/XML.
- Ajout d'un menu Paramètres.
- Ajout de la persistance des paramètres utilisateur dans `settings.json`.
- Ajout de `IJobRepository` et `JsonJobRepository`.
- Ajout de `ISettingsRepository` et `JsonSettingsRepository`.
- Ajout de `JobController`, `SettingsController` et `ConsoleApplicationController`.
- Ajout des vues console : `JobView`, `SettingsView`, `LanguageView`.
- Ajout du champ `Progression` dans `state.json`.
- Ajout du champ `EncryptionTimeMs` dans EasyLog pour compatibilité future.

#### Modifié
- Refactorisation de l'application console vers une architecture MVC plus claire.
- Simplification de `Program.cs` comme point d'entrée léger.
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
