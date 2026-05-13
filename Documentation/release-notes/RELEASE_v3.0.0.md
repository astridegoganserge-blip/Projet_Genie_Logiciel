# Release note — EasySave v3.0.0

## Version concernée

EasySave v3.0.0

## Type de version

Version majeure.

Cette version transforme EasySave en outil de sauvegarde multi-tâches concurrent, capable d'exécuter plusieurs travaux en parallèle, de réagir en temps réel aux interactions utilisateur, et de centraliser les logs sur un service Docker partagé entre plusieurs serveurs clients.

---

## Objectif de la version

EasySave v3.0 répond aux demandes d'évolution de la direction ProSoft suite au retour client de la version 2.0. L'objectif est de tirer parti des architectures multi-cœurs et des déploiements multi-serveurs sans dégrader la robustesse acquise.

La version 3.0 permet notamment :

- d'exécuter les travaux de sauvegarde en parallèle ;
- de prioriser des extensions de fichiers via une barrière globale ;
- de limiter les transferts simultanés de fichiers volumineux ;
- de mettre en pause / reprendre / arrêter chaque travail individuellement ou globalement ;
- de mettre en pause automatiquement toutes les sauvegardes lorsqu'un logiciel métier est détecté, avec reprise automatique ;
- d'imposer que CryptoSoft fonctionne en mono-instance ;
- de centraliser les fichiers de logs journaliers sur un serveur Docker dédié.

---

## Nouveautés principales

### Exécution en parallèle

Le mode séquentiel est remplacé par une exécution parallèle.

Chaque travail dispose désormais de son propre contexte d'exécution `JobExecutionContext`, qui porte :

- un `CancellationToken` pour l'arrêt immédiat ;
- un drapeau de pause pour l'interruption douce après le fichier en cours.

Le `BackupManager` expose une méthode `ExecuteParallel(ids, logger)` qui lance les travaux via `Task.WhenAll`.

---

### Gestion des fichiers prioritaires

Une liste d'extensions prioritaires est configurable dans les paramètres généraux.

Aucun fichier non prioritaire ne peut être transféré tant qu'un fichier prioritaire reste à traiter sur l'un des travaux actifs (barrière globale gérée par `PriorityFileFilter`).

Les fichiers prioritaires sont également placés en tête de la file de transfert de chaque travail.

---

### Limitation des transferts simultanés de fichiers volumineux

Un seuil de taille en kilo-octets est configurable dans les paramètres généraux.

Lorsqu'un fichier dépasse ce seuil, son transfert acquiert un `SemaphoreSlim(1, 1)` global : un seul fichier volumineux peut être transféré à la fois, toutes sauvegardes confondues.

Les fichiers plus petits continuent de transférer en parallèle sans contrainte.

---

### Contrôle Play / Pause / Stop

L'interface graphique expose désormais six commandes :

- Pause / Reprendre / Arrêter, par travail ;
- Pause All / Resume All / Stop All, globales.

La pause s'applique après le transfert du fichier en cours. L'arrêt interrompt immédiatement le travail et marque l'état comme `Interrompu`.

---

### Pause automatique sur logiciel métier

Lorsqu'un logiciel métier est configuré, le service `BusinessSoftwareWatcher.StartContinuousWatch` surveille en continu sa présence.

Si le logiciel métier démarre pendant une sauvegarde, tous les travaux passent automatiquement en pause via `PauseAll`. À l'arrêt du logiciel métier, les travaux reprennent automatiquement via `ResumeAll`.

---

### CryptoSoft mono-instance

`CryptoSoft.exe` refuse désormais une seconde instance simultanée.

L'unicité est garantie par un mutex global nommé `Global\CryptoSoft_ProSoft_Mutex`.

Côté EasySave, le service `CryptoService` attend la disponibilité de CryptoSoft via le même mutex avant de lancer le chiffrement, avec un délai d'expiration de 30 secondes.

---

### Centralisation des logs Docker

Un nouveau service `EasySave.LogServer` containerisé est fourni dans le dépôt.

Il expose deux endpoints HTTP :

- `GET /` : statut du service ;
- `POST /log` : réception d'une entrée de log.

Le service écrit toutes les entrées dans un fichier journalier unique `yyyy-MM-dd.ndjson` du dossier `centralized-logs`. Les écritures concurrentes sont sérialisées par un `SemaphoreSlim` pour éviter l'entrelacement des lignes.

Trois modes de fonctionnement sont disponibles côté client :

- **Local** : logs uniquement sur le poste utilisateur ;
- **Docker** : logs uniquement sur le serveur Docker centralisé ;
- **Both** : logs sur les deux à la fois.

Chaque entrée envoyée porte le champ `MachineName` pour différencier les machines émettrices.

---

## Architecture

La V3 ajoute les classes suivantes par rapport à la V2 :

| Composant | Rôle |
|---|---|
| `BackupStrategyBase` | Classe abstraite Template Method partagée par les deux stratégies. |
| `JobExecutionContext` | Contexte d'exécution par travail (pause, stop). |
| `PriorityFileFilter` | Barrière globale des fichiers prioritaires. |
| `LargeFileThrottle` | Sémaphore global des fichiers volumineux. |
| `LogCentralizer` | Client HTTP vers le service Docker. |
| `EasySave.LogServer` | Service Docker de centralisation des logs. |

---

## Compatibilité

- Les fichiers `jobs.config.json` et `state.json` restent compatibles avec la version 2.0.
- Le fichier `settings.json` accepte de nouveaux champs optionnels (`PriorityExtensions`, `MaxFileSizeKb`, `LogMode`, `DockerLogServerUrl`). Les paramètres existants sont conservés.
- Les anciennes valeurs par défaut s'appliquent en l'absence de configuration explicite (`LogMode = Local`, `MaxFileSizeKb = 0` désactive le throttling).

---

## Limites connues

- Le surveillant du logiciel métier est démarré au lancement de l'application avec la valeur initiale du paramètre. Un changement du nom de processus dans les paramètres nécessite un redémarrage pour être pris en compte.
- Le format de log XML applique une stratégie de réécriture complète à chaque entrée. Une optimisation en mode append est prévue en version 4.0.

---

## Équipe

- Anelka MAPA
- Astride Gogan
- Luc Dai

---

## Licence

Projet académique CESI dans le cadre de la fiction pédagogique ProSoft.
# Release note — EasySave v3.0.0

## Version concernée

EasySave v3.0.0

## Type de version

Version majeure.

Cette version transforme EasySave en outil de sauvegarde multi-tâches concurrent, capable d'exécuter plusieurs travaux en parallèle, de réagir en temps réel aux interactions utilisateur, et de centraliser les logs sur un service Docker partagé entre plusieurs serveurs clients.

---

## Objectif de la version

EasySave v3.0 répond aux demandes d'évolution de la direction ProSoft suite au retour client de la version 2.0. L'objectif est de tirer parti des architectures multi-cœurs et des déploiements multi-serveurs sans dégrader la robustesse acquise.

La version 3.0 permet notamment :

- d'exécuter les travaux de sauvegarde en parallèle ;
- de prioriser des extensions de fichiers via une barrière globale ;
- de limiter les transferts simultanés de fichiers volumineux ;
- de mettre en pause / reprendre / arrêter chaque travail individuellement ou globalement ;
- de mettre en pause automatiquement toutes les sauvegardes lorsqu'un logiciel métier est détecté, avec reprise automatique ;
- d'imposer que CryptoSoft fonctionne en mono-instance ;
- de centraliser les fichiers de logs journaliers sur un serveur Docker dédié.

---

## Nouveautés principales

### Exécution en parallèle

Le mode séquentiel est remplacé par une exécution parallèle.

Chaque travail dispose désormais de son propre contexte d'exécution `JobExecutionContext`, qui porte :

- un `CancellationToken` pour l'arrêt immédiat ;
- un drapeau de pause pour l'interruption douce après le fichier en cours.

Le `BackupManager` expose une méthode `ExecuteParallel(ids, logger)` qui lance les travaux via `Task.WhenAll`.

---

### Gestion des fichiers prioritaires

Une liste d'extensions prioritaires est configurable dans les paramètres généraux.

Aucun fichier non prioritaire ne peut être transféré tant qu'un fichier prioritaire reste à traiter sur l'un des travaux actifs (barrière globale gérée par `PriorityFileFilter`).

Les fichiers prioritaires sont également placés en tête de la file de transfert de chaque travail.

---

### Limitation des transferts simultanés de fichiers volumineux

Un seuil de taille en kilo-octets est configurable dans les paramètres généraux.

Lorsqu'un fichier dépasse ce seuil, son transfert acquiert un `SemaphoreSlim(1, 1)` global : un seul fichier volumineux peut être transféré à la fois, toutes sauvegardes confondues.

Les fichiers plus petits continuent de transférer en parallèle sans contrainte.

---

### Contrôle Play / Pause / Stop

L'interface graphique expose désormais six commandes :

- Pause / Reprendre / Arrêter, par travail ;
- Pause All / Resume All / Stop All, globales.

La pause s'applique après le transfert du fichier en cours. L'arrêt interrompt immédiatement le travail et marque l'état comme `Interrompu`.

---

### Pause automatique sur logiciel métier

Lorsqu'un logiciel métier est configuré, le service `BusinessSoftwareWatcher.StartContinuousWatch` surveille en continu sa présence.

Si le logiciel métier démarre pendant une sauvegarde, tous les travaux passent automatiquement en pause via `PauseAll`. À l'arrêt du logiciel métier, les travaux reprennent automatiquement via `ResumeAll`.

---

### CryptoSoft mono-instance

`CryptoSoft.exe` refuse désormais une seconde instance simultanée.

L'unicité est garantie par un mutex global nommé `Global\CryptoSoft_ProSoft_Mutex`.

Côté EasySave, le service `CryptoService` attend la disponibilité de CryptoSoft via le même mutex avant de lancer le chiffrement, avec un délai d'expiration de 30 secondes.

---

### Centralisation des logs Docker

Un nouveau service `EasySave.LogServer` containerisé est fourni dans le dépôt.

Il expose deux endpoints HTTP :

- `GET /` : statut du service ;
- `POST /log` : réception d'une entrée de log.

Le service écrit toutes les entrées dans un fichier journalier unique `yyyy-MM-dd.ndjson` du dossier `centralized-logs`. Les écritures concurrentes sont sérialisées par un `SemaphoreSlim` pour éviter l'entrelacement des lignes.

Trois modes de fonctionnement sont disponibles côté client :

- **Local** : logs uniquement sur le poste utilisateur ;
- **Docker** : logs uniquement sur le serveur Docker centralisé ;
- **Both** : logs sur les deux à la fois.

Chaque entrée envoyée porte le champ `MachineName` pour différencier les machines émettrices.

---

## Architecture

La V3 ajoute les classes suivantes par rapport à la V2 :

| Composant | Rôle |
|---|---|
| `BackupStrategyBase` | Classe abstraite Template Method partagée par les deux stratégies. |
| `JobExecutionContext` | Contexte d'exécution par travail (pause, stop). |
| `PriorityFileFilter` | Barrière globale des fichiers prioritaires. |
| `LargeFileThrottle` | Sémaphore global des fichiers volumineux. |
| `LogCentralizer` | Client HTTP vers le service Docker. |
| `EasySave.LogServer` | Service Docker de centralisation des logs. |

---

## Compatibilité

- Les fichiers `jobs.config.json` et `state.json` restent compatibles avec la version 2.0.
- Le fichier `settings.json` accepte de nouveaux champs optionnels (`PriorityExtensions`, `MaxFileSizeKb`, `LogMode`, `DockerLogServerUrl`). Les paramètres existants sont conservés.
- Les anciennes valeurs par défaut s'appliquent en l'absence de configuration explicite (`LogMode = Local`, `MaxFileSizeKb = 0` désactive le throttling).

---

## Limites connues

- Le surveillant du logiciel métier est démarré au lancement de l'application avec la valeur initiale du paramètre. Un changement du nom de processus dans les paramètres nécessite un redémarrage pour être pris en compte.
- Le format de log XML applique une stratégie de réécriture complète à chaque entrée. Une optimisation en mode append est prévue en version 4.0.

---

## Équipe

- Anelka MAPA
- Astride Gogan
- Luc Dai

---

## Licence

Projet académique CESI dans le cadre de la fiction pédagogique ProSoft.
