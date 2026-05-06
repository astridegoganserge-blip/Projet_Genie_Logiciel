# EasySave

EasySave est un logiciel de sauvegarde développé en C# dans le cadre du projet fil rouge ProSoft.

Le projet contient plusieurs versions évolutives :

- **EasySave v1.0 / v1.1** : application console ;
- **EasySave v2.0** : interface graphique WPF fondée sur une architecture MVVM.

La version actuelle du projet est :

```text
EasySave v2.0.0
```

---

## Objectif du projet

EasySave permet de créer, configurer et exécuter des travaux de sauvegarde complets ou différentiels.

Le logiciel permet notamment :

- de sauvegarder des dossiers sources vers des dossiers cibles ;
- de gérer plusieurs travaux de sauvegarde ;
- d’exécuter un travail individuellement ;
- d’exécuter plusieurs travaux de manière séquentielle ;
- de suivre l’état d’exécution ;
- de générer des logs journaliers ;
- de choisir le format JSON ou XML pour les logs ;
- de chiffrer certains fichiers avec CryptoSoft ;
- de bloquer l’exécution si un logiciel métier est détecté ;
- d’utiliser l’application en français ou en anglais.

---

## Versions disponibles

### EasySave v1.1 — Console

La version console permet :

- de créer jusqu’à 5 travaux de sauvegarde ;
- d’exécuter un travail depuis un menu interactif ;
- d’exécuter un ou plusieurs travaux en ligne de commande ;
- de choisir la langue au démarrage ;
- de générer des logs JSON ou XML ;
- de produire un fichier `state.json`.

### EasySave v2.0 — Interface graphique

La version graphique ajoute :

- une interface WPF ;
- une architecture MVVM ;
- un projet métier séparé `EasySave.Core` ;
- un nombre illimité de travaux ;
- une numérotation visible auto-incrémentée : `01`, `02`, `03` ;
- des identifiants techniques en `Guid` ;
- l’exécution séquentielle depuis la GUI ;
- le changement dynamique de langue FR/EN ;
- l’intégration de CryptoSoft ;
- la détection d’un logiciel métier bloquant ;
- un suivi d’état enrichi ;
- une interface graphique modernisée.

---

## Stack technique

- C#
- .NET 10.0
- WPF
- MVVM
- JSON avec `System.Text.Json`
- XML avec `XmlSerializer`
- EasyLog pour la journalisation
- CryptoSoft pour le chiffrement externe

---

## Arborescence principale

```text
Projet_Genie_Logiciel/
├── EasySave/
│   ├── Controllers/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   ├── Strategies/
│   ├── Views/
│   ├── docs/
│   └── Program.cs
│
├── EasySave.Core/
│   ├── Managers/
│   ├── Models/
│   ├── Repositories/
│   ├── Services/
│   └── Strategies/
│
├── EasySave.GUI/
│   ├── Resources/
│   ├── Services/
│   ├── ViewModels/
│   ├── Views/
│   ├── App.xaml
│   └── MainWindow.xaml
│
├── EasyLog/
├── CryptoSoft/
└── README.md
```

---

## Rôle des projets

| Projet | Rôle |
|---|---|
| `EasySave` | Application console historique V1/V1.1 |
| `EasySave.Core` | Cœur métier de la V2 |
| `EasySave.GUI` | Interface graphique WPF V2 |
| `EasyLog` | Bibliothèque de journalisation |
| `CryptoSoft` | Outil externe de chiffrement |

---

## Architecture V2

La version 2.0 applique une organisation de type MVVM côté interface graphique.

```text
EasySave.GUI
    ↓
Views
    ↓
ViewModels
    ↓
EasySave.Core.Managers.BackupManager
    ↓
Repositories + Services + Strategies
    ↓
EasyLog + CryptoSoft
```

### Rôle des couches

| Couche | Rôle |
|---|---|
| Views | Affichage WPF |
| ViewModels | État écran et commandes |
| Managers | Orchestration métier |
| Repositories | Persistance JSON |
| Services | CryptoSoft, StateTracker, BusinessSoftwareWatcher |
| Strategies | Sauvegarde complète et différentielle |
| EasyLog | Logs journaliers |
| CryptoSoft | Chiffrement externe |

---

## Prérequis

- Windows 10 ou supérieur
- .NET 10.0 SDK
- Visual Studio ou Visual Studio Code
- Droits de lecture sur les dossiers sources
- Droits d’écriture sur les dossiers cibles
- Droits d’exécution pour `CryptoSoft.exe`

---

## Compilation

Depuis la racine du dépôt :

```bash
dotnet build CryptoSoft/CryptoSoft.csproj
dotnet build EasySave.Core/EasySave.Core.csproj
dotnet build EasySave.GUI/EasySave.GUI.csproj
```

Pour compiler uniquement la version console :

```bash
dotnet build EasySave/EasySave.csproj
```

---

## Lancer EasySave v2.0

Depuis la racine du dépôt :

```bash
dotnet run --project EasySave.GUI/EasySave.GUI.csproj
```

L’application graphique s’ouvre avec les sections suivantes :

- Backup jobs / Travaux ;
- Create job / Créer un job ;
- Settings / Paramètres ;
- Execution / Exécution.

---

## Lancer EasySave v1.1 console

Depuis la racine :

```bash
dotnet run --project EasySave/EasySave.csproj
```

Ou après build, depuis le dossier de sortie :

```bash
EasySave.exe
```

La version console accepte aussi des arguments :

```bash
EasySave.exe 1
EasySave.exe 1-3
EasySave.exe "1;3"
```

---

## Fonctionnalités V2 principales

### Gestion illimitée des travaux

La V2 ne limite plus le nombre de travaux à 5.

Chaque travail contient :

- un `Guid` technique ;
- un numéro visible ;
- un nom ;
- un chemin source ;
- un chemin cible ;
- un type de sauvegarde ;
- une date de dernière exécution.

Exemple de numérotation visible :

```text
01
02
03
```

---

### Types de sauvegarde

EasySave prend en charge deux types de sauvegarde.

| Type | Description |
|---|---|
| Complète | Copie tous les fichiers du dossier source |
| Différentielle | Copie uniquement les fichiers nouveaux ou modifiés |

---

### Exécution séquentielle

La V2 permet d’exécuter tous les travaux dans l’ordre de leur numéro visible.

Exemple :

```text
01 → 02 → 03
```

À la fin, l’interface affiche :

```text
Sequential execution completed.
```

---

### Logs JSON/XML

Le format des logs est configurable depuis l’interface graphique.

Les logs sont générés dans :

```text
logs/
```

Exemples :

```text
2026-05-04.json
2026-05-04.xml
```

Chaque entrée contient :

- horodatage ;
- nom du travail ;
- fichier source ;
- fichier cible ;
- taille du fichier ;
- temps de transfert ;
- temps de chiffrement.

---

### Chiffrement CryptoSoft

CryptoSoft est intégré à EasySave v2.0.

Seuls les fichiers dont l’extension est configurée sont chiffrés.

Exemple :

```json
"ExtensionsToEncrypt": [
  ".txt"
]
```

Dans les logs :

```text
EncryptionTimeMs = 0   → aucun chiffrement
EncryptionTimeMs > 0   → chiffrement effectué
EncryptionTimeMs < 0   → erreur de chiffrement
```

---

### Logiciel métier bloquant

EasySave peut bloquer l’exécution si un logiciel métier est ouvert.

Exemple avec la calculatrice Windows :

```json
"BusinessSoftware": "CalculatorApp"
```

Pour trouver le nom du processus :

```powershell
Get-Process | Where-Object { $_.ProcessName -like "*calc*" -or $_.ProcessName -like "*calculator*" } | Select-Object ProcessName, Id
```

Si le logiciel métier est détecté, EasySave bloque la sauvegarde et ajoute une entrée dans les logs.

---

### Interface bilingue

La V2 permet de choisir la langue de l’interface :

```text
fr
en
```

La langue est sauvegardée dans :

```text
settings.json
```

---

## Fichiers générés

Les fichiers runtime sont générés dans le dossier d’exécution.

Pour la GUI :

```text
EasySave.GUI/bin/Debug/net10.0-windows/
```

Fichiers principaux :

| Fichier | Rôle |
|---|---|
| `jobs.config.json` | Liste des travaux |
| `settings.json` | Paramètres utilisateur |
| `state.json` | État d’exécution |
| `logs/` | Logs journaliers |
| `CryptoSoft/` | Exécutable de chiffrement copié en sortie |

---

## Exemple de jobs.config.json

```json
[
  {
    "Id": "4cec67d2-7c5e-4e81-986d-f976df470f2b",
    "Number": 1,
    "Name": "test1",
    "SourcePath": "C:\\Users\\astri\\Desktop\\A",
    "TargetPath": "C:\\Users\\astri\\Desktop\\I",
    "Type": 0,
    "LastExecutionTime": "2026-05-04T23:55:33.7322882+02:00"
  }
]
```

Interprétation du champ `Type` :

```text
0 = sauvegarde complète
1 = sauvegarde différentielle
```

---

## Exemple de settings.json

```json
{
  "LogFormat": 1,
  "Language": "fr",
  "ExtensionsToEncrypt": [
    ".txt"
  ],
  "BusinessSoftware": ""
}
```

Interprétation de `LogFormat` :

```text
0 = JSON
1 = XML
```

---

## Exemple de state.json

```json
[
  {
    "BackupName": "test1",
    "LastActionTime": "2026-05-04T23:55:33+02:00",
    "Status": "Terminé",
    "TotalFiles": 4,
    "TotalSize": 6614,
    "RemainingFiles": 0,
    "RemainingSize": 0,
    "Progression": 100,
    "CurrentSourceFile": "",
    "CurrentTargetFile": ""
  }
]
```

Statuts possibles :

```text
Actif
Terminé
Erreur
Interrompu
```

---

## Documentation

La documentation est disponible dans :

```text
EasySave/docs/
```

### Release notes

```text
EasySave/docs/release-notes/RELEASE_v1.1.0.md
EasySave/docs/release-notes/RELEASE_v1.1.0_EN.md
EasySave/docs/release-notes/RELEASE_v2.0.0.md
EasySave/docs/release-notes/RELEASE_v2.0.0_EN.md
```

### Manuels utilisateur

```text
EasySave/docs/user-manual/EasySave_User_Manual_v1.1.md
EasySave/docs/user-manual/EasySave_User_Manual_v1.1_EN.md
EasySave/docs/user-manual/EasySave_User_Manual_v2.0.md
EasySave/docs/user-manual/EasySave_User_Manual_v2.0_EN.md
```

### Support technique

```text
EasySave/docs/support/EasySave_Support_Info.md
EasySave/docs/support/EasySave_Support_Info_EN.md
EasySave/docs/support/EasySave_Support_Info_v2.0.md
EasySave/docs/support/EasySave_Support_Info_v2.0_EN.md
```

---

## Validation V2 effectuée

Les points suivants ont été validés :

- build complet ;
- interface graphique WPF ;
- architecture MVVM ;
- interface bilingue FR/EN ;
- création de plus de 5 travaux ;
- numérotation visible des travaux ;
- exécution individuelle ;
- exécution séquentielle ;
- message de fin d’exécution séquentielle ;
- génération de logs XML ;
- chiffrement CryptoSoft ;
- `EncryptionTimeMs > 0` sur les fichiers `.txt` ;
- suivi `state.json` ;
- statut textuel dans `state.json` ;
- progression à 100 % ;
- blocage par logiciel métier ;
- interface graphique améliorée.

---

## Limites connues

- CryptoSoft chiffre directement les fichiers copiés.
- Le nom du logiciel métier doit correspondre au nom réel du processus Windows.
- Certains messages internes restent techniques.
- L’interface pourra encore être enrichie dans une version future.
- Le packaging installable n’est pas encore fourni.

---

## Équipe

- Anelka MAPA
- Astride Gogan
- Luc Dai

---

## Licence

Projet académique réalisé dans le cadre du parcours CESI.

Projet développé pour ProSoft dans le cadre de la fiction pédagogique EasySave.