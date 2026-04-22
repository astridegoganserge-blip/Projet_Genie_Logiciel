# EasySave 1.0

Logiciel de sauvegarde en ligne de commande développé pour ProSoft dans le cadre du projet fil rouge EasySave.

Cette première version couvre les besoins de base : créer des travaux de sauvegarde, les lancer à la demande, tracer ce qui se passe dans des fichiers de log journaliers, et garder un état temps réel de ce qui tourne.

## Ce que fait le logiciel

- Jusqu'à 5 travaux de sauvegarde configurables, identifiés par un numéro de 1 à 5
- Deux modes de copie : sauvegarde complète ou différentielle
- Interface console en français et en anglais (choix de la langue au démarrage)
- Exécution via menu interactif **ou** via argument en ligne de commande
- Un log journalier au format JSON, un par jour, qui consigne chaque fichier transféré
- Un fichier `state.json` unique qui reflète en temps réel l'avancement du travail en cours
- Support des disques locaux, externes et des lecteurs réseau (via chemins UNC)

## Stack technique

- C# / .NET 8.0
- Application console
- Sérialisation JSON via `System.Text.Json`
- Bibliothèque externe `EasyLog.dll` (développée à part) pour l'écriture des logs journaliers

## Arborescence du projet

```
EasySave/
├── Managers/
│   └── BackupManager.cs       -> gestion des travaux (CRUD, exécution)
├── Models/
│   ├── BackupJob.cs           -> représente un travail de sauvegarde
│   └── BackupTypes.cs         -> enum Complete / Differential
├── Services/
│   ├── ConfigurationManager.cs-> lecture/écriture de jobs.config.json
│   ├── StateTracker.cs        -> écriture du state.json temps réel
│   ├── LanguageManager.cs     -> chargement des traductions
│   └── CommandLineParser.cs   -> parsing des arguments "1-3" ou "1;3"
├── Strategies/
│   ├── IBackupStrategy.cs
│   ├── CompleteBackupStrategy.cs
│   └── DifferentialBackupStrategy.cs
├── UI/
│   └── ConsoleInterface.cs    -> menu et interactions console
├── Resources/
│   ├── fr.json                -> traductions françaises
│   └── en.json                -> traductions anglaises
└── Program.cs
```

Le pattern Strategy a été retenu pour les deux types de sauvegarde. L'idée c'est de pouvoir ajouter un troisième type plus tard (incrémental par exemple) sans toucher au `BackupManager`.

## Installation

Pré-requis : **.NET 8.0 SDK** minimum, Windows 10/11.

Cloner le dépôt puis ouvrir `EasySave.slnx` dans Visual Studio 2022. Faire un build en Release. L'exécutable se retrouve dans `EasySave/bin/Release/net8.0/EasySave.exe`.

En ligne de commande :

```
dotnet build -c Release
```

## Utilisation

### Mode interactif

Double-clic sur l'exécutable, ou depuis un terminal :

```
EasySave.exe
```

Au lancement le logiciel demande la langue puis affiche un menu avec les actions disponibles : créer un travail, exécuter un travail, exécution séquentielle, voir la liste, supprimer.

### Mode ligne de commande

On passe en argument les numéros des travaux à lancer. Deux syntaxes sont acceptées :

Plage de travaux (1 à 3 inclus) :
```
EasySave.exe 1-3
```

Liste de travaux (1 et 3 uniquement) :
```
EasySave.exe "1;3"
```

Un travail unique fonctionne aussi :
```
EasySave.exe 2
```

## Fichiers générés

### jobs.config.json
Stocke la configuration des travaux. Il est créé automatiquement à côté de l'exécutable dès que vous créez un premier job.

### logs/AAAA-MM-JJ.json
Un fichier par jour, dans un sous-dossier `logs/`. Chaque ligne de transfert contient :

- l'horodatage
- le nom du travail de sauvegarde
- le chemin source complet (format UNC)
- le chemin cible complet (format UNC)
- la taille du fichier en octets
- le temps de transfert en millisecondes (valeur négative en cas d'erreur)

### state.json
Fichier unique qui reflète l'état courant. Il est réécrit après chaque fichier copié et contient le nom du travail, son statut, le nombre total de fichiers, ce qu'il reste à faire, et les chemins source/cible du fichier en cours.

Les fichiers JSON sont écrits avec `WriteIndented = true` pour rester lisibles dans un simple Notepad, comme demandé dans le cahier des charges.

## Sauvegarde complète vs différentielle

La **complète** copie systématiquement tous les fichiers du répertoire source, qu'ils aient changé ou non.

La **différentielle** compare la date de dernière modification entre la source et la cible. Un fichier est copié uniquement s'il est absent côté cible, ou si la version source est plus récente. Pratique pour les sauvegardes quotidiennes où on ne veut pas recopier 200 Go à chaque fois.

## Choix d'architecture

Quelques décisions qui méritent d'être expliquées pour la reprise du projet par une autre équipe :

- **EasyLog en DLL séparée** : comme le cahier des charges le précise, la partie logging est isolée dans sa propre bibliothèque, versionnée dans un repo Git distinct. Les futures versions d'EasySave doivent pouvoir réutiliser la même DLL sans la modifier (sinon on casse la compatibilité promise).
- **Classes statiques pour StateTracker, ConfigurationManager et LanguageManager** : il n'y a qu'une instance logique de chacun pendant la vie du programme, on évite de les trimballer partout via injection. Ce choix sera peut-être revu en v2 avec l'arrivée du MVVM.
- **Ressources JSON pour les traductions** plutôt que des fichiers `.resx` classiques. C'est plus simple à éditer manuellement, et l'utilisateur final peut rajouter une langue sans recompiler.
- **Validation des chemins au moment de la création** du job et non à l'exécution, pour échouer vite.

## Limitations connues (v1.0)

- Le logiciel ne gère pas la parallélisation des travaux, ils s'exécutent un par un dans l'ordre demandé.
- Pas d'interface graphique (prévue pour la v2.0 en MVVM).
- Si un fichier source est verrouillé pendant la copie, le travail s'arrête. Pas de mécanisme de retry pour le moment.
- Le status dans `state.json` mélange un peu les langues (`"Actif"` en FR et `"Completed"` en EN selon l'état), à uniformiser.

## Équipe

Anelka MAPA
Astride Gogan
Luc Dai

## Licence

Projet académique, propriété ProSoft dans le cadre de la fiction pédagogique.
Projet réalisé dans le cadre du parcours CESI
=======
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
