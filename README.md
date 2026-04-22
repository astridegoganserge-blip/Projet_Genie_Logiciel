# EasySave 1.0

Logiciel de sauvegarde en ligne de commande développé en **C# / .NET 8**.
Application console bilingue (français / anglais) permettant de créer, gérer et exécuter jusqu'à 5 travaux de sauvegarde complets ou différentiels.

---

## Fonctionnalités

- Gestion de **5 travaux de sauvegarde** maximum, identifiés par un ID unique (1 à 5).
- Deux stratégies de sauvegarde :
  - **Complète** : copie intégrale des fichiers de la source vers la cible.
  - **Différentielle** : copie uniquement des fichiers nouveaux ou modifiés depuis la dernière sauvegarde.
- Prise en charge des chemins **locaux, externes et UNC** (réseau).
- Interface console **bilingue FR / EN** (choix de la langue au lancement).
- **Journalisation temps réel** de chaque transfert de fichier (librairie `EasyLog`) : un fichier JSON par jour dans `logs/`.
- **Suivi d'état en temps réel** (`state.json`) : nom du travail en cours, statut, nombre et taille des fichiers restants, fichier source / cible en cours.
- **Persistance** des travaux configurés dans `jobs.config.json`.
- **Exécution en ligne de commande** : possibilité de lancer un ou plusieurs travaux directement au démarrage de l'application.

---

## Prérequis

- **.NET 8.0 SDK** ou runtime équivalent
- Système : Windows, Linux ou macOS
- Le projet dépend d'un projet frère `EasyLog` (référencé via `..\EasyLog\EasyLog.csproj`)

---

## Architecture du projet

```
EasySave/
├── Program.cs                         # Point d'entrée, sélection de la langue
├── EasySave.csproj
├── EasySave.slnx                      # Solution (inclut EasyLog)
│
├── Managers/
│   └── BackupManager.cs              # Gestion CRUD + exécution des travaux
│
├── Models/
│   ├── BackupJob.cs                  # Modèle d'un travail de sauvegarde
│   └── BackupTypes.cs                # Enum : Complete / Differential
│
├── Services/
│   ├── CommandLineParser.cs          # Analyse des commandes (1, 1-3, 1;3;5)
│   ├── ConfigurationManager.cs       # Persistance JSON des travaux
│   ├── LanguageManager.cs            # Chargement des traductions
│   └── StateTracker.cs               # Écriture temps réel de state.json
│
├── Strategies/
│   ├── IBackupStrategy.cs            # Interface commune (pattern Strategy)
│   ├── CompleteBackupStrategy.cs     # Sauvegarde complète
│   └── DifferentialBackupStrategy.cs # Sauvegarde différentielle
│
├── UI/
│   └── ConsoleInterface.cs           # Menu console et saisies utilisateur
│
└── Resources/
    ├── fr.json                       # Traductions françaises
    └── en.json                       # Traductions anglaises
```

Le projet applique le **pattern Strategy** pour découpler les types de sauvegarde, ce qui permet d'ajouter facilement de nouvelles stratégies sans modifier `BackupManager`.

---

## Compilation

Depuis le dossier `EasySave/` :

```bash
dotnet build
```

Le binaire est généré dans `bin/Debug/net8.0/` (ou `bin/Release/net8.0/` en release).

Pour une build de release :

```bash
dotnet build -c Release
```

---

## Utilisation

### Mode interactif (menu console)

```bash
dotnet run
```

Au lancement, l'utilisateur choisit la langue (FR / EN), puis accède au menu principal :

```
**** EasySave 1.0 ****
1. Créer un travail de sauvegarde
2. Exécuter un travail
3. Exécution séquentielle (ex: 1-3 ou 1;3)
4. Liste des travaux configurés
5. Supprimer un travail
0. Quitter l'application
```

### Mode ligne de commande

Il est possible de lancer directement un ou plusieurs travaux via un argument :

| Syntaxe       | Effet                                                    |
|---------------|----------------------------------------------------------|
| `1`           | Exécute le travail n°1                                   |
| `1-3`         | Exécute les travaux 1 à 3 (plage)                        |
| `1;3;5`       | Exécute les travaux 1, 3 et 5 (sélection)                |

Exemples :

```bash
EasySave.exe 1
EasySave.exe 1-3
EasySave.exe 1;3;5
```

---

## Fichiers générés à l'exécution

Tous les fichiers sont écrits à côté de l'exécutable (`AppContext.BaseDirectory`).

### `jobs.config.json`
Liste persistée des travaux configurés.

```json
[
  {
    "Id": 1,
    "Name": "backup_docs",
    "SourcePath": "C:\\Users\\HP\\Desktop\\SAUV1",
    "TargetPath": "C:\\Users\\HP\\Desktop\\Backup",
    "Type": 0,
    "LastExecutionTime": "2026-04-22T01:02:55.53+02:00"
  }
]
```
> `Type` : `0` = Complète, `1` = Différentielle.

### `state.json`
État temps réel du travail en cours (mis à jour après chaque fichier copié).

```json
{
  "BackupName": "backup_docs",
  "LastActionTime": "2026-04-22T07:55:11+02:00",
  "Status": "Completed",
  "TotalFiles": 29,
  "TotalSize": 27345,
  "RemainingFiles": 0,
  "RemainingSize": 0,
  "CurrentSourceFile": "",
  "CurrentTargetFile": ""
}
```

### `logs/YYYY-MM-DD.json`
Un journal par jour, listant chaque transfert de fichier.

```json
[
  {
    "Timestamp": "2026-04-22T01:02:42+02:00",
    "BackupName": "backup_docs",
    "SourceFile": "C:\\...\\document.pdf",
    "TargetFile": "C:\\...\\Backup\\document.pdf",
    "FileSize": 81237,
    "TransferTimeMs": 1
  }
]
```

---

## Internationalisation

Les libellés du menu et des messages sont définis dans `Resources/fr.json` et `Resources/en.json`.
Pour ajouter une langue, créer un nouveau fichier JSON à la racine `Resources/` et adapter `LanguageManager.cs`.

---

## Auteurs

Projet réalisé dans le cadre d'un projet école (CESI).

## Version

**EasySave 1.0** — Avril 2026