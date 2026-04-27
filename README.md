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

> Les guillemets autour de `1;3` sont nécessaires sous PowerShell, sinon le `;` est interprété comme un séparateur de commandes.

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
